using Rms.Models.DataBase.Ams;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Services.Core.RabbitMq;
using SqlSugar;

namespace Rms.Services.Core.Services.AMS
{
    public class LockMachineJob : BackgroundService
    {
        private readonly ISqlSugarClient sqlSugarClient;
        private readonly RabbitMqService rabbitMqService;
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        private readonly System.Threading.Timer jobTimer;


        public LockMachineJob(ISqlSugarClient sqlSugarClient, RabbitMqService rabbitMqService)
        {
            this.sqlSugarClient = sqlSugarClient;
            this.rabbitMqService = rabbitMqService;
            jobTimer = new System.Threading.Timer(JobTransaction, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            jobTimer.Change(0, 10000);
            return Task.CompletedTask;
        }


        public class AMS_ALARM_RECORD_VM
        {
            public string ID { get; set; }
            public string ALID { get; set; }
            public string ALTX { get; set; }
            public DateTime ALTIME { get; set; }
            public bool ALARM_TRIGGERED { get; set; } = false;
            public string EQID { get; set; }
            public string EQUIPMENTTYPEID { get; set; }
        }

        private void JobTransaction(object? state)
        {
            try
            {
                var configs = sqlSugarClient.Queryable<AMS_CONFIGURATION>().Where(it => it.ACTIONID == "ContinuousAlarmLock").ToList();
                foreach (var config in configs)
                {

                    //var alarmRecords = sqlSugarClient.Queryable<AMS_ALARM_RECORD>().Where(it => it.ETID == config.EQUIPMENT_TYPE_ID && it.ALID == config.ALID && it.ALTIME > DateTime.Now.AddHours(12)).ToList();
                    sqlSugarClient.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        //我可以在这里面写逻辑

                        //技巧：AOP中获取IOC对象
                        //var serviceBuilder = services.BuildServiceProvider();
                        //var log= serviceBuilder.GetService<ILogger<WeatherForecastController>>();
                    };
                    var alarmRecords = sqlSugarClient.Queryable<AMS_ALARM_RECORD>()
                         .InnerJoin<RMS_EQUIPMENT>((aar, re) => aar.EQID == re.ID)
                         .Where((aar, re) => re.TYPE == config.EQUIPMENT_TYPE_ID && aar.ALID == config.ALID && aar.ALTIME > DateTime.Now.AddHours(-12))
                         .Select((aar, re) => new AMS_ALARM_RECORD_VM
                         {
                             ID = aar.ID,
                             ALID = aar.ALID,
                             ALTX = aar.ALTX,
                             ALTIME = aar.ALTIME,
                             ALARM_TRIGGERED = aar.ALARM_TRIGGERED,
                             EQID = aar.EQID,
                             EQUIPMENTTYPEID = re.TYPE
                         }).ToList();

                    var eqps = alarmRecords.Select(it => it.EQID).ToList();
                    foreach (var eqp in eqps)
                    {
                        try
                        {
                            var eqpAlarmRecords = alarmRecords.Where(it => it.EQID == eqp).OrderBy(it => it.ALTIME).ToList();
                            //如果config.TRIGGER_INTERVAL分钟内有超过config.TRIGGER_COUNT次报警，则锁机
                            if (eqpAlarmRecords.Count >= config.TRIGGER_COUNT)
                            {
                                for (int i = config.TRIGGER_COUNT - 1; i < eqpAlarmRecords.Count; i++)
                                {
                                    var compareTime = (config.TRIGGER_COUNT == 1) ? DateTime.MinValue : eqpAlarmRecords[i + 1 - config.TRIGGER_COUNT].ALTIME;
                                    var interval = (eqpAlarmRecords[i].ALTIME - compareTime).TotalMinutes;
                                    if (!eqpAlarmRecords[i].ALARM_TRIGGERED && interval < config.TRIGGER_INTERVAL)
                                    {
                                        Log.Info($"{eqp}在{interval}分钟内发生{config.TRIGGER_COUNT}次报警，锁机");
                                        string lockmessage = $"{DateTime.Now.ToString("yy/MM/dd HH:mm:ss")}: Machine Locked, {config.ALID} happened {config.TRIGGER_COUNT} times in {interval.ToString("2F")} minutes";
                                        var rabbitTrans = new RabbitMqTransaction()
                                        {
                                            TransactionName = "LockMachine",
                                            EquipmentID = eqp,
                                            NeedReply = false,
                                            Parameters = new Dictionary<string, object>() { { "Message", lockmessage } }
                                        };

                                        rabbitMqService.Produce($"EAP.SecsClient.{eqp}", rabbitTrans);
                                        //insert record to database
                                        sqlSugarClient.Insertable(new AMS_ACTION_RECORD()
                                        {
                                            CONFIG_ID = config.ID,
                                            EQID = eqp,
                                            ETID = config.EQUIPMENT_TYPE_ID,
                                            ALID = config.ALID,
                                            TRIGGER_INTERVAL = config.TRIGGER_INTERVAL,
                                            TRIGGER_COUNT = config.TRIGGER_COUNT,
                                            DATETIME = DateTime.Now,
                                            ISHANDLED = false
                                        }).ExecuteCommand();
                                        eqpAlarmRecords[i].ALARM_TRIGGERED = true;
                                        //update alarm record in database
                                        sqlSugarClient.Updateable<AMS_ALARM_RECORD>(eqpAlarmRecords[i]).UpdateColumns(it => new { it.ALARM_TRIGGERED }).ExecuteCommand();
                                        //update equipment in database
                                        sqlSugarClient.Updateable<RMS_EQUIPMENT>()
                                            .SetColumns(it => new RMS_EQUIPMENT { ISLOCKED = true, LOCKED_MESSAGE = lockmessage })
                                            .Where(it => it.ID == eqp).ExecuteCommand();
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error(e.ToString());
                            continue;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

    }
}

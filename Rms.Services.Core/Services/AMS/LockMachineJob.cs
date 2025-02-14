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

        private static readonly int interval = (int)TimeSpan.FromSeconds(5).TotalMilliseconds; // 设置固定的等待时间

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 设置定时器在固定的时间间隔后触发
            jobTimer.Change(interval, Timeout.Infinite);
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
        private static readonly object lockObject = new object();
        private static bool isRunning = false;
        private void JobTransaction(object? state)
        {
            // 使用锁来确保线程安全
            lock (lockObject)
            {
                // 如果任务已经在运行，则直接返回
                if (isRunning)
                {
                    return;
                }
                // 标记任务为正在运行
                isRunning = true;
            }
            try
            {
                var configs = sqlSugarClient.Queryable<AMS_CONFIGURATION>().Where(it => it.ACTIONID == "ContinuousAlarmLock").ToList();
                var fromTime = DateTime.Now.AddHours(-12);
                var allAlarmRecords = sqlSugarClient.Queryable<AMS_ALARM_RECORD>().Where(it => it.ALTIME > fromTime).ToList();
                var equipments = sqlSugarClient.Queryable<RMS_EQUIPMENT>().ToList();
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
                    //var alarmRecords = sqlSugarClient.Queryable<AMS_ALARM_RECORD>()
                    //     .InnerJoin<RMS_EQUIPMENT>((aar, re) => aar.EQID == re.ID)
                    //     .Where((aar, re) => re.TYPE == config.EQUIPMENT_TYPE_ID &&aar.ALTX == config.ALTX && aar.ALID == config.ALID  && aar.ALTIME > DateTime.Now.AddHours(-12))
                    //     .Select((aar, re) => new AMS_ALARM_RECORD_VM
                    //     {
                    //         ID = aar.ID,
                    //         ALID = aar.ALID,
                    //         ALTX = aar.ALTX,
                    //         ALTIME = aar.ALTIME,
                    //         ALARM_TRIGGERED = aar.ALARM_TRIGGERED,
                    //         EQID = aar.EQID,
                    //         EQUIPMENTTYPEID = re.TYPE
                    //     }).ToList();
                    var alarmRecords = allAlarmRecords
    .Join(
        equipments, // 第二个集合
        aar => aar.EQID, // 第一个集合的键选择器
        re => re.ID, // 第二个集合的键选择器
        (aar, re) => new { aar, re } // 合并结果的选择器
    )
    .Where(joined =>
        joined.re.TYPE == config.EQUIPMENT_TYPE_ID &&
        joined.aar.ALTX.Contains(config.ALTX) &&
        joined.aar.ALID == config.ALID &&
        joined.aar.ALTIME > DateTime.Now.AddHours(-12)
    )
    .Select(joined => new AMS_ALARM_RECORD_VM
    {
        ID = joined.aar.ID,
        ALID = joined.aar.ALID,
        ALTX = joined.aar.ALTX,
        ALTIME = joined.aar.ALTIME,
        ALARM_TRIGGERED = joined.aar.ALARM_TRIGGERED,
        EQID = joined.aar.EQID,
        EQUIPMENTTYPEID = joined.re.TYPE
    })
    .ToList();


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
                                    var compareTime = (config.TRIGGER_COUNT == 1) ? eqpAlarmRecords[i].ALTIME : eqpAlarmRecords[i + 1 - config.TRIGGER_COUNT].ALTIME;
                                    var interval = (eqpAlarmRecords[i].ALTIME - compareTime).TotalMinutes;
                                    if (!eqpAlarmRecords[i].ALARM_TRIGGERED && interval < config.TRIGGER_INTERVAL)
                                    {
                                        Log.Info($"{eqp}在{interval}分钟内发生{config.TRIGGER_COUNT}次报警，锁机");
                                        string lockmessage = $"{DateTime.Now.ToString("yy/MM/dd HH:mm:ss")}: Machine Locked, {config.ALID} happened {config.TRIGGER_COUNT} times in {interval.ToString("F2")} minutes";
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
                                            ALTX = eqpAlarmRecords.First().ALTX,
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
            finally
            {
                isRunning = false;
            }
            jobTimer.Change(interval, Timeout.Infinite);
        }

    }
}

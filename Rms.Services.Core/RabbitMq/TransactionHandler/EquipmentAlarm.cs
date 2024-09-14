using log4net;
using Rms.Models.DataBase.Ams;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using SqlSugar;

namespace Rms.Services.Core.RabbitMq.TransactionHandler
{
    public class EquipmentAlarm : ITransactionHandler
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        public void HandleTransaction(RabbitMqTransaction trans, RabbitMqService rabbitMq, ISqlSugarClient sqlSugarClient)
        {
            try
            {
                var AlarmEqp = string.Empty;
                var AlarmCode = string.Empty;
                var AlarmText = string.Empty;
                var AlarmSource = string.Empty;
                var AlarmTime = DateTime.Now;
                var AlarmSet = false;
                if (trans.Parameters.TryGetValue("AlarmEqp", out object _alarmeqp)) AlarmEqp = _alarmeqp?.ToString();
                if (trans.Parameters.TryGetValue("AlarmCode", out object _alarmcode)) AlarmCode = _alarmcode?.ToString();
                if (trans.Parameters.TryGetValue("AlarmText", out object _alarmtext)) AlarmText = _alarmtext?.ToString();
                if (trans.Parameters.TryGetValue("AlarmSource", out object _alarmsource)) AlarmSource = _alarmsource?.ToString();
                if (trans.Parameters.TryGetValue("AlarmTime", out object _alarmtime)) AlarmTime = (DateTime)_alarmtime;
                if (trans.Parameters.TryGetValue("AlarmSet", out object _alarmset)) AlarmSet = (bool)_alarmset;
                if (AlarmSet)
                {
                    sqlSugarClient.Insertable<AMS_ALARM_RECORD>(new AMS_ALARM_RECORD()
                    {
                        EQID = AlarmEqp,
                        ALID = AlarmCode,
                        ALTIME = AlarmTime,
                        ALTX = AlarmText,
                    }).ExecuteCommand();
                }


            }
            catch (Exception ex)
            {

            }
        }
    }
}

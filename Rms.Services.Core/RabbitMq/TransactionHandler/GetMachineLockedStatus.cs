using log4net;
using Rms.Models.DataBase.Ams;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Services.Core.Rms;
using SqlSugar;

namespace Rms.Services.Core.RabbitMq.TransactionHandler
{
    public class GetMachineLockedStatus : ITransactionHandler
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        public void HandleTransaction(RabbitMqTransaction trans, RabbitMqService rabbitMq, ISqlSugarClient sqlSugarClient)
        {
            var repTrans = trans.GetReplyTransaction();
            try
            {
                var EquipmentId = string.Empty;
                //var RecipeName = string.Empty;         
                if (trans.Parameters.TryGetValue("EquipmentId", out object _equipmentId)) EquipmentId = _equipmentId?.ToString();
                //if (trans.Parameters.TryGetValue("RecipeName", out object _recipeName)) RecipeName = _recipeName?.ToString();

                var eqp = sqlSugarClient.Queryable<RMS_EQUIPMENT>().InSingle(EquipmentId);
                if (eqp != null)
                {
                    repTrans.Parameters.Add("Result", true);
                    repTrans.Parameters.Add("IsLocked", eqp.ISLOCKED);
                    repTrans.Parameters.Add("Message", eqp.LOCKED_MESSAGE);
                }
                else
                {
                    repTrans.Parameters.Add("Result", false);
                    repTrans.Parameters.Add("Message", $"Can not find machine {EquipmentId}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                repTrans.Parameters.Add("Result", false);
                repTrans.Parameters.Add("Message", $"RMS Service error:{ex.ToString()}");
            }
            rabbitMq.Produce(trans.ReplyChannel, repTrans);
        }
    }
}

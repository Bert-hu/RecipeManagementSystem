using log4net;
using Rms.Models.DataBase.Ams;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Services.Core.Rms;
using SqlSugar;

namespace Rms.Services.Core.RabbitMq.TransactionHandler
{
    public class CompareRecipeBody : ITransactionHandler
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        public void HandleTransaction(RabbitMqTransaction trans, RabbitMqService rabbitMq, ISqlSugarClient sqlSugarClient)
        {
            var repTrans = trans.GetReplyTransaction();
            try
            {
                var EquipmentId = string.Empty;
                var RecipeName = string.Empty;         
                if (trans.Parameters.TryGetValue("EquipmentId", out object _equipmentId)) EquipmentId = _equipmentId?.ToString();
                if (trans.Parameters.TryGetValue("RecipeName", out object _recipeName)) RecipeName = _recipeName?.ToString();

                var recipe = sqlSugarClient.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == EquipmentId && it.NAME == RecipeName).First();
                if (recipe == null)
                {
                    repTrans.Parameters.Add("Result", false);
                    repTrans.Parameters.Add("Message", $"Recipe does not exist in RMS");
                }
                else
                {
                    var recipe_version = sqlSugarClient.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_EFFECTIVE_ID).First();
                    var eqp = sqlSugarClient.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
                    RmsTransactionService service = new RmsTransactionService(sqlSugarClient, rabbitMq);
                    (var result, var message) = service.CompareRecipe(eqp, recipe_version.ID);
                    repTrans.Parameters.Add("Result", result);
                    repTrans.Parameters.Add("Message", message);
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

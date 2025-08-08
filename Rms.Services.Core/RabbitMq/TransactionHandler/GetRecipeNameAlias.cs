using log4net;
using Rms.Models.DataBase.Ams;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Services.Core.Rms;
using SqlSugar;

namespace Rms.Services.Core.RabbitMq.TransactionHandler
{
    public class GetRecipeNameAlias : ITransactionHandler
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        public void HandleTransaction(RabbitMqTransaction trans, RabbitMqService rabbitMq, ISqlSugarClient sqlSugarClient)
        {
            var repTrans = trans.GetReplyTransaction();
            try
            {
                string message = string.Empty;
                bool result = false;
                var EquipmentTypeId = string.Empty;
                var RecipeName = string.Empty;

                if (trans.Parameters.TryGetValue("EquipmentTypeId", out object _equipmentTypeId)) EquipmentTypeId = _equipmentTypeId?.ToString();
                if (trans.Parameters.TryGetValue("RecipeName", out object _recipeName)) RecipeName = _recipeName?.ToString();

                var config = sqlSugarClient.Queryable<RMS_RECIPE_NAME_ALIAS>().First(it => it.EQUIPMENT_TYPE_ID == EquipmentTypeId && it.RECIPE_NAME == RecipeName);


                if (config != null)
                {
                    result =true;
                    repTrans.Parameters.Add("EquipmentTypeId", config.EQUIPMENT_TYPE_ID);
                    repTrans.Parameters.Add("RecipeName",config.RECIPE_NAME);
                    repTrans.Parameters.Add("RecipeAlias", config.RECIPE_ALIAS);
                }
                else
                {
                    message = $"'{EquipmentTypeId}' can not find RecipeAlias related to '{RecipeName}'";
                }
                repTrans.Parameters.Add("Result", result);
                repTrans.Parameters.Add("Message", message);
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

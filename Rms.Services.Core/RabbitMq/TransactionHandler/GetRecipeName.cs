using log4net;
using Rms.Models.DataBase.Ams;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Services.Core.Rms;
using SqlSugar;

namespace Rms.Services.Core.RabbitMq.TransactionHandler
{
    public class GetRecipeName : ITransactionHandler
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        public void HandleTransaction(RabbitMqTransaction trans, RabbitMqService rabbitMq, ISqlSugarClient sqlSugarClient)
        {
            var repTrans = trans.GetReplyTransaction();
            try
            {
                string message = string.Empty;
                bool result = false;
                var EquipmentId = trans.EquipmentID;
                var EquipmentTypeId = string.Empty;
                var RecipeNameAlias = string.Empty;

                if (trans.Parameters.TryGetValue("EquipmentTypeId", out object _equipmentTypeId)) EquipmentTypeId = _equipmentTypeId?.ToString();
                if (trans.Parameters.TryGetValue("RecipeNameAlias", out object _recipeNameAlias)) RecipeNameAlias = _recipeNameAlias?.ToString();
                var configs = sqlSugarClient.Queryable<RMS_RECIPE_NAME_ALIAS>().Where(it => it.EQUIPMENT_TYPE_ID == EquipmentTypeId).ToList();
                RMS_RECIPE_NAME_ALIAS? config = null;
                config = configs.FirstOrDefault(it => it.EQUIPMENT_TYPE_ID == EquipmentTypeId && it.RECIPE_ALIAS.Contains(RecipeNameAlias));

                if (config != null)
                {
                    result =true;
                    repTrans.Parameters.Add("RecipeName",config.RECIPE_NAME);
                }
                else
                {
                    message = $"'{EquipmentTypeId}' can not find Recipe related to '{RecipeNameAlias}'";
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

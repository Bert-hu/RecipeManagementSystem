using log4net;
using Rms.Models.DataBase.Ams;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Services.Core.Rms;
using SqlSugar;

namespace Rms.Services.Core.RabbitMq.TransactionHandler
{
    public class ReloadRecipeBodyToEffectiveVersion : ITransactionHandler
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        public void HandleTransaction(RabbitMqTransaction trans, RabbitMqService rabbitMq, ISqlSugarClient sqlSugarClient)
        {
            var repTrans = trans.GetReplyTransaction();
            try
            {
                var EquipmentId = trans.EquipmentID;
                var RecipeName = string.Empty;
                if (trans.Parameters.TryGetValue("RecipeName", out object _recipeName)) RecipeName = _recipeName?.ToString();
                var eqp = sqlSugarClient.Queryable<RMS_EQUIPMENT>().In(EquipmentId).First();
                var recipe = sqlSugarClient.Queryable<RMS_RECIPE>().First(it => it.EQUIPMENT_ID == EquipmentId && it.NAME == RecipeName);
                if (recipe == null || recipe.VERSION_EFFECTIVE_ID == null)
                {
                    repTrans.Parameters.Add("Result", false);
                    repTrans.Parameters.Add("Message", $"Effective Recipe does not exist in RMS");
                }
                else
                {
                    var recipeVersion = sqlSugarClient.Queryable<RMS_RECIPE_VERSION>().InSingle(recipe.VERSION_EFFECTIVE_ID);
                    RmsTransactionService service = new RmsTransactionService(sqlSugarClient, rabbitMq);
                    (bool result, string message, byte[]? body) = service.UploadRecipeToServer(eqp, RecipeName);
                    if (result)
                    {
                        sqlSugarClient.Ado.BeginTran();
                        try
                        {
                            if (!string.IsNullOrEmpty(recipeVersion.RECIPE_DATA_ID))
                            {
                                sqlSugarClient.Deleteable<RMS_RECIPE_DATA>().In(recipeVersion.RECIPE_DATA_ID).ExecuteCommand();
                            }
                            var data = new RMS_RECIPE_DATA
                            {
                                NAME = RecipeName,
                                CONTENT = body,
                                CREATOR = trans.ReplyChannel,
                            };
                            sqlSugarClient.Insertable<RMS_RECIPE_DATA>(data).ExecuteCommand();
                            recipeVersion.RECIPE_DATA_ID = data.ID;
                            //判断recipeVersion.REMARK行数是否大于10如果大于10，取第1行和后4行
                            if (recipeVersion.REMARK.Split("\r\n".ToArray()).Length > 10)
                            {
                                var newremark = string.Empty;
                                newremark += recipeVersion.REMARK.Split("\r\n".ToArray())[0];
                                newremark += "\r\n...";
                                newremark += recipeVersion.REMARK.Split("\r\n".ToArray())[recipeVersion.REMARK.Split("\r\n".ToArray()).Length - 4];
                                newremark += recipeVersion.REMARK.Split("\r\n".ToArray())[recipeVersion.REMARK.Split("\r\n".ToArray()).Length - 3];
                                newremark += recipeVersion.REMARK.Split("\r\n".ToArray())[recipeVersion.REMARK.Split("\r\n".ToArray()).Length - 2];
                                newremark += recipeVersion.REMARK.Split("\r\n".ToArray())[recipeVersion.REMARK.Split("\r\n".ToArray()).Length - 1];
                                recipeVersion.REMARK = newremark;
                            }
                            recipeVersion.REMARK += $"\r\n{DateTime.Now} update recipe body without upgrade version";

                            sqlSugarClient.Updateable<RMS_RECIPE_VERSION>(recipeVersion).UpdateColumns(it => new { it.RECIPE_DATA_ID, it.REMARK }).ExecuteCommand();
                            sqlSugarClient.Ado.CommitTran();
                            repTrans.Parameters.Add("Result", true);
                        }
                        catch (Exception ex)
                        {
                            sqlSugarClient.Ado.RollbackTran();
                            Log.Error(ex.Message, ex);
                            repTrans.Parameters.Add("Result", false);
                            repTrans.Parameters.Add("Message", $"RMS Service Error:{ex.Message}");
                        }
                    }
                    else
                    {
                        repTrans.Parameters.Add("Result", false);
                        repTrans.Parameters.Add("Message", $"Get recipe body fail:{message}");
                    }
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

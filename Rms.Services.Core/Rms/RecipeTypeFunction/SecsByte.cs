using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Services.Core.RabbitMq;
using SqlSugar;
using System.Text;

namespace Rms.Services.Core.Rms.RecipeTypeFunction
{
    internal class SecsByte : IRecipeType
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        protected ISqlSugarClient db;
        protected RabbitMqService rabbitMq;

        public SecsByte(ISqlSugarClient _db, RabbitMqService _rabbitMq)
        {
            db = _db;
            rabbitMq = _rabbitMq;
        }

        public (bool result, string message) DeleteAllMachineRecipes(string EquipmentId)
        {
            try
            {
                string rabbitmqroute = $"EAP.SecsClient.{EquipmentId}";

                var trans = new RabbitMqTransaction
                {
                    TransactionName = "DeleteAllRecipes",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ExpireSecond = 30
                };
                var rabbitres = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);
                if (rabbitres != null)
                {
                    var result = rabbitres.Parameters["Result"].ToString().ToUpper() == "TRUE";
                    rabbitres.Parameters.TryGetValue("Message", out object message);
                    return (result, message?.ToString());
                }
                else//Rabbit Mq失败
                {
                    Log.Error($"Delete all recipe Time out!");
                    return (false, "Equipment offline or EAP client error!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, "Rms Service Error");
            }
        }

        public (bool result, string message) DownloadRecipeToMachine(string EquipmentId, string RecipeVersionId)
        {
            try
            {
                var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().InSingle(RecipeVersionId);
                if (recipe_version.RECIPE_DATA_ID == null)
                {
                    return (false, "Recipe do not have data.");
                }
                var eqp = db.Queryable<RMS_EQUIPMENT>().InSingle(EquipmentId);
                var eqtpye = db.Queryable<RMS_EQUIPMENT_TYPE>().InSingle(eqp.TYPE);
                if (eqtpye.DELETEBEFOREDOWNLOAD)
                {
                    DeleteAllMachineRecipes(EquipmentId);//暂时不处理删除的答复
                }
                var recipe = db.Queryable<RMS_RECIPE>().InSingle(recipe_version.RECIPE_ID);
                var data = db.Queryable<RMS_RECIPE_DATA>().InSingle(recipe_version.RECIPE_DATA_ID)?.CONTENT;
                string rabbitMqRoute = $"EAP.SecsClient.{EquipmentId}";
                var body = Convert.ToBase64String(data);
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "SetUnformattedRecipe",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ExpireSecond = 60,
                    Parameters = new Dictionary<string, object>() { { "RecipeName", recipe.NAME }, { "RecipeBody", body } }
                };
                var rabbitRes = rabbitMq.ProduceWaitReply(rabbitMqRoute, trans);
                if (rabbitRes != null)
                {
                    var result = rabbitRes.Parameters["Result"].ToString().ToUpper() == "TRUE";
                    rabbitRes.Parameters.TryGetValue("Message", out object message);
                    return (result, message?.ToString());
                }
                else//Rabbit Mq失败
                {
                    Log.Error($"Download recipe Time out!");
                    return (false, "Equipment offline or EAP client error!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, "Rms Service Error");
            }
        }

        public (bool result, string message, List<string> eppd) GetEppd(string EquipmentId)
        {
            try
            {
                var rabbitmqroute = $"EAP.SecsClient.{EquipmentId}";
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "GetEPPD",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ExpireSecond = 5
                };
                var rabbitres = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);
                if (rabbitres != null)
                {
                    if (rabbitres.Parameters.TryGetValue("EPPD", out object eppdstr))
                    {
                        var eppd = JsonConvert.DeserializeObject<List<string>>(eppdstr.ToString());

                        return (true, string.Empty, eppd);
                    }
                    else
                    {
                        rabbitres.Parameters.TryGetValue("Message", out object message);
                        return (false, message?.ToString(), null);
                    }
                }
                else//Rabbit Mq失败
                {
                    Log.Debug($"Rabbit Mq send to {rabbitmqroute}\n:{JsonConvert.SerializeObject(trans, Formatting.Indented)}");
                    Log.Error($"GetEPPD Time out!");
                    return (false, "EAP Reply Timeout", null);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, "Rms Service Error", null);
            }
        }

        public (bool result, string message, byte[] body) UploadRecipeToServer(string EquipmentId, string RecipeName)
        {
            string rabbitmqRoute = $"EAP.SecsClient.{EquipmentId}";
            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetUnformattedRecipe",
                EquipmentID = EquipmentId,
                NeedReply = true,
                ExpireSecond = 60,
                Parameters = new Dictionary<string, object>() { { "RecipeName", RecipeName } }
            };
            var rabbitRes = rabbitMq.ProduceWaitReply(rabbitmqRoute, trans);
           if (rabbitRes != null)
            {
                var result = rabbitRes.Parameters["Result"].ToString().ToUpper() == "TRUE";
                rabbitRes.Parameters.TryGetValue("Message", out object message);
                if (result)
                {
                    var body = Convert.FromBase64String(rabbitRes.Parameters["RecipeBody"].ToString());
                    return (result, message?.ToString(), body);
                }
                else
                {
                    return (result, message?.ToString(), null);
                }

            }
            else//Rabbit Mq失败
            {
                Log.Error($"Get Recipe Body Time out!");
                return (false, "EAP Reply Timeout", null);
            }

        }
        public (bool result, string message) CompareRecipe(string EquipmentId, string RecipeVersionId)
        {
            try
            {
                var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().InSingle(RecipeVersionId);
                if (recipe_version.RECIPE_DATA_ID == null)
                {
                    return (false, "Recipe do not have data.");
                }
                var recipe = db.Queryable<RMS_RECIPE>().InSingle(recipe_version.RECIPE_ID);
                var data = db.Queryable<RMS_RECIPE_DATA>().InSingle(recipe_version.RECIPE_DATA_ID)?.CONTENT;
                string rabbitMqRoute = $"EAP.SecsClient.{EquipmentId}";
                var body = Convert.ToBase64String(data);

                var trans = new RabbitMqTransaction
                {
                    TransactionName = "CompareRecipe",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ExpireSecond = 15,
                    Parameters = new Dictionary<string, object>() { { "RecipeName", recipe.NAME }, { "RecipeBody", body } }
                };
                var rabbitRes = rabbitMq.ProduceWaitReply(rabbitMqRoute, trans);
                if (rabbitRes != null)
                {
                    var result = rabbitRes.Parameters["Result"].ToString().ToUpper() == "TRUE";
                    rabbitRes.Parameters.TryGetValue("Message", out object message);
                    return (result, message?.ToString());
                }
                else//Rabbit Mq失败
                {
                    Log.Error($"Compare recipe Time out!");
                    return (false, "Equipment offline or EAP client error!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, "Rms Service Error");
            }
        }
    }
}

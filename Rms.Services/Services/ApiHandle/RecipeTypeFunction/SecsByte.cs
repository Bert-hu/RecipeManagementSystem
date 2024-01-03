using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Utils;
using RMS.Domain.Rms;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Services.ApiHandle.RecipeTypeFunction
{
    internal class SecsByte : IRecipeType
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        SqlSugarClient db = DbFactory.GetSqlSugarClient();
        public (bool result, string message) DeleteAllMachineRecipes(string EquipmentId)
        {
            try
            {
                string rabbitmqroute = $"EAP.SecsClient.{EquipmentId}";

                var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "DeleteAllRecipes",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ReplyChannel = ListenChannel,
                };
                var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 30);
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
                var listenChannel = ConfigurationManager.AppSettings["ListenChannel"];
                var body = Convert.ToBase64String(data);
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "SetUnformattedRecipe",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ReplyChannel = listenChannel,
                    Parameters = new Dictionary<string, object>() { { "RecipeName", recipe.NAME }, { "RecipeBody", body } }
                };
                var rabbitRes = RabbitMqService.ProduceWaitReply(rabbitMqRoute, trans, 60);
                if (rabbitRes != null)
                {
                    var result = rabbitRes.Parameters["Result"].ToString().ToUpper() == "TRUE";
                    rabbitRes.Parameters.TryGetValue("Message", out object message);
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

        public (bool result, string message, List<string> eppd) GetEppd(string EquipmentId)
        {
            try
            {
                var rabbitmqroute = $"EAP.SecsClient.{EquipmentId}";
                var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "GetEPPD",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ReplyChannel = ListenChannel,
                };
                var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 5);
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
                Parameters = new Dictionary<string, object>() { { "RecipeName", RecipeName } }
            };
            var rabbitRes = RabbitMqService.ProduceWaitReply(rabbitmqRoute, trans, 60);
            if (rabbitRes != null)
            {
                var result = rabbitRes.Parameters["Result"].ToString().ToUpper() == "TRUE";
                rabbitRes.Parameters.TryGetValue("Message", out object message);
                var body = Convert.FromBase64String(rabbitRes.Parameters["RecipeBody"].ToString());
                return (result, message?.ToString(), body);
            }
            else//Rabbit Mq失败
            {
                Log.Error($"Get Recipe Body Time out!");
                return (false, "EAP Reply Timeout", null);
            }

        }
    }
}

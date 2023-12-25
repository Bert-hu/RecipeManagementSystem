using Newtonsoft.Json;
using RMS.Domain.Rms;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rms.Models.RabbitMq;
using System.Configuration;

namespace Rms.Services.Services.ApiHandle
{
    public partial class ApiMessageHandler
    {
        public ResponseMessage DownloadEffectiveRecipeToMachine(string jsonContent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new DownloadEffectiveRecipeToMachineResponse();
            var req = JsonConvert.DeserializeObject<DownloadEffectiveRecipeToMachineRequest>(jsonContent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            var eqType = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            if (eqp == null)
            {
                res.Result = false;
                res.Message = "Equipment does not exist in RMS";
                return res;
            }
            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == req.EquipmentId && it.NAME == req.RecipeName).First();
            if (recipe == null)
            {
                res.Result = false;
                res.Message = "Recipe does not exist in RMS";
                return res;
            }
            var recipeVersion = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_EFFECTIVE_ID).First();
            if (recipeVersion.RECIPE_DATA_ID == null)
            {
                res.Result = false;
                res.Message = "RMS ERROR! Effective version does not have recipe content!";
                return res;
            }
            if (eqType.DELETEBEFOREDOWNLOAD)
            {
                var deleteRes = DeleteAllRecipes(eqp.RECIPE_TYPE, eqp.ID);
                // 暂时不处理删除的答复
            }
            var serverData = db.Queryable<RMS_RECIPE_DATA>().In(recipeVersion.RECIPE_DATA_ID)?.First()?.CONTENT;
            var rabbitRes = SetSecsRecipe(eqp.RECIPE_TYPE, eqp.ID, recipe.NAME, serverData);
            bool isOffline = false;
            if (rabbitRes != null)
            {
                if (rabbitRes.Parameters.TryGetValue("Status", out object status))
                {
                    isOffline = status.ToString().ToUpper() == "OFFLINE";
                }
                if (isOffline)
                {
                    res.Result = false;
                    res.Message = "Equipment offline!";
                    return res;
                }
                if (rabbitRes.Parameters["Result"].ToString().ToUpper() != "TRUE")
                {
                    res.Message = rabbitRes.Parameters["Message"].ToString();
                }
                else
                {
                    res.Result = true;
                    res.RecipeName = recipe.NAME;
                    db.Insertable<RMS_CHANGE_RECORD>(new RMS_CHANGE_RECORD
                    {
                        EQID = req.EquipmentId,
                        TO_RECIPE_NAME = req.RecipeName,
                        TO_RECIPE_VERSION = recipeVersion.VERSION?.ToString(),
                        CREATOR = req.TrueName,
                        CREATETIME = DateTime.Now
                    }).ExecuteCommand();
                    //PPSelect(eqp.ID, recipe.NAME); // 不管回复了
                }
            }
            else
            {
                res.Message = "Equipment offline or EAP client error!";
            }
            return res;
        }

        public RabbitMqTransaction SetSecsRecipe(string recipeType, string equipmentID, string recipeName, byte[] serverData)
        {
            string rabbitMqRoute;
            string transName;
            string body;

            switch (recipeType)
            {
                case "secsByte":
                    rabbitMqRoute = $"EAP.SecsClient.{equipmentID}";
                    transName = "SetUnfomattedRecipe";
                    body = Convert.ToBase64String(serverData);
                    break;
                case "secsSml":
                    rabbitMqRoute = $"EAP.SecsClient.{equipmentID}";
                    transName = "SetFormattedRecipe";
                    body = System.Text.Encoding.Unicode.GetString(serverData);
                    break;
                default:
                    rabbitMqRoute = $"EAP.SecsClient.{equipmentID}";
                    transName = "SetUnfomattedRecipe";
                    body = Convert.ToBase64String(serverData);
                    break;
            }

            var listenChannel = ConfigurationManager.AppSettings["ListenChannel"];
            var trans = new RabbitMqTransaction
            {
                TransactionName = transName,
                EquipmentID = equipmentID,
                NeedReply = true,
                ReplyChannel = listenChannel,
                Parameters = new Dictionary<string, object>() { { "RecipeName", recipeName }, { "RecipeBody", body } }
            };

            var rabbitRes = RabbitMqService.ProduceWaitReply(rabbitMqRoute, trans, 120);

            return rabbitRes;
        }

        public RabbitMqTransaction PPSelect(string EquipmentID, string RecipeName)
        {
            string rabbitmqroute = rabbitmqroute = $"EAP.SecsClient.{EquipmentID}";
            //识别设备类型和恢复queue
            string body = string.Empty;

            var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
            var trans = new RabbitMqTransaction
            {
                TransactionName = "PpSelect",
                EquipmentID = EquipmentID,
                NeedReply = true,
                ReplyChannel = ListenChannel,
                Parameters = new Dictionary<string, object>() { { "RecipeName", RecipeName } }
            };
            var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 1);

            return rabbitres;
        }

        public RabbitMqTransaction DeleteAllRecipes(string RecipeType, string EquipmentID)
        {
            string rabbitmqroute = string.Empty;

            switch (RecipeType)
            {
                default:
                    rabbitmqroute = $"EAP.SecsClient.{EquipmentID}";
                    break;
            }
            var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
            var trans = new RabbitMqTransaction
            {
                TransactionName = "DeleteAllRecipes",
                EquipmentID = EquipmentID,
                NeedReply = true,
                ReplyChannel = ListenChannel,
            };
            var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 30);
            return rabbitres;
        }
    }
}

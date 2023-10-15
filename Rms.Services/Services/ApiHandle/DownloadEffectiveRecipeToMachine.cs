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
        public ResponseMessage DownloadEffectiveRecipeToMachine(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new DownloadEffectiveRecipeToMachineResponse();
            var req = JsonConvert.DeserializeObject<DownloadEffectiveRecipeToMachineRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            if (eqp == null)
            {
                res.Result = false;
                res.Message = "Equipment does not exist in RMS";
                return res;
            }
            var recipe = db.Queryable<RMS_RECIPE>().Where(it =>it.EQUIPMENT_ID ==req.EquipmentId && it.NAME ==req.RecipeName).First();
            if (recipe == null)
            {
                res.Result = false;
                res.Message = "Recipe does not exist in RMS";
                return res;
            }
            var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_EFFECTIVE_ID).First();

            if (recipe_version.RECIPE_DATA_ID == null)
            {
                res.Result = false;
                res.Message = "RMS ERROR! Effective version do not have recipe content!";
                return res;
            }

            var serverdata = db.Queryable<RMS_RECIPE_DATA>().In(recipe_version.RECIPE_DATA_ID)?.First()?.CONTENT;

            var rabbitRes = SetUnfomattedRecipe(eqp.RECIPE_TYPE, eqp.ID, recipe.NAME, serverdata);


            #region 返回是否是离线
            bool isOffline = false;
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
            #endregion
            if (rabbitRes != null)
            {
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
                        TO_RECIPE_VERSION = recipe_version.VERSION?.ToString(),
                        CREATOR = req.TrueName,
                        CREATETIME = DateTime.Now
                    }).ExecuteCommand();

                    PPSelect(eqp.ID, recipe.NAME);//不管回复了
                }


            }
            else//Rabbit Mq失败
            {
                res.Message = "Equipment offline or EAP client error!";
            }


            return res;
        }


        public RabbitMqTransaction SetUnfomattedRecipe(string RecipeType, string EquipmentID, string RecipeName, byte[] serverdata)
        {
            string rabbitmqroute = string.Empty;
            //识别设备类型和恢复queue
            string body = string.Empty;
            switch (RecipeType)
            {
               case "secsByte":
                    body = Convert.ToBase64String(serverdata);
                    rabbitmqroute = $"EAP.SecsClient.{EquipmentID}";
                    break;
                default:
                    body = Convert.ToBase64String(serverdata);
                    rabbitmqroute = $"EAP.SecsClient.{EquipmentID}";
                    break;
            }
            var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
            var trans = new RabbitMqTransaction
            {
                TransactionName = "SetUnfomattedRecipe",
                EquipmentID = EquipmentID,
                NeedReply = true,
                ReplyChannel = ListenChannel,
                Parameters = new Dictionary<string, object>() { { "RecipeName", RecipeName }, { "RecipeBody", body } }
            };
            var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 5);

            return rabbitres;
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
    }
}

using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;
using RMS.Domain.Rms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rms.Services.Services.ApiHandle
{
    public partial class ApiMessageHandler
    {
        /// <summary>
        /// 新建Recipe
        /// </summary>
        /// <param name="jsoncontent"></param>
        /// <returns></returns>
        public ResponseMessage AddNewRecipe(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new AddNewRecipeResponse();
            var req = JsonConvert.DeserializeObject<AddNewRecipeRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            if (eqp == null)
            {
                res.Message = $"Euipment '{req.EquipmentId}' not exists!";
                return res;
            }
            //检查同名recipe
            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == req.EquipmentId && it.NAME == req.RecipeName)?.First();
            if (recipe != null)//检查是否存在同名
            {
                res.Message = $"Recipe '{req.RecipeName}' already exists!";
                return res;
            }

            switch (eqp.RECIPE_TYPE)
            {
                case "secsByte":
                case "secsSml":
                    return AddSecsRecipeTransaction(eqp, req);                
                case "onlyName":
                    return AddOnlyNameRecipeTransaction(eqp, req);
                default:
                    return AddSecsRecipeTransaction(eqp, req);
            }

        }

        private AddNewRecipeResponse AddSecsRecipeTransaction(RMS_EQUIPMENT eqp, AddNewRecipeRequest req)
        {
            var res = new AddNewRecipeResponse();

            var rabbitRes = GetSecsRecipe(eqp.RECIPE_TYPE, req.EquipmentId, req.RecipeName);

            if (rabbitRes == null)
            {
                res.Message = "Equipment offline or EAP client error!";
                return res;
            }

            if (rabbitRes.Parameters["Result"] is string result && result.ToUpper() != "TRUE")
            {
                res.Result = false;
                res.Message = rabbitRes.Parameters["Message"].ToString();
                return res;
            }

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

            if (!IsDebugMode && rabbitRes.Parameters["RecipeName"].ToString() != req.RecipeName)
            {
                res.Message = $"The RecipeName of the response({rabbitRes.Parameters["RecipeName"].ToString()}) is inconsistent with the request({req.RecipeName})";
                return res;
            }

            byte[] body;
            switch (eqp.RECIPE_TYPE)
            {
                case "secsByte":
                    body = Convert.FromBase64String(rabbitRes.Parameters["RecipeBody"].ToString());
                    break;
                case "secsSml":
                    body = System.Text.Encoding.Unicode.GetBytes(rabbitRes.Parameters["RecipeBody"].ToString());
                    break;
                default:
                    body = Convert.FromBase64String(rabbitRes.Parameters["RecipeBody"].ToString());
                    break;
            }

            using (var db = DbFactory.GetSqlSugarClient())
            {
                db.BeginTran();

                try
                {
                    var recipe = new RMS_RECIPE
                    {
                        EQUIPMENT_ID = req.EquipmentId,
                        NAME = req.RecipeName
                    };
                    db.Insertable<RMS_RECIPE>(recipe).ExecuteCommand();

                    var eqtype = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();

                    var version = new RMS_RECIPE_VERSION
                    {
                        RECIPE_ID = recipe.ID,
                        _FLOW_ROLES = eqtype.FLOWROLEIDS,
                        CURRENT_FLOW_INDEX = 100,
                        REMARK = "First Version",
                        CREATOR = req.TrueName
                    };
                    db.Insertable<RMS_RECIPE_VERSION>(version).ExecuteCommand();

                    var data = new RMS_RECIPE_DATA
                    {
                        NAME = req.RecipeName,
                        CONTENT = body,
                        CREATOR = req.TrueName,
                    };
                    db.Insertable<RMS_RECIPE_DATA>(data).ExecuteCommand();

                    version.RECIPE_DATA_ID = data.ID;
                    db.Updateable<RMS_RECIPE_VERSION>(version).UpdateColumns(it => new { it.RECIPE_DATA_ID }).ExecuteCommand();

                    recipe.VERSION_LATEST_ID = version.ID;
                    recipe.VERSION_EFFECTIVE_ID = version.ID;
                    db.Updateable<RMS_RECIPE>(recipe).UpdateColumns(it => new { it.VERSION_LATEST_ID, it.VERSION_EFFECTIVE_ID }).ExecuteCommand();

                    res.RECIPE_ID = recipe.ID;
                    res.VERSION_LATEST_ID = recipe.VERSION_LATEST_ID;

                    db.CommitTran();
                    res.Result = true;
                }
                catch
                {
                    db.RollbackTran();
                    throw;
                }
            }

            return res;
        }

        public RabbitMqTransaction GetSecsRecipe(string recipeType, string equipmentID, string recipeName)
        {
            string rabbitmqRoute;
            string transName;
            switch (recipeType)
            {
                case "secsByte":
                    rabbitmqRoute = $"EAP.SecsClient.{equipmentID}";
                    transName = "GetUnformattedRecipe";
                    break;
                case "secsSml":
                    rabbitmqRoute = $"EAP.SecsClient.{equipmentID}";
                    transName = "GetFormattedRecipe";
                    break;
                default:
                    rabbitmqRoute = $"EAP.SecsClient.{equipmentID}";
                    transName = "GetUnformattedRecipe";
                    break;
            }

            var listenChannel = ConfigurationManager.AppSettings["ListenChannel"];
            var trans = new RabbitMqTransaction
            {
                TransactionName = transName,
                EquipmentID = equipmentID,
                NeedReply = true,
                ReplyChannel = listenChannel,
                Parameters = new Dictionary<string, object>() { { "RecipeName", recipeName } }
            };

            var rabbitRes = RabbitMqService.ProduceWaitReply(rabbitmqRoute, trans, 120);

            if (rabbitRes == null)
            {
                Log.Debug($"Rabbit Mq send content:\n{JsonConvert.SerializeObject(trans, Formatting.Indented)}");
                Log.Error("GetNewRecipeVersion Time out!");
            }

            return rabbitRes;
        }

        private AddNewRecipeResponse AddOnlyNameRecipeTransaction(RMS_EQUIPMENT eqp, AddNewRecipeRequest req)
        {
            AddNewRecipeResponse res = new AddNewRecipeResponse();
            var db = DbFactory.GetSqlSugarClient();
            db.BeginTran();
            try
            {
                //添加Recipe
                var recipe = new RMS_RECIPE
                {
                    EQUIPMENT_ID = req.EquipmentId,
                    NAME = req.RecipeName,
                };
                db.Insertable<RMS_RECIPE>(recipe).ExecuteCommand();
                //添加Version
                var eqtype = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
                var version = new RMS_RECIPE_VERSION
                {
                    RECIPE_ID = recipe.ID,
                    _FLOW_ROLES = eqtype.FLOWROLEIDS,
                    CURRENT_FLOW_INDEX = 100,
                    REMARK = "First Version",
                    CREATOR = req.TrueName
                };
                db.Insertable<RMS_RECIPE_VERSION>(version).ExecuteCommand();
                recipe.VERSION_LATEST_ID = version.ID;
                recipe.VERSION_EFFECTIVE_ID = version.ID;
                db.Updateable<RMS_RECIPE>(recipe).UpdateColumns(it => new { it.VERSION_LATEST_ID, it.VERSION_EFFECTIVE_ID }).ExecuteCommand();
                res.RECIPE_ID = recipe.ID;
                res.VERSION_LATEST_ID = recipe.VERSION_LATEST_ID;
            }
            catch
            {
                db.RollbackTran();
                throw;
            }
            db.CommitTran();//提交事务
            res.Result = true;
            return res;
        }
    }
}

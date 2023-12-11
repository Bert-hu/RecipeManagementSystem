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

            //TODO Unfomatted/Fomatted/onlyName
            switch (eqp.RECIPE_TYPE)
            {
                case "secsByte":
                    return AddUnfomattedRecipeTransaction(eqp, req);
                case "onlyName":
                    return AddOnlyNameRecipeTransaction(eqp, req);
                default:
                    return AddUnfomattedRecipeTransaction(eqp, req);
            }

        }

        private AddNewRecipeResponse AddUnfomattedRecipeTransaction(RMS_EQUIPMENT eqp, AddNewRecipeRequest req)
        {
            AddNewRecipeResponse res = new AddNewRecipeResponse();
            var rabbitRes = GetUnfomattedRecipe(eqp.RECIPE_TYPE, req.EquipmentId, req.RecipeName);
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

                if (rabbitRes.Parameters["Result"].ToString().ToUpper() == "TRUE")
                {
                    if (!isdebugmode)//生产环境才检查 设备回复的RecipeName和请求中的RecipeName是否一致
                    {
                        if (rabbitRes.Parameters["RecipeName"].ToString() != req.RecipeName)
                        {
                            res.Message = $"The RecipeName of the response({rabbitRes.Parameters["RecipeName"].ToString()}) is inconsistent with the request({req.RecipeName})";
                            return res;
                        }
                    }

                    byte[] body;
                    //根据Recipe Type获取 ByteArray类型的Body
                    switch (eqp.RECIPE_TYPE)
                    {
                        default:
                            body = Convert.FromBase64String(rabbitRes.Parameters["RecipeBody"].ToString());
                            break;
                    }
                    //开启事务，执行新建Recipe的任务
                    var db = DbFactory.GetSqlSugarClient();
                    db.BeginTran();
                    try
                    {
                        //添加Recipe
                        var recipe = new RMS_RECIPE
                        {
                            EQUIPMENT_ID = req.EquipmentId,
                            NAME = req.RecipeName,
                            //  CREATOR = req.TrueName,
                            //  LASTEDITOR = req.TrueName
                        };
                        db.Insertable<RMS_RECIPE>(recipe).ExecuteCommand();
                        var eqtype = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
                        //添加Version
                        //var flow = db.Queryable<RMS_FLOW>().In(eqp.FLOW_ID).First();
                        var version = new RMS_RECIPE_VERSION
                        {
                            RECIPE_ID = recipe.ID,
                            //FLOW_ID = recipe.FLOW_ID,
                            _FLOW_ROLES = eqtype.FLOWROLEIDS,
                            CURRENT_FLOW_INDEX = 100,
                            REMARK = "First Version",
                            CREATOR = req.TrueName
                        };
                        db.Insertable<RMS_RECIPE_VERSION>(version).ExecuteCommand();
                        //添加Data
                        var data = new RMS_RECIPE_DATA
                        {
                            NAME = req.RecipeName,
                            CONTENT = body,
                            CREATOR = req.TrueName,
                        };
                        db.Insertable<RMS_RECIPE_DATA>(data).ExecuteCommand();
                        //更新外键
                        version.RECIPE_DATA_ID = data.ID;
                        db.Updateable<RMS_RECIPE_VERSION>(version).UpdateColumns(it => new { it.RECIPE_DATA_ID }).ExecuteCommand();
                        //更新生效版和最新版
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
                }
                else
                {
                    res.Result = false;
                    res.Message = rabbitRes.Parameters["Message"].ToString();
                }


            }
            else//Rabbit Mq失败
            {
                res.Message = "Equipment offline or EAP client error!";
            }


            return res;
        }

        public RabbitMqTransaction GetUnfomattedRecipe(string RecipeType, string EquipmentID, string RecipeName)
        {
            string rabbitmqroute = string.Empty;
            //识别设备类型和回复queue
            switch (RecipeType)
            {
                default:
                    rabbitmqroute = $"EAP.SecsClient.{EquipmentID}";
                    break;
            }
            var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetUnfomattedRecipe",
                EquipmentID = EquipmentID,
                NeedReply = true,
                ReplyChannel = ListenChannel,
                Parameters = new Dictionary<string, object>() { { "RecipeName", RecipeName } }
            };
            var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 120);


            if (rabbitres == null)//Rabbit Mq失败
            {
                Log.Debug($"Rabbit Mq send content:\n{JsonConvert.SerializeObject(trans, Formatting.Indented)}");
                Log.Error($"GetNewRecipeVersion Time out!");
            }
            return rabbitres;
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

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
            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.NAME == req.RecipeName)?.First();
            if (recipe != null)//检查是否存在同名
            {
                res.Message = $"Recipe '{req.RecipeName}' already exists!";
                return res;
            }
            var rabbitRes = GetUnfomattedRecipe(eqp.RECIPE_TYPE, req.EquipmentId, req.RecipeName);


            if (rabbitRes != null)
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
                db.BeginTran();
                try
                {
                    //添加Recipe
                    recipe = new RMS_RECIPE
                    {
                        EQUIPMENT_ID = req.EquipmentId,
                        NAME = req.RecipeName,
                        FLOW_ID = eqp.FLOW_ID,
                        //  CREATOR = req.TrueName,
                        //  LASTEDITOR = req.TrueName
                    };
                    db.Insertable<RMS_RECIPE>(recipe).ExecuteCommand();
                    //添加Version
                    var flow = db.Queryable<RMS_FLOW>().In(eqp.FLOW_ID).First();
                    var version = new RMS_RECIPE_VERSION
                    {
                        RECIPE_ID = recipe.ID,
                        FLOW_ID = recipe.FLOW_ID,
                        FLOW_ROLES = flow.FLOW_ROLES,
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
            else//Rabbit Mq失败
            {
                res.Message = "GetNewRecipeVersion Time out!";
            }


            return res;
        }

        public RabbitMqTransaction GetUnfomattedRecipe(string RecipeType, string EquipmentID, string RecipeName)
        {
            string rabbitmqroute = string.Empty;
            //识别设备类型和恢复queue
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
            var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 5);
            if (rabbitres == null)//Rabbit Mq失败
            {
                Log.Debug($"Rabbit Mq send content:\n{JsonConvert.SerializeObject(trans, Formatting.Indented)}");
                Log.Error($"GetNewRecipeVersion Time out!");
            }
            return rabbitres;
        }
    }
}

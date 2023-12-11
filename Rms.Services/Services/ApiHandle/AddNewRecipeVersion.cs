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

namespace Rms.Services.Services.ApiHandle
{
    public partial class ApiMessageHandler
    {
        public ResponseMessage AddNewRecipeVersion(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new AddNewRecipeVersionResponse();
            var req = JsonConvert.DeserializeObject<AddNewRecipeVersionRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            if (eqp == null)
            {
                res.Message = $"Euipment '{req.EquipmentId}' not exists!";
                return res;
            }
            var eqtype = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            if (eqp.RECIPE_TYPE == "onlyName")
            {
                res.Message = $"Recipe Type 'onlyName' can not add new version!";
                return res;
            }

            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.ID == req.RecipeId && it.EQUIPMENT_ID == req.EquipmentId).First();
            if (recipe.VERSION_EFFECTIVE_ID != recipe.VERSION_LATEST_ID) //检查是否存在未完成版本
            {
                res.Message = $"Last vsersion is not finished, can not add a new version!";
                return res;
            }
            //开启事务，执行新建Recipe Version的任务
            db.BeginTran();
            try
            {
                var lastversion = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_LATEST_ID).First();
                RMS_RECIPE_VERSION newversion;

                //添加Version
                newversion = new RMS_RECIPE_VERSION
                {
                    RECIPE_ID = req.RecipeId,
                    //FLOW_ID = recipe.FLOW_ID,
                    VERSION = lastversion.VERSION + 1,
                    _FLOW_ROLES = eqtype.FLOWROLEIDS,
                    CURRENT_FLOW_INDEX = -1,
                    CREATOR = req.TrueName,
                };
                db.Insertable<RMS_RECIPE_VERSION>(newversion).ExecuteCommand();
                //更新外键
                recipe.VERSION_LATEST_ID = newversion.ID;
                recipe.VERSION_EFFECTIVE_ID = newversion.ID;
                db.Updateable<RMS_RECIPE>(recipe).UpdateColumns(it => new { it.VERSION_LATEST_ID }).ExecuteCommand();
                res.VERSION_LATEST_ID = newversion.ID;

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

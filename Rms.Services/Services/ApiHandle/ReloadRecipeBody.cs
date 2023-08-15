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

namespace Rms.Services.Services.ApiHandle
{
    public partial class ApiMessageHandler
    {
        public ResponseMessage ReloadRecipeBody(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new ReloadRecipeBodyResponse();
            var req = JsonConvert.DeserializeObject<ReloadRecipeBodyRequest>(jsoncontent);
            var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().In(req.VersionId).First();
            var recipe = db.Queryable<RMS_RECIPE>().In(recipe_version.RECIPE_ID).First();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
            if (recipe.VERSION_LATEST_ID != req.VersionId)//检查是否最新版
            {
                res.Message = "Only the latest version can reload body!";
                return res;
            }
            if (recipe_version.CURRENT_FLOW_INDEX != 0)//检查是否未提交
            {
                res.Message = "Only the unsubmitted recipe version can reload body!";
                return res;
            }
            var rabbitRes = GetUnfomattedRecipe(eqp.RECIPE_TYPE, eqp.ID, recipe.NAME);


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
               
                    if (!string.IsNullOrEmpty(recipe_version.RECIPE_DATA_ID))
                    {
                        //删除旧的data
                        db.Deleteable<RMS_RECIPE_DATA>().In(recipe_version.RECIPE_DATA_ID).ExecuteCommand();
                    }
                    //添加Data
                    var data = new RMS_RECIPE_DATA
                    {
                        NAME = req.RecipeName,
                        CONTENT = body,
                        CREATOR = req.TrueName,
                    };
                    db.Insertable<RMS_RECIPE_DATA>(data).ExecuteCommand();
                    //更新外键
                    recipe_version.RECIPE_DATA_ID = data.ID;
                    db.Updateable<RMS_RECIPE_VERSION>(recipe_version).UpdateColumns(it => new { it.RECIPE_DATA_ID }).ExecuteCommand();
                    res.RECIPE_DATA_ID = data.ID;
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
                res.Message = "ReloadRecipe Time out!";
            }


            return res;
        }
    }
}

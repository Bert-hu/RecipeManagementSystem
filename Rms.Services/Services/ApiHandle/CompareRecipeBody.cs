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
        public ResponseMessage CompareRecipeBody(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new CompareRecipeBodyResponse();
            var req = JsonConvert.DeserializeObject<CompareRecipeBodyRequest>(jsoncontent);
      
            var recipe = db.Queryable<RMS_RECIPE>().Where(it =>it.EQUIPMENT_ID ==req.EquipmentId && it.NAME ==req.RecipeName).First();
            if (recipe == null)
            {
                res.Result = false;
                res.Message = "Recipe does not exist in RMS";
                return res;
            }
            var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_EFFECTIVE_ID).First();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();

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

                var serverdata = db.Queryable<RMS_RECIPE_DATA>().In(recipe_version.RECIPE_DATA_ID)?.First()?.CONTENT;
                var compareresult = body.SequenceEqual(serverdata);
                if (!compareresult) res.Message = "The content of Recipe is inconsistent, please check";
                res.Result = compareresult;
            }
            else//Rabbit Mq失败
            {
                res.Message = "ReloadRecipe Time out!";
            }


            return res;
        }
    }
}

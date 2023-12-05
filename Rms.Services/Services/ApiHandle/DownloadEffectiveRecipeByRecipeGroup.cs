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
        public ResponseMessage DownloadEffectiveRecipeByRecipeGroup(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new DownloadEffectiveRecipeByRecipeGroupResponse();
            var req = JsonConvert.DeserializeObject<DownloadEffectiveRecipeByRecipeGroupRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            var recipegroup = db.Queryable<RMS_RECIPE_GROUP>().First(it => it.NAME == req.RecipeGroupName);
            if (recipegroup == null )
            {
                db.Insertable<RMS_RECIPE_GROUP>(new RMS_RECIPE_GROUP { NAME = req.RecipeGroupName }).ExecuteCommand();
                res.Result = false;
                res.Message = $"Unable to find recipe bound to '{req.RecipeGroupName}'";
                return res;
            }
            if (eqp == null)
            {
                res.Result = false;
                res.Message = "Equipment does not exist in RMS";
                return res;
            }
            var data = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == req.EquipmentId).ToList();
            var eqrecipeids = data.Select(it => it.ID).ToList();
            var binding = db.Queryable<RMS_RECIPE_GROUP_MAPPING>().Where(it => it.RECIPE_GROUP_ID == recipegroup.ID && eqrecipeids.Contains(it.RECIPE_ID)).First();     
            if (binding == null)
            {
                res.Result = false;
                res.Message = $"Unable to find recipe bound to '{req.RecipeGroupName}'";
                return res;
            }
            var recipe = db.Queryable<RMS_RECIPE>().In(binding.RECIPE_ID).First();
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
                        TO_RECIPE_NAME = recipe.NAME,
                        TO_RECIPE_VERSION = recipe_version.VERSION?.ToString(),
                        CREATOR = req.TrueName,
                        CREATETIME = DateTime.Now
                    }).ExecuteCommand();
                    eqp.LASTRUN_RECIPE_ID = recipe.ID;
                    eqp.LASTRUN_RECIPE_TIME = DateTime.Now;
                    db.Updateable<RMS_EQUIPMENT>(eqp).ExecuteCommand();

                }


            }
            else//Rabbit Mq失败
            {
                res.Message = "Equipment offline or EAP client error!";
            }


            return res;
        }


    }
}

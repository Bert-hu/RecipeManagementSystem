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
using Rms.Services.Services.ApiHandle.MessageHandler;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    public partial class DownloadEffectiveRecipeByRecipeGroup : CommonHandler
    {
        public override ResponseMessage Handle(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new DownloadEffectiveRecipeByRecipeGroupResponse();
            var req = JsonConvert.DeserializeObject<DownloadEffectiveRecipeByRecipeGroupRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            var eqtype = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            var recipegroup = db.Queryable<RMS_RECIPE_GROUP>().First(it => it.NAME == req.RecipeGroupName);
            if (recipegroup == null)
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

            if (eqtype.DELETEBEFOREDOWNLOAD)
            {
                var delteres = DeleteAllMachineRecipes(eqp.ID);//暂时不处理删除的答复
            }

            (bool downloadResult, string downloadMessage) = DownloadRecipeToMachine(eqp.ID, recipe.VERSION_EFFECTIVE_ID);

            if (downloadResult)
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
            else
            {
                res.Message = downloadMessage;
            }
            return res;
        }


    }
}

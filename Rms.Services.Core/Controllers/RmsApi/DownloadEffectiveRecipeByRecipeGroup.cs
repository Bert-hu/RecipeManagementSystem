using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using SqlSugar;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        /// <summary>
        /// 按RecipeGroup下载Effective Recipe，支持Golden Recipe Type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DownloadEffectiveRecipeByRecipeGroup(DownloadEffectiveRecipeByRecipeGroupRequest req)
        {
            var res = new DownloadEffectiveRecipeByRecipeGroupResponse();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            if (eqp == null)
            {
                res.Message = $"Euipment '{req.EquipmentId}' not exists!";
                return Json(res);
            }

            var eqpType = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            RMS_EQUIPMENT goldenEqp = eqp;//默认为自己

            if (!string.IsNullOrEmpty(eqpType.GOLDEN_EQID) && eqpType.GOLDEN_RECIPE_TYPE)
            {
                goldenEqp = db.Queryable<RMS_EQUIPMENT>().In(eqpType.GOLDEN_EQID).First();
            }

            var recipegroup = db.Queryable<RMS_RECIPE_GROUP>().First(it => it.NAME == req.RecipeGroupName);
            if (recipegroup == null)
            {
                db.Insertable<RMS_RECIPE_GROUP>(new RMS_RECIPE_GROUP { NAME = req.RecipeGroupName }).ExecuteCommand();
                res.Result = false;
                res.Message = $"Unable to find recipe bound to '{req.RecipeGroupName}'";
                return Json(res);
            }

            var allRecipes = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == goldenEqp.ID).ToList();
            var allRecipeIds = allRecipes.Select(it => it.ID).ToList();
            var binding = db.Queryable<RMS_RECIPE_GROUP_MAPPING>().Where(it => it.RECIPE_GROUP_ID == recipegroup.ID && allRecipeIds.Contains(it.RECIPE_ID)).First();
            if (binding == null)
            {
                res.Result = false;
                res.Message = $"Unable to find recipe bound to '{req.RecipeGroupName}'";
                return Json(res);
            }
            var recipe = db.Queryable<RMS_RECIPE>().In(binding.RECIPE_ID).First();
            var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_EFFECTIVE_ID).First();



            if (eqpType.DELETEBEFOREDOWNLOAD)
            {
                var delteres = rmsTransactionService.DeleteAllMachineRecipes(eqp);//暂时不处理删除的答复
            }
            bool downloadResult = false;
            string downloadMessage = string.Empty;
            if (eqp.RECIPE_TYPE.ToUpper() != "ONLYNAME")
            {
                if (recipe_version.RECIPE_DATA_ID == null)
                {
                    res.Result = false;
                    res.Message = "RMS ERROR! Effective version do not have recipe content!";
                    return Json(res);
                }
                (downloadResult, downloadMessage) = rmsTransactionService.DownloadRecipeToMachine(eqp, recipe.VERSION_EFFECTIVE_ID);

                //20250815: 新增下载绑定的所有subrecipe
                var subbinding = db.Queryable<RMS_RECIPE_GROUP_MAPPING_SUBRECIPE>().Where(it => it.RECIPE_GROUP_ID == recipegroup.ID).First();
                if (subbinding != null && subbinding.RECIPE_ID_LIST?.Count > 0)
                {
                    foreach (var subrecipeId in subbinding.RECIPE_ID_LIST)
                    {
                        var subrecipe = db.Queryable<RMS_RECIPE>().InSingle(subrecipeId);
                        if (subrecipe != null)
                        {
                            var subrecipe_version = db.Queryable<RMS_RECIPE_VERSION>().InSingle(subrecipe.VERSION_EFFECTIVE_ID);
                            if (subrecipe_version != null && subrecipe_version.RECIPE_DATA_ID != null)
                            {
                                rmsTransactionService.DownloadRecipeToMachine(eqp, subrecipe.VERSION_EFFECTIVE_ID);
                            }
                        }
                    }
                }
            }
            else
            {
                downloadResult = true;
            }

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
            return Json(res);
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        /// <summary>
        /// 按Recipe Name下载Recipe到Machine，支持Golden Recipe Type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DownloadEffectiveRecipeToMachine(DownloadEffectiveRecipeToMachineRequest req)
        {
            var res = new DownloadEffectiveRecipeToMachineResponse();
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
                goldenEqp = db.Queryable<RMS_EQUIPMENT>().In(goldenEqp.FATHER_EQID).First();
            }

            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == goldenEqp.ID && it.NAME == req.RecipeName).First();
            if (recipe == null)
            {
                res.Result = false;
                res.Message = $"{goldenEqp.ID} Recipe '{req.RecipeName}' does not exist in RMS";
                return Json(res);
            }
            var recipeVersion = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_EFFECTIVE_ID).First();
            if (recipeVersion.RECIPE_DATA_ID == null)
            {
                res.Result = false;
                res.Message = "RMS ERROR! Effective version does not have recipe content!";
                return Json(res);
            }
            if (eqpType.DELETEBEFOREDOWNLOAD)
            {
                var delteres = rmsTransactionService.DeleteAllMachineRecipes(eqp);//暂时不处理删除的答复
            }

            (bool downloadResult, string downloadMessage) = rmsTransactionService.DownloadRecipeToMachine(eqp, recipe.VERSION_EFFECTIVE_ID);

            if (downloadResult)
            {
                res.Result = true;
                res.RecipeName = recipe.NAME;
                db.Insertable<RMS_CHANGE_RECORD>(new RMS_CHANGE_RECORD
                {
                    EQID = req.EquipmentId,
                    TO_RECIPE_NAME = recipe.NAME,
                    TO_RECIPE_VERSION = recipeVersion.VERSION?.ToString(),
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

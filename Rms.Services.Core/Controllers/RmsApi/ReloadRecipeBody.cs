using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        /// <summary>
        /// 重新加载Recipe Body，req中EquipmentId为空时从EquipmentId对应的设备获取，req中EquipmentId不为空时从指定设备获取
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReloadRecipeBody(ReloadRecipeBodyRequest req)
        {
            var res = new ReloadRecipeBodyResponse();
            var recipeVersion = db.Queryable<RMS_RECIPE_VERSION>().In(req.VersionId).First();
            var recipe = db.Queryable<RMS_RECIPE>().In(recipeVersion.RECIPE_ID).First();
            if (recipe.VERSION_LATEST_ID != req.VersionId)
            {
                res.Message = "Only the latest version can reload body!";
                return Json(res);
            }
            if (recipeVersion.CURRENT_FLOW_INDEX != -1)
            {
                res.Message = "Only the unsubmitted recipe version can reload body!";
                return Json(res);
            }



            RMS_EQUIPMENT eqp;

            if (string.IsNullOrEmpty(req.EquipmentId))//老方法从recipe获取eqp
            {
                eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
            }
            else//新的请求会加上EquipmentId, 适应Golden Recipe Type
            {
                eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            }

            (bool result, string message, byte[]? body) = rmsTransactionService.UploadRecipeToServer(eqp, req.RecipeName);
            if (result)
            {
                db.Ado.BeginTran();
                try
                {
                    if (!string.IsNullOrEmpty(recipeVersion.RECIPE_DATA_ID))
                    {
                        db.Deleteable<RMS_RECIPE_DATA>().In(recipeVersion.RECIPE_DATA_ID).ExecuteCommand();
                    }
                    var data = new RMS_RECIPE_DATA
                    {
                        NAME = req.RecipeName,
                        CONTENT = body,
                        CREATOR = req.TrueName,
                    };
                    db.Insertable<RMS_RECIPE_DATA>(data).ExecuteCommand();
                    recipeVersion.RECIPE_DATA_ID = data.ID;
                    db.Updateable<RMS_RECIPE_VERSION>(recipeVersion).UpdateColumns(it => new { it.RECIPE_DATA_ID }).ExecuteCommand();
                    res.RECIPE_DATA_ID = data.ID;

                    //RMS_DIFFERENT_PARAMETER
                    res.Message = message;
                }
                catch (Exception ex)
                {
                    db.Ado.RollbackTran();
                    Log.Error(ex.Message, ex);
                    res.Message = $"RMS Service Error:{ex.Message}";
                    return Json(res);
                }
                db.Ado.CommitTran();
                res.Result = true;
            }
            else
            {
                res.Message = message;
                return Json(res);
            }
            return Json(res);
        }

    }
}

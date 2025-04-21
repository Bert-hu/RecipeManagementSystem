using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using System.Drawing.Imaging;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        /// <summary>
        /// 添加新的Recipe Version到RMS,支持Golden Recipe Type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddNewRecipeVersion(AddNewRecipeVersionRequest req)
        {
            var res = new AddNewRecipeVersionResponse();
            var goldenEqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            if (goldenEqp == null)
            {
                res.Message = $"Equipment '{req.EquipmentId}' does not exist!";
                return Json(res);
            }
            var eqpType = db.Queryable<RMS_EQUIPMENT_TYPE>().In(goldenEqp.TYPE).First();
            if (goldenEqp.RECIPE_TYPE == "onlyName")
            {
                res.Message = "Recipe Type 'onlyName' cannot add a new version!";
                return Json(res);
            }

            if (!string.IsNullOrEmpty(eqpType.GOLDEN_EQID) && eqpType.GOLDEN_RECIPE_TYPE)
            {
                goldenEqp = db.Queryable<RMS_EQUIPMENT>().In(eqpType.GOLDEN_EQID).First();
            }


            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.NAME == req.RecipeName && it.EQUIPMENT_ID == goldenEqp.ID).First();
            if (recipe.VERSION_EFFECTIVE_ID != recipe.VERSION_LATEST_ID)
            {
                res.Message = "Last version is not finished, cannot add a new version!";
                return Json(res);
            }

            db.Ado.BeginTran();
            try
            {
                var lastVersion = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_LATEST_ID).First();
                RMS_RECIPE_VERSION newVersion;

                // 添加 Version
                newVersion = new RMS_RECIPE_VERSION
                {
                    RECIPE_ID = recipe.ID,
                    VERSION = lastVersion.VERSION + 1,
                    _FLOW_ROLES = eqpType.FLOWROLEIDS,
                    CURRENT_FLOW_INDEX = -1,
                    CREATOR = req.TrueName,
                };
                db.Insertable<RMS_RECIPE_VERSION>(newVersion).ExecuteCommand();

                // 更新外键
                recipe.VERSION_LATEST_ID = newVersion.ID;
                recipe.VERSION_EFFECTIVE_ID = newVersion.ID;
                db.Updateable<RMS_RECIPE>(recipe).UpdateColumns(it => new { it.VERSION_LATEST_ID }).ExecuteCommand();
                res.VERSION_LATEST_ID = newVersion.ID;
                db.Ado.CommitTran();
                res.Result = true;
                return Json(res);
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                Log.Error(ex.Message, ex);
                res.Message = $"RMS Service Error:{ex.Message}";
                return Json(res);
            }
        }

    }
}

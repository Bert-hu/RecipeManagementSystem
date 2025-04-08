using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Core.Rms;
using SqlSugar;
using System.Collections;
using System.Security.Cryptography;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        /// <summary>
        /// 比较Recipe Body,支持Golden Recipe Type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CompareRecipeBody(CompareRecipeBodyRequest req)
        {
            var res = new CompareRecipeBodyResponse();

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
                res.Message = "Recipe does not exist in RMS";
                return Json(res);
            }
            var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_EFFECTIVE_ID).First();

            (res.Result, res.Message) = rmsTransactionService.CompareRecipe(eqp, recipe_version.ID);
            return Json(res);
        }
    }
}

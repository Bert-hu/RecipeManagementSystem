using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using SqlSugar;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        /// <summary>
        /// 获取recipe group绑定的recipe，检查和更新最后运行的recipe，支持Golden Recipe Type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckRecipeGroup(CheckRecipeGroupRequest req)
        {
            var res = new CheckRecipeGroupResponse();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            var recipegroup = db.Queryable<RMS_RECIPE_GROUP>().First(it => it.NAME == req.RecipeGroupName);
            if (recipegroup == null)
            {
                db.Insertable<RMS_RECIPE_GROUP>(new RMS_RECIPE_GROUP { NAME = req.RecipeGroupName }).ExecuteCommand();
                res.Result = false;
                res.Message = $"Unable to find recipe bound to '{req.RecipeGroupName}'";
                return Json(res);
            }
            if (eqp == null)
            {
                res.Result = false;
                res.Message = "Equipment does not exist in RMS";
                return Json(res);
            }
            var eqpType = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            RMS_EQUIPMENT goldenEqp = eqp;//默认为自己

            if (!string.IsNullOrEmpty(eqpType.GOLDEN_EQID) && eqpType.GOLDEN_RECIPE_TYPE)
            {
                goldenEqp = db.Queryable<RMS_EQUIPMENT>().In(goldenEqp.FATHER_EQID).First();
            }

            var allRecipes = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == goldenEqp.ID).ToList();//获取设备所有recipe
            var allRecipeIds = allRecipes.Select(it => it.ID).ToList();
            var binding = db.Queryable<RMS_RECIPE_GROUP_MAPPING>().Where(it => it.RECIPE_GROUP_ID == recipegroup.ID && allRecipeIds.Contains(it.RECIPE_ID)).First();
            if (binding == null)
            {
                res.Result = false;
                res.Message = $"Unable to find recipe bound to '{req.RecipeGroupName}'";
                return Json(res);
            }
            var recipe = db.Queryable<RMS_RECIPE>().In(binding.RECIPE_ID).First();
            if (req.CheckLastRecipe)
            {
                if (recipe.ID != eqp.LASTRUN_RECIPE_ID)
                {
                    res.RecipeName = recipe.NAME;
                    res.Message = $"The last run of the recipe is inconsistent, please download it again";
                }
                else if (eqp.LASTRUN_RECIPE_TIME < DateTime.Now.AddHours(-2))
                {
                    res.RecipeName = recipe.NAME;
                    res.Message = "The recipe did not run for 2 hours, please download it again";
                }
                else
                {
                    res.Result = true;
                    res.RecipeName = recipe.NAME;
                    eqp.LASTRUN_RECIPE_TIME = DateTime.Now;
                    db.Updateable<RMS_EQUIPMENT>(eqp).ExecuteCommand();
                }
            }
            else//不检查，只记录
            {
                res.Result = true;
                res.RecipeName = recipe.NAME;
                eqp.LASTRUN_RECIPE_TIME = DateTime.Now;
                db.Updateable<RMS_EQUIPMENT>(eqp).ExecuteCommand();
            }

            return Json(res);
        }

    }
}

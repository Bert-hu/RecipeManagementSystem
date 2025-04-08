
using Rms.Services.Core.Extension;
using Rms.Services.Core.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Core.Rms;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        /// <summary>
        /// 根据recipe name获取对应equipment tpye的recipe alias List
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetRecipeNameAlias(GetRecipeNameAliasRequest req)
        {
            RMS_RECIPE_NAME_ALIAS config = null;

            config = db.Queryable<RMS_RECIPE_NAME_ALIAS>().First(it => it.EQUIPMENT_TYPE_ID == req.EquipmentTypeId && it.RECIPE_NAME == req.RecipeName);

            GetRecipeNameAliasResponse response = new GetRecipeNameAliasResponse();
            if (config != null)
            {
                response = new GetRecipeNameAliasResponse
                {
                    Result = true,
                    Id = config.ID,
                    EquipmentTypeId = config.EQUIPMENT_TYPE_ID,
                    RecipeName = config.RECIPE_NAME,
                    RecipeAlias = config.RECIPE_ALIAS
                };
            }
            return Json(response);
        }

    }
}

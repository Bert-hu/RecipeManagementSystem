
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

        [HttpPost]
        public JsonResult SetRecipeNameAlias(SetRecipeNameAliasRequest req)
        {
            var config = db.Queryable<RMS_RECIPE_NAME_ALIAS>().First(it => it.EQUIPMENT_TYPE_ID == req.EquipmentTypeId && it.RECIPE_NAME == req.RecipeName);
            if (config != null)
            {
                config.RECIPE_ALIAS = req.RecipeAlias;
                db.Updateable(config).ExecuteCommand();
            }
            else
            {
                config = new RMS_RECIPE_NAME_ALIAS
                {
                    EQUIPMENT_TYPE_ID = req.EquipmentTypeId,
                    RECIPE_ALIAS = req.RecipeAlias,
                    RECIPE_NAME = req.RecipeName
                };
                db.Insertable(config).ExecuteCommand();
            }
            return Json(new ResponseMessage
            {
                Result = true
            });
        }

    }
}

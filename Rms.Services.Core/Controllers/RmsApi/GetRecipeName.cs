
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
        public JsonResult GetRecipeName(GetRecipeNameRequest req)
        {
            GetRecipeNameResponse response = new GetRecipeNameResponse();
            try
            {
                RMS_RECIPE_NAME_ALIAS? config = null;
                var configs = db.Queryable<RMS_RECIPE_NAME_ALIAS>().Where(it => it.EQUIPMENT_TYPE_ID == req.EquipmentTypeId).ToList();

                config = configs.FirstOrDefault(it => it.EQUIPMENT_TYPE_ID == req.EquipmentTypeId && it.RECIPE_ALIAS.Contains(req.RecipeNameAlias));



                if (config != null)
                {
                    response = new GetRecipeNameResponse
                    {
                        Result = true,
                        Id = config.ID,
                        EquipmentTypeId = config.EQUIPMENT_TYPE_ID,
                        RecipeName = config.RECIPE_NAME
                    };
                }
                else
                {
                    response.Message = $"'{req.EquipmentTypeId}' can not find Recipe related to '{req.RecipeNameAlias}'";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                response.Message = ex.Message;
            }
            return Json(response);
        }

    }
}

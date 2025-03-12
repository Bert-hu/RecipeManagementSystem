
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
using System.Diagnostics;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        [HttpPost]
        public JsonResult ReloadRecipeBodyToEffectiveVersion(ReloadRecipeBodyToEffectiveVersionRequest req)
        {
            var res = new ReloadRecipeBodyToEffectiveVersionResponse();

            try
            {
                var EquipmentId = req.EquipmentId;
                var RecipeName = req.RecipeName;
                var eqp = db.Queryable<RMS_EQUIPMENT>().In(EquipmentId).First();
                var recipe = db.Queryable<RMS_RECIPE>().First(it => it.EQUIPMENT_ID == EquipmentId && it.NAME == RecipeName);
                if (recipe == null || recipe.VERSION_EFFECTIVE_ID == null)
                {
                    res.Result = false;
                    res.Message = $"Effective Recipe does not exist in RMS";
                }
                else
                {
                    var recipeVersion = db.Queryable<RMS_RECIPE_VERSION>().InSingle(recipe.VERSION_EFFECTIVE_ID);
                    RmsTransactionService service = new RmsTransactionService(db, rabbitMq);
                    (bool result, string message, byte[]? body) = service.UploadRecipeToServer(eqp, RecipeName);
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
                                NAME = RecipeName,
                                CONTENT = body,
                                CREATOR = recipeVersion.CREATOR,
                            };
                            db.Insertable<RMS_RECIPE_DATA>(data).ExecuteCommand();
                            recipeVersion.RECIPE_DATA_ID = data.ID;

                            db.Updateable<RMS_RECIPE_VERSION>(recipeVersion).UpdateColumns(it => new { it.RECIPE_DATA_ID }).ExecuteCommand();
                            db.Ado.CommitTran();
                            if (req.DeleteAfterReload)
                            {
                                service.DeleteMachineRecipe(eqp, req.RecipeName);
                            }
                            res.Result = true;
                        }
                        catch (Exception ex)
                        {
                            db.Ado.RollbackTran();
                            Log.Error(ex.Message, ex);
                            res.Result = false;
                            res.Message = $"RMS Service Error:{ex.Message}";
                        }
                    }
                    else
                    {
                        res.Result = false;
                        res.Message = $"Get recipe '{recipe.NAME}' body from machine fail:{message}";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                res.Result = false;
                res.Message = $"RMS Service error:{ex.ToString()}";
            }
            return Json(res);
        }

    }
}

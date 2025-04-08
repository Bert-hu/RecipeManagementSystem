using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;

namespace Rms.Services.Core.Controllers
{

    public partial class ApiController : Controller
    {
        /// <summary>
        /// 添加新的Recipe到RMS，支持Golden Recipe Type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddNewRecipe(AddNewRecipeRequest req)
        {
            var res = new AddNewRecipeResponse();
            try
            {
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


                //检查同名recipe
                var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == goldenEqp.ID && it.NAME == req.RecipeName)?.First();
                if (recipe != null)//检查是否存在同名
                {
                    res.Message = $"Recipe '{req.RecipeName}' already exists!";
                    return Json(res);
                }

                (bool result, string message, byte[]? body) = rmsTransactionService.UploadRecipeToServer(eqp, req.RecipeName);//注意这里是从请求的eqp上传

                if (result)
                {
                    db.Ado.BeginTran();
                    try
                    {
                        recipe = new RMS_RECIPE
                        {
                            EQUIPMENT_ID = goldenEqp.ID,
                            NAME = req.RecipeName
                        };
                        db.Insertable<RMS_RECIPE>(recipe).ExecuteCommand();

                        //var eqtype = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
                        var versionFlowIndex = 100;
                        if (eqpType.FLOWROLEIDS.Count != 0) //没有flowrole，不能添加
                        {
                            versionFlowIndex = -1;
                        }

                        var version = new RMS_RECIPE_VERSION
                        {
                            RECIPE_ID = recipe.ID,
                            _FLOW_ROLES = eqpType.FLOWROLEIDS,
                            CURRENT_FLOW_INDEX = versionFlowIndex,
                            REMARK = "First Version",
                            CREATOR = req.TrueName
                        };
                        db.Insertable<RMS_RECIPE_VERSION>(version).ExecuteCommand();
                        if (body != null) //Recipe 为OnlyName返回result为true，body为null
                        {
                            var data = new RMS_RECIPE_DATA
                            {
                                NAME = req.RecipeName,
                                CONTENT = body,
                                CREATOR = req.TrueName,
                            };
                            db.Insertable<RMS_RECIPE_DATA>(data).ExecuteCommand();

                            version.RECIPE_DATA_ID = data.ID;
                            db.Updateable<RMS_RECIPE_VERSION>(version).UpdateColumns(it => new { it.RECIPE_DATA_ID }).ExecuteCommand();
                        }

                        recipe.VERSION_LATEST_ID = version.ID;
                        if (versionFlowIndex == 100) recipe.VERSION_EFFECTIVE_ID = version.ID;
                        db.Updateable<RMS_RECIPE>(recipe).UpdateColumns(it => new { it.VERSION_LATEST_ID, it.VERSION_EFFECTIVE_ID }).ExecuteCommand();

                        res.RECIPE_ID = recipe.ID;
                        res.VERSION_LATEST_ID = recipe.VERSION_LATEST_ID;

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
                else
                {
                    res.Message = message;
                    return Json(res);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                res.Message = ex.Message;
                return Json(res);
            }
        }


    }
}

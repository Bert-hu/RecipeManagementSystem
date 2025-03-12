using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;
using Rms.Web.Utils;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Media.Media3D;

namespace Rms.Web.Controllers.Production
{
    [Route("[controller]/[action]")]
    public class ZJPD5Controller : CommonProductionController
    {
        public ActionResult Mounter()
        {
            return View("~/Views/Production/PD5/Mounter.cshtml");
        }

        public JsonResult GetEquipments()
        {
            var db = DbFactory.GetSqlSugarClient();

            var sql = @"select RE.* from RMS_EQUIPMENT RE,RMS_EQUIPMENT_TYPE RET
where RE.TYPE = RET.ID
AND RE.LINE like 'PD5%'
ORDER BY RET.ORDERSORT,RE.ORDERSORT";
            var data = db.SqlQueryable<RMS_EQUIPMENT>(sql).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetServerRecipeList(int page, int limit, string equipmentId, string searchRecipe)
        {

            var eqp = db.Queryable<RMS_EQUIPMENT>().In(equipmentId).First();
            var totalnum = 0;
            List<RecipeVersion> RecipeVersionList = db.Queryable<RMS_RECIPE>()
                .Where(it => it.EQUIPMENT_ID == equipmentId && (string.IsNullOrEmpty(searchRecipe) || it.NAME.Contains(searchRecipe)))
                .LeftJoin<RMS_RECIPE_VERSION>((r, rv) => r.VERSION_LATEST_ID == rv.ID)
                .LeftJoin<RMS_RECIPE_VERSION>((r, rv, rv2) => r.VERSION_EFFECTIVE_ID == rv2.ID)
                .Select((r, rv, rv2) => new RecipeVersion
                {
                    RECIPE_ID = r.ID,
                    RECIPE_NAME = r.NAME,
                    RECIPE_LATEST_VERSION = (decimal)rv.VERSION,
                    RECIPE_EFFECTIVE_VERSION = (decimal)rv2.VERSION,
                    RECIPE_LATEST_VERSION_ID = rv.ID,
                    RECIPE_EFFECTIVE_VERSION_ID = rv2.ID
                })
                .ToPageList(page, limit, ref totalnum);

            return Json(new
            {
                data = RecipeVersionList,
                code = 0,
                count = totalnum,
                canEdit = eqp.RECIPE_TYPE == "secsSml"

            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMachineRecipeList(int page, int limit, string equipmentId, string searchRecipe)
        {
            try
            {
                string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/geteppd";
                var body = JsonConvert.SerializeObject(new GetEppdRequest { EquipmentId = equipmentId });

                var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
                var rep = JsonConvert.DeserializeObject<GetEppdResponse>(apiresult);
                if (rep.Result)
                {

                    List<RMS_RECIPE> recipelist = new List<RMS_RECIPE>();
                    foreach (var item in rep.EPPD)
                    {
                        RMS_RECIPE rcpItem = new RMS_RECIPE();
                        rcpItem.NAME = item;
                        rcpItem.EQUIPMENT_ID = equipmentId;
                        recipelist.Add(rcpItem);
                    };
                    //分页
                    var totalNum = recipelist.Count();
                    recipelist = recipelist.Count() > limit ? recipelist.Skip((page - 1) * limit).Take(limit).ToList() : recipelist;
                    return Json(new
                    {
                        data = recipelist,
                        code = 0,
                        count = totalNum

                    }, JsonRequestBehavior.AllowGet);
                    //return Json(new { recipelist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        data = rep,
                        code = 10,
                        count = 0,
                        msg = rep.Message
                    }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { code = 11, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetModelNameFromBarcode(string barcode)
        {
            string sfis_step7_req = $"EQXXXXXX01,{barcode},7,M001603,JORDAN,,OK,SN_MODEL_NAME_PROJECT_NAME_INFO=???";
            string sfis_step7_res = string.Empty;
            string errmsg = string.Empty;
            if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg))
            {
                Dictionary<string, string> sfisParameters = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
          .Where(keyValueArray => keyValueArray.Length == 2)
          .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                string modelName = sfisParameters["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[0];
                string projectName = sfisParameters["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[1];
                string groupName = sfisParameters["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[2];

                return Json(new { Result = true, ModelName = modelName, ProjectName = projectName, GroupName = groupName });
            }
            else
            {
                return Json(new { Result = false, Message = $"{errmsg}" });
            }

        }

        public JsonResult PpSelect(string equipmentId, string serverRecipeId, string machineRecipeName)
        {
            string message = string.Empty;
            bool result = false;
            try
            {
                string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/DownloadEffectiveRecipeToMachine";
                var recipe = db.Queryable<RMS_RECIPE>().In(serverRecipeId).First();
                var body = JsonConvert.SerializeObject(new DownloadEffectiveRecipeToMachineRequest { TrueName = User.TRUENAME, EquipmentId = equipmentId, RecipeName = recipe.NAME });
                var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
                var rep = JsonConvert.DeserializeObject<DownloadEffectiveRecipeToMachineResponse>(apiresult);
                if (rep.Result)
                {
                    message += $"下载server程式{recipe.NAME}成功";

                    if (!string.IsNullOrEmpty(machineRecipeName))
                    {
                        apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/ReloadRecipeBodyToEffectiveVersion";
                        var body2 = JsonConvert.SerializeObject(new ReloadRecipeBodyToEffectiveVersionRequest { TrueName = User.TRUENAME, EquipmentId = equipmentId, RecipeName = machineRecipeName, DeleteAfterReload = true });
                        var apiresult2 = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body2);
                        var rep2 = JsonConvert.DeserializeObject<ReloadRecipeBodyToEffectiveVersionResponse>(apiresult2);
                        if (rep2.Result)
                        {
                            message += $",上传更新server程式{machineRecipeName}成功";
                            result = true;
                        }
                        else
                        {
                            message += $",上传更新server程式{recipe.NAME}失败：{rep2.Message}";
                        }
                    }
                    else
                    {
                        message = $"下载server程式{recipe.NAME}失败：{rep.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"切换失败：{ex.Message}";
            }
            return Json(new { Result = false, Message = message });

        }

        public JsonResult ScanBarCode(string sn)
        {
            string message = string.Empty;
            bool result = false;

            try
            {
                string sfis_step7_req = $"EQXXXXXX01,{sn},7,M001603,JORDAN,,OK,SN_MODEL_NAME_PROJECT_NAME_INFO=??? ";
                string sfis_step7_res = string.Empty;
                string errmsg = string.Empty;
                if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg))
                {
                    Dictionary<string, string> sfispara = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
    .Where(keyValueArray => keyValueArray.Length == 2)
.ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                    string modelName = sfispara["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[0];
                    string projectName = sfispara["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[1];
                    string groupName = sfispara["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[2];

                    return Json(new { Result = true, ModelName = modelName, ProjectName = projectName, GroupName = groupName });
                }
                else
                {
                    message = $"获取SN信息失败";
                }

            }
            catch (Exception)
            {

            }
            return Json(new { Result = result, Message = message });
        }
    }
}
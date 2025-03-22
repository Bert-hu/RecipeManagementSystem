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
    public class SmtController : CommonProductionController
    {
        public ActionResult Index()
        {
            return View("~/Views/Production/SMT/Index.cshtml");
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
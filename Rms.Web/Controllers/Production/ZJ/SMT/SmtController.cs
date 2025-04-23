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
        class LineMachines
        {
            public string Line { get; set; } = string.Empty;
            public string Printer { get; set; } = string.Empty;
            public string Spi { get; set; } = string.Empty;
            public List<string> Mounter { get; set; } = new List<string>();
            public string Aoi_F { get; set; } = string.Empty;
            public string Reflow { get; set; } = string.Empty;
            public string Aoi_B { get; set; } = string.Empty;

        }
        List<LineMachines> lineMachines = new List<LineMachines>
        {
            new LineMachines{Line = "S77",Aoi_B = "EQAOI00169"},
             new LineMachines{Line = "Z42", Spi= "EQSPI00190",Aoi_B = "EQAOI00213",Reflow="EQP010"},
             new LineMachines{Line = "Test", Spi= "Test",Aoi_B = "Test"},
                      new LineMachines{Line = "S72",Reflow="EQFRF00125"},
        };
        public ActionResult Index()
        {
            return View("~/Views/Production/SMT/Index.cshtml");
        }

        public ActionResult SelectRecipe()
        {
            return View("~/Views/Production/SMT/SelectRecipe.cshtml");
        }

        public JsonResult GetLines()
        {
            return Json(lineMachines);
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

        public JsonResult PpSelectByPanelSn(string station, string machineId, string panelSn)
        {
            //station用于以后不同的站点，使用不同规则匹配recipe name
            string message = string.Empty;
            bool result = false;
            try
            {
                string sfis_step7_req = $"EQXXXXXX01,{panelSn},7,M001603,JORDAN,,OK,SN_MODEL_NAME_PROJECT_NAME_INFO=???";
                string sfis_step7_res = string.Empty;
                string errmsg = string.Empty;
                if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg) && sfis_step7_res.ToUpper().StartsWith("OK"))
                {
                    Dictionary<string, string> sfisParameters = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
              .Where(keyValueArray => keyValueArray.Length == 2)
              .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                    string modelName = sfisParameters["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[0];
                    string projectName = sfisParameters["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[1];
                    string groupName = sfisParameters["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[2];

                    string getEPPDUrl = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/GetEppd";
                    var getEppdResponseStr = HTTPClientHelper.HttpPostRequestAsync4Json(getEPPDUrl, JsonConvert.SerializeObject(new GetEppdRequest { EquipmentId = machineId }));
                    var getEppdResponse = JsonConvert.DeserializeObject<GetEppdResponse>(getEppdResponseStr);
                    if (getEppdResponse.Result)
                    {
                        var eppd = getEppdResponse.EPPD;

                        var recipeName = GetRecipeNameByModelName(eppd, modelName);
                        if (!string.IsNullOrEmpty(recipeName))
                        {
                            string ppselectUrl = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/PpSelect";
                            var ppSelectResponseStr = HTTPClientHelper.HttpPostRequestAsync4Json(ppselectUrl, JsonConvert.SerializeObject(new PpSelectRequest { TrueName = User.TRUENAME, EquipmentId = machineId, RecipeName = recipeName }));
                            var ppSelectResponse = JsonConvert.DeserializeObject<PpSelectResponse>(ppSelectResponseStr);
                            if (ppSelectResponse.Result)
                            {
                                AddProductionLog(machineId, "PpSelectByPanelSn", "True", $"{machineId}发送切换到{recipeName}指令成功");
                                return Json($"{machineId}发送切换到{recipeName}指令成功，请等待设备切换");
                            }
                            else
                            {
                                AddProductionLog(machineId, "PpSelectByPanelSn", "False", $"{machineId}发送切换到{recipeName}指令失败:{ppSelectResponse.Message}");
                                return Json($"{machineId}发送切换到{recipeName}指令失败:{ppSelectResponse.Message}");
                            }
                        }
                        else
                        {
                            AddProductionLog(machineId, "PpSelectByPanelSn", "False", $"{machineId}中找不到与{modelName}匹配的程式");
                            return Json($"{machineId}中找不到与{modelName}匹配的程式");
                        }
                    }
                    else
                    {
                        AddProductionLog(machineId, "PpSelectByPanelSn", "False", $"{machineId}获取设备程式清单失败");
                        return Json($"{machineId}获取设备程式清单失败");
                    }



                }
                else
                {
                    AddProductionLog(machineId, "PpSelectByPanelSn", "False", $"{machineId}切换失败，SFIS Error {sfis_step7_res} {errmsg}");
                    return Json($"{machineId}切换失败，SFIS Error {sfis_step7_res} {errmsg}");
                }



            }
            catch (Exception ex)
            {
                return Json($"切换失败：{ex.Message}");
            }

        }

        public JsonResult PpSelectGetRecipeList(string station, string machineId, string panelSn)
        {
            if (User == null)
            {
                LogProductionEvent(machineId, "PpSelectGetRecipeList", false, "未登录或登录已超时");
                return Json(new { Result = false, Message = "未登录或登录已超时" });
            }

            string message = string.Empty;
            string sfis_step7_req = $"EQXXXXXX01,{panelSn},7,M001603,JORDAN,,OK,SN_MODEL_NAME_PROJECT_NAME_INFO=???";
            string sfis_step7_res = string.Empty;
            string errmsg = string.Empty;

            try
            {
                if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg))
                {
                    if (sfis_step7_res.ToUpper().StartsWith("OK"))
                    {
                        var sfisParameters = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                           .Where(keyValueArray => keyValueArray.Length == 2)
                           .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);

                        string modelName = sfisParameters["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[0];
                        string projectName = sfisParameters["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[1];
                        string groupName = sfisParameters["SN_MODEL_NAME_PROJECT_NAME_INFO"].TrimEnd(';').Split(':')[2];

                        string getEPPDUrl = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/GetEppd";
                        var getEppdResponseStr = HTTPClientHelper.HttpPostRequestAsync4Json(getEPPDUrl, JsonConvert.SerializeObject(new GetEppdRequest { EquipmentId = machineId }));
                        var getEppdResponse = JsonConvert.DeserializeObject<GetEppdResponse>(getEppdResponseStr);

                        if (getEppdResponse.Result)
                        {
                            var eppd = getEppdResponse.EPPD;
                            var recipeList = GetRecipeListByModelName(eppd, modelName);

                            if (recipeList != null)
                            {
                                LogProductionEvent(machineId, "PpSelectGetRecipeList", true, $"SN:{panelSn} 成功获取 {modelName} 的Recipe列表");
                                return Json(new { Result = true, RecipeList = recipeList.Select(it => new { NAME = it }).ToList() });
                            }
                            else
                            {
                                LogProductionEvent(machineId, "PpSelectGetRecipeList", false, $"{machineId} 中找不到与 {modelName} 匹配的程式");
                                return Json(new { Result = false, Message = $"{machineId} 中找不到与 {modelName} 匹配的程式" });
                            }
                        }
                        else
                        {
                            LogProductionEvent(machineId, "PpSelectGetRecipeList", false, $"{machineId} 获取设备程式清单失败");
                            return Json(new { Result = false, Message = $"{machineId} 获取设备程式清单失败" });
                        }
                    }
                    else
                    {
                        LogProductionEvent(machineId, "PpSelectGetRecipeList", false, $"{machineId} 切换失败，SFIS Error {sfis_step7_res} {errmsg}");
                        return Json(new { Result = false, Message = $"{machineId} 切换失败，SFIS Error {sfis_step7_res} {errmsg}" });
                    }
                }
                else
                {
                    LogProductionEvent(machineId, "PpSelectGetRecipeList", false, $"{machineId} 切换失败，SFIS Error {sfis_step7_res} {errmsg}");
                    return Json(new { Result = false, Message = $"{machineId} 切换失败，SFIS Error {sfis_step7_res} {errmsg}" });
                }
            }
            catch (Exception ex)
            {
                message = $"切换失败：{ex.Message}";
                LogProductionEvent(machineId, "PpSelectGetRecipeList", false, message + Environment.NewLine + ex.StackTrace);
            }

            return Json(new { Result = false, Message = message });
        }

        public JsonResult PpSelectGetRecipeList_MO(string station, string machineId, string mo)
        {
            if (User == null)
            {
                LogProductionEvent(machineId, "PpSelectGetRecipeList_MO", false, "未登录或登录已超时");
                return Json(new { Result = false, Message = "未登录或登录已超时" });
            }

            string message = string.Empty;
            string sfis_step7_req = $"EQXXXXXX01,{mo},7,M001603,JORDAN,,OK,MO_INFO=???";
            string sfis_step7_res = string.Empty;
            string errmsg = string.Empty;

            try
            {
                if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg))
                {
                    if (sfis_step7_res.ToUpper().StartsWith("OK"))
                    {
                        var sfisParameters = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                           .Where(keyValueArray => keyValueArray.Length == 2)
                           .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);

                        string projectName = sfisParameters["MO_INFO"].TrimEnd(';').Split(':')[0];
                        string modelName = sfisParameters["MO_INFO"].TrimEnd(';').Split(':')[1];

                        string getEPPDUrl = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/GetEppd";
                        var getEppdResponseStr = HTTPClientHelper.HttpPostRequestAsync4Json(getEPPDUrl, JsonConvert.SerializeObject(new GetEppdRequest { EquipmentId = machineId }));
                        var getEppdResponse = JsonConvert.DeserializeObject<GetEppdResponse>(getEppdResponseStr);

                        if (getEppdResponse.Result)
                        {
                            var eppd = getEppdResponse.EPPD;
                            var recipeList = GetRecipeListByModelName(eppd, modelName);

                            if (recipeList != null)
                            {
                                LogProductionEvent(machineId, "PpSelectGetRecipeList_MO", true, $"MO:{mo} 成功获取 {modelName} 的Recipe列表");
                                return Json(new { Result = true, RecipeList = recipeList.Select(it => new { NAME = it }).ToList() });
                            }
                            else
                            {
                                LogProductionEvent(machineId, "PpSelectGetRecipeList_MO", false, $"{machineId} 中找不到与 {modelName} 匹配的程式");
                                return Json(new { Result = false, Message = $"{machineId} 中找不到与 {modelName} 匹配的程式" });
                            }
                        }
                        else
                        {
                            LogProductionEvent(machineId, "PpSelectGetRecipeList_MO", false, $"{machineId} 获取设备程式清单失败");
                            return Json(new { Result = false, Message = $"{machineId} 获取设备程式清单失败" });
                        }
                    }
                    else
                    {
                        LogProductionEvent(machineId, "PpSelectGetRecipeList_MO", false, $"{machineId} 切换失败，SFIS Error {sfis_step7_res} {errmsg}");
                        return Json(new { Result = false, Message = $"{machineId} 切换失败，SFIS Error {sfis_step7_res} {errmsg}" });
                    }
                }
                else
                {
                    LogProductionEvent(machineId, "PpSelectGetRecipeList_MO", false, $"{machineId} 切换失败，SFIS Error {sfis_step7_res} {errmsg}");
                    return Json(new { Result = false, Message = $"{machineId} 切换失败，SFIS Error {sfis_step7_res} {errmsg}" });
                }
            }
            catch (Exception ex)
            {
                message = $"切换失败：{ex.Message}";
                LogProductionEvent(machineId, "PpSelectGetRecipeList_MO", false, message + Environment.NewLine + ex.StackTrace);
            }

            return Json(new { Result = false, Message = message });
        }

        public JsonResult PpSelectByRecipeName(string machineId, string recipeName)
        {
            string message = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(recipeName))
                {
                    string ppselectUrl = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/PpSelect";
                    var ppSelectResponseStr = HTTPClientHelper.HttpPostRequestAsync4Json(ppselectUrl, JsonConvert.SerializeObject(new PpSelectRequest { TrueName = User.TRUENAME, EquipmentId = machineId, RecipeName = recipeName }));
                    var ppSelectResponse = JsonConvert.DeserializeObject<PpSelectResponse>(ppSelectResponseStr);

                    if (ppSelectResponse.Result)
                    {
                        LogProductionEvent(machineId, "PpSelectByRecipeName", true, $"{machineId} 发送切换到 {recipeName} 指令成功");
                        return Json(new { Result = true, Message = $"{machineId} 发送切换到 {recipeName} 指令成功，请等待设备切换" });
                    }
                    else
                    {
                        LogProductionEvent(machineId, "PpSelectByRecipeName", false, $"{machineId} 发送切换到 {recipeName} 指令失败: {ppSelectResponse.Message}");
                        return Json(new { Result = false, Message = $"{machineId} 发送切换到 {recipeName} 指令失败: {ppSelectResponse.Message}" });
                    }
                }
                else
                {
                    LogProductionEvent(machineId, "PpSelectByRecipeName", false, "Recipe Name 未选择");
                    return Json(new { Result = false, Message = "Recipe Name 未选择" });
                }
            }
            catch (Exception ex)
            {
                message = $"切换失败：{ex.Message}";
                LogProductionEvent(machineId, "PpSelectByRecipeName", false, message + Environment.NewLine + ex.StackTrace);
            }

            return Json(new { Result = false, Message = message });
        }

        private void LogProductionEvent(string machineId, string operation, bool success, string message)
        {
            AddProductionLog(machineId, operation, success ? "True" : "False", message);
        }

        private string GetRecipeNameByModelName(List<string> EPPD, string modelName)
        {
            for (int length = 10; length >= 7; length--)
            {
                if (modelName.Length >= length)
                {
                    string modelSubstring = modelName.Substring(0, length);
                    string match = EPPD.FirstOrDefault(it => it.Length >= length && it.Substring(0, length) == modelSubstring);
                    if (match != null)
                    {
                        return match;
                    }
                }
            }
            return string.Empty;
        }

        private List<string> GetRecipeListByModelName(List<string> EPPD, string modelName)
        {
            for (int length = 10; length >= 7; length--)
            {
                if (modelName.Length >= length)
                {
                    string modelSubstring = modelName.Substring(0, length);
                    List<string> matches = EPPD.Where(it => it.Length >= length && it.Substring(0, length) == modelSubstring).ToList();
                    if (matches.Count > 0)
                    {
                        return matches;
                    }
                }
            }
            return null;
        }


    }
}
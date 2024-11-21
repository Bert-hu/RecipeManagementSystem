using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using Rms.Web.Utils;
using Rms.Web.ViewModels;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Rms.Models.DataBase.Mms;
using System.Collections;
using System.Web.Http.Results;
using Antlr.Runtime.Misc;

namespace Rms.Web.Controllers.Production
{
    public class CommonController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }


        public JsonResult GetProcesses()
        {
            var eqptypes = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipmenttypeids).ToList();
            var processes = eqptypes.Select(it => it.PROCESS).Distinct().ToList();
            return Json(processes, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetEQPs(int page, int limit, string processfilter = "")
        {

            var totalnum = 0;
            var eqptypes = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipmenttypeids).ToList();

            if (!string.IsNullOrEmpty(processfilter))
            {
                eqptypes = eqptypes.Where(it => it.PROCESS == processfilter).OrderBy(it => it.ORDERSORT).ToList();
            }
            var rawdata = db.Queryable<RMS_EQUIPMENT>().OrderBy(it => it.ORDERSORT).ToList();

            var totaleqpData = rawdata.Join(eqptypes, item => item.TYPE, order => order.ID, (eqp, eqptype) => new { eqp = eqp, eqptype = eqptype })
                 .OrderBy(x => x.eqptype.ORDERSORT)
                 .Select(x => new EquipmentViewModel
                 {
                     ID = x.eqp.ID,
                     NAME = x.eqp.NAME,
                     CREATOR = x.eqp.CREATOR,
                     CREATETIME = x.eqp.CREATETIME,
                     LASTEDITOR = x.eqp.LASTEDITOR,
                     LASTEDITTIME = x.eqp.LASTEDITTIME,
                     RECIPE_TYPE = x.eqp.RECIPE_TYPE,
                     ORDERSORT = x.eqp.ORDERSORT,
                     TYPE = x.eqp.TYPE,
                     LINE = x.eqp.LINE,
                     LASTRUN_RECIPE_ID = x.eqp.LASTRUN_RECIPE_ID,
                     LASTRUN_RECIPE_TIME = x.eqp.LASTRUN_RECIPE_TIME,
                     TYPEID = x.eqptype.ID,
                     TYPENAME = x.eqptype.NAME,
                     TYPEPROCESS = x.eqptype.PROCESS,
                     TYPEORDERSORT = x.eqptype.ORDERSORT,
                     TYPEVENDOR = x.eqptype.VENDOR,
                     TYPETYPE = x.eqptype.TYPE,
                 }).ToList();
            totalnum = totaleqpData.Count;
            var eqpData = totaleqpData.Skip((page - 1) * limit).Take(limit);
            return Json(new { eqpData, data = eqpData, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public class EquipmentInfo
        {
            public string ID { get; set; }
            public string NAME { get; set; }
            public string RECIPE_GROUP { get; set; }
            public string RECIPE_NAME { get; set; }
            public DateTime DATETIME { get; set; }
            public string DATETIMESTR { get { return DATETIME.ToString(); } }
            public string STATUS { get; set; }
        }

        public JsonResult GetEquipmentInfo(string equipmentid)
        {
            var db = DbFactory.GetSqlSugarClient();

            var sql = string.Format(@"SELECT 
RE.ID,RE.NAME,RE.LASTRUN_RECIPE_ID,RR.NAME RECIPE_NAME,RRG.NAME RECIPE_GROUP,RE.LASTRUN_RECIPE_TIME DATETIME
FROM RMS_EQUIPMENT RE
LEFT JOIN RMS_RECIPE RR
ON RE.LASTRUN_RECIPE_ID = RR.ID
LEFT JOIN RMS_RECIPE_GROUP RRG
ON RR.RECIPE_GROUP_ID = RRG.ID
WHERE RE.ID = '{0}'", equipmentid);
            var data = db.SqlQueryable<EquipmentInfo>(sql).First();

            string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/getequipmentstatus";
            var body = JsonConvert.SerializeObject(new GetEquipmentStatusRequest { EquipmentId = equipmentid });

            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
            var replyItem = JsonConvert.DeserializeObject<GetEquipmentStatusResponse>(apiresult);
            if (replyItem != null) data.STATUS = replyItem.Status;

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DownloadRecipeByPanelID(string equipmentid, string panelid)
        {
            string sfis_step7_req = $"EQXXXXXX01,{panelid},7,M001603,JORDAN,,OK,SN_MODEL_NAME_INFO=???";
            string sfis_step7_res = string.Empty;
            string errmsg = string.Empty;
            if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg))
            {
                if (sfis_step7_res.ToUpper().StartsWith("OK"))
                {
                    Dictionary<string, string> sfisParameters = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                               .Where(keyValueArray => keyValueArray.Length == 2)
                               .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                    string modelName = sfisParameters["SN_MODEL_NAME_INFO"];
                    string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/downloadeffectiverecipebyrecipegroup";
                    var body = JsonConvert.SerializeObject(new DownloadEffectiveRecipeByRecipeGroupRequest { TrueName = User?.TRUENAME ?? "NA", EquipmentId = equipmentid, RecipeGroupName = modelName });
                    var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
                    var replyItem = JsonConvert.DeserializeObject<DownloadEffectiveRecipeByRecipeGroupResponse>(apiresult);
                    AddProductionLog(equipmentid, "DownloadRecipe", $"{replyItem.Result}", $"PanelId:{panelid},RecipeGroup:{modelName},RecipeName:{replyItem.RecipeName},{replyItem.Message}");

                    return Json(new { Result = replyItem.Result, Message = replyItem.Message, RecipeGroupName = modelName, RecipeName = replyItem.RecipeName });
                }
                else
                {
                    AddProductionLog(equipmentid, "DownloadRecipe", "False", $"PanelId:{panelid},Sfis error:{sfis_step7_res}");
                    return Json(new { Result = false, Message = $"Sfis error:{sfis_step7_res}" });
                }
            }
            else
            {
                AddProductionLog(equipmentid, "DownloadRecipe", "False", $"PanelId:{panelid},error:{errmsg}");
                return Json(new { Result = false, Message = $"{errmsg}" });
            }
        }

        public JsonResult SwitchRecipeByPanelID(string equipmentid, string panelid)
        {
            string sfis_step7_req = $"EQXXXXXX01,{panelid},7,M001603,JORDAN,,OK,SN_MODEL_NAME_INFO=???";
            string sfis_step7_res = string.Empty;
            string errmsg = string.Empty;
            if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg))
            {
                if (sfis_step7_res.ToUpper().StartsWith("OK"))
                {
                    Dictionary<string, string> sfisParameters = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                               .Where(keyValueArray => keyValueArray.Length == 2)
                               .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                    string modelName = sfisParameters["SN_MODEL_NAME_INFO"];
                    string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/PpSelectByRecipeGroup";
                    var body = JsonConvert.SerializeObject(new PpSelectByRecipeGroupRequest { TrueName = User?.TRUENAME ?? "NA", EquipmentId = equipmentid, RecipeGroupName = modelName });
                    var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
                    var replyItem = JsonConvert.DeserializeObject<PpSelectByRecipeGroupResponse>(apiresult);
                    AddProductionLog(equipmentid, "DownloadRecipe", $"{replyItem.Result}", $"PanelId:{panelid},RecipeGroup:{modelName},RecipeName:{replyItem.RecipeName},{replyItem.Message}");

                    return Json(new { Result = replyItem.Result, Message = replyItem.Message, RecipeGroupName = modelName, RecipeName = replyItem.RecipeName });
                }
                else
                {
                    AddProductionLog(equipmentid, "DownloadRecipe", "False", $"PanelId:{panelid},Sfis error:{sfis_step7_res}");
                    return Json(new { Result = false, Message = $"Sfis error:{sfis_step7_res}" });
                }
            }
            else
            {
                AddProductionLog(equipmentid, "DownloadRecipe", "False", $"PanelId:{panelid},error:{errmsg}");
                return Json(new { Result = false, Message = $"{errmsg}" });
            }
        }
        public bool SendMessageToSfis(string message, ref string receiveMsg, ref string errMsg)
        {
            try
            {
                TcpClient tcpSync;
                string sfisip = ConfigurationManager.AppSettings["SfisIp"].ToString();
                int sfisport = int.Parse(ConfigurationManager.AppSettings["SfisPort"]);
                IPAddress serverip = IPAddress.Parse(sfisip);
                IPEndPoint point = new IPEndPoint(serverip, sfisport);
                tcpSync = new TcpClient();
                tcpSync.Connect(point);
                if (tcpSync != null && tcpSync.Connected)
                {
                    NetworkStream nStream = tcpSync.GetStream();
                    byte[] data = new byte[1024];
                    data = Encoding.UTF8.GetBytes(message);
                    nStream.Write(data, 0, data.Length);
                    nStream.Flush();

                    int len = 0;
                    int trytimes = 3;
                    while (len == 0 && trytimes > 0)
                    {
                        data = new byte[1024];
                        nStream = tcpSync.GetStream();
                        len = nStream.Read(data, 0, data.Length);
                        trytimes--;
                    }
                    if (len > 0)
                    {
                        receiveMsg = Encoding.UTF8.GetString(data, 0, len);
                        nStream.Flush();
                        tcpSync.Close();
                        return true;
                    }
                    else
                    {
                        errMsg = "SFIS Timeout";
                        return false;
                    }
                }
                else
                {
                    errMsg = "Can not connect to SFIS";
                    return false;
                }
            }
            catch (Exception)
            {
                errMsg = "EAP send message to SFIS fail.";
                return false;
            }
        }
        public void AddProductionLog(string equipmentid, string action, string result, string message)
        {
            var db = DbFactory.GetSqlSugarClient();
            db.Insertable<RMS_PRODUCTIONLOG>(
                new RMS_PRODUCTIONLOG()
                {
                    EQUIPMENT_ID = equipmentid,
                    ACTION = action,
                    RESULT = result,
                    MESSAGE = message
                }).ExecuteCommand();
        }


    }
}
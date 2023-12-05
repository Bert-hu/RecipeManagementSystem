using Newtonsoft.Json;
using Rms.Models.DataBase.Pms;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using Rms.Web.Extensions;
using Rms.Web.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Production
{
    public class CommonProductionController : Controller
    {
        protected new PMS_USER User => Session["user_account"] as PMS_USER;
        // GET: WaferGrinding
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

        public JsonResult GetNewLog(string equipmentid, string logid)
        {
            var db = DbFactory.GetSqlSugarClient();
            var sql = string.Format(@"SELECT * FROM RMS_PRODUCTIONLOG
WHERE 1=1
{0}
AND EQUIPMENT_ID = '{1}'
AND CREATE_TIME>SYSDATE-7
ORDER BY CREATE_TIME", string.IsNullOrEmpty(logid) ? "" : $"AND CREATE_TIME>(SELECT CREATE_TIME FROM RMS_PRODUCTIONLOG WHERE ID = '{logid}')"
, equipmentid);
            var data = db.SqlQueryable<RMS_PRODUCTIONLOG>(sql).ToList();
            data = data.Skip(Math.Max(0, data.Count - 20)).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        //[LogAttribute]
        public JsonResult DownloadRecipeByLot(string eqid, string lotid)
        {
            string sfis_step7_req = $"{eqid},{lotid},7,M068397,JORDAN,,OK,MODEL_NAME=???";
            string sfis_step7_res = string.Empty;
            string errmsg = string.Empty;
            if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg))
            {
                if (sfis_step7_res.ToUpper().StartsWith("OK"))
                {

                    Dictionary<string, string> sfispara = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                    .Where(keyValueArray => keyValueArray.Length == 2)
               .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                    string rcpgroupname = sfispara["MODEL_NAME"];

                    var db = DbFactory.GetSqlSugarClient();
                    //var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == eqid && it.NAME.TrimEnd() == rcpname).First();

                    string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/downloadeffectiverecipebyrecipegroup";
                    var body = JsonConvert.SerializeObject(new DownloadEffectiveRecipeByRecipeGroupRequest { TrueName = User?.TRUENAME??"NA", EquipmentId = eqid, RecipeGroupName = rcpgroupname });

                    var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
                    var replyItem = JsonConvert.DeserializeObject<DownloadEffectiveRecipeByRecipeGroupResponse>(apiresult);

                    AddProductionLog(eqid, "DownloadRecipe", $"{replyItem.Result}", $"Lot:{lotid},RecipeGroup:{rcpgroupname},RecipeName:{replyItem.RecipeName},{replyItem.Message}");

                    return Json(new { Result = replyItem.Result, Message = replyItem.Message, RecipeGroupName = rcpgroupname, RecipeName = replyItem.RecipeName });
                }
                else
                {
                    AddProductionLog(eqid, "DownloadRecipe", "False", $"Lot:{lotid},Sfis error:{sfis_step7_res}");
                    return Json(new { Result = false, Message = $"Sfis error:{sfis_step7_res}" });
                }
            }
            else
            {
                AddProductionLog(eqid, "DownloadRecipe", "False", $"Lot:{lotid},error:{errmsg}");
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
            catch (Exception ex)
            {
                errMsg = "EAP send message to SFIS fail.";
                return false;
            }
        }

        public void AddProductionLog(string equipmentid,string action,string result,string message)
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

        public RMS_RECIPE GetRecipeWithGroupName(string equipmentid, string recipegroupname,out string errmsg)
        { 
            var db = DbFactory.GetSqlSugarClient();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(equipmentid).First();
            var recipegroup = db.Queryable<RMS_RECIPE_GROUP>().First(it => it.NAME == recipegroupname);
            if (recipegroup == null)
            {
                db.Insertable<RMS_RECIPE_GROUP>(new RMS_RECIPE_GROUP { NAME = recipegroupname }).ExecuteCommand();    
                errmsg = $"Unable to find recipe bound to '{recipegroupname}'";
                return null;
            }
            if (eqp == null)
            {                
                errmsg = "Equipment does not exist in RMS";
                return null;
            }
            var data = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == equipmentid).ToList();
            var eqrecipeids = data.Select(it => it.ID).ToList();
            var binding = db.Queryable<RMS_RECIPE_GROUP_MAPPING>().Where(it => it.RECIPE_GROUP_ID == recipegroup.ID && eqrecipeids.Contains(it.RECIPE_ID)).First();
            if (binding == null)
            {
                errmsg = $"Unable to find recipe bound to '{recipegroupname}'";
                return null;
            }
            var recipe = db.Queryable<RMS_RECIPE>().In(binding.RECIPE_ID).First();
            errmsg = null;
            return recipe;
        }


        public JsonResult LotEnd(string equipmentid, string lotid)
        {
            string sfis_step2_req = $"{equipmentid},{lotid},2,M068397,JORDAN,,OK,";
            string sfis_step2_res = string.Empty;
            string errmsg = string.Empty;
            string repmsg = $"Lot:{lotid}";
            bool result = false;
            if (SendMessageToSfis(sfis_step2_req, ref sfis_step2_res, ref errmsg))
            {
                if (sfis_step2_res.ToUpper().StartsWith("OK"))//SFIS获取LOT INFO成功
                {
                    repmsg += " LotEnd OK";
                    result = true;
                }
                else//SFIS获取LOT INFO失败
                {
                    repmsg += $"Sfis reply FAIL:{sfis_step2_res}";
                }
            }
            else
            {
                repmsg += $"Can not connect to SFIS";
            }
            AddProductionLog(equipmentid, "LotEnd", result.ToString(), repmsg);
            return Json(new { Result = result, Message = repmsg }, JsonRequestBehavior.AllowGet);
        }


    }
}
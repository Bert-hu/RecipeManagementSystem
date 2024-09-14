using Newtonsoft.Json;
using Renci.SshNet;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using Rms.Web.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Production
{
    public class WaferAOIController : CommonProductionController
    {

        // GET: WaferGrinding
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetEquipments()
        {
            var db = DbFactory.GetSqlSugarClient();
            var data = db.Queryable<RMS_EQUIPMENT>().Where(it => it.TYPE == "WaferAOI").ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DownloadMapByLot(string eqid, string lotid)
        {
            try
            {
                string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/downloadmapbylot";
                var body = JsonConvert.SerializeObject(new DownloadMapByLotRequest { EquipmentId = eqid,LotId = lotid }) ;
                var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
                if (apiresult != null)
                {
                    var reply = JsonConvert.DeserializeObject<DownloadMapByLotResponse>(apiresult);
                    AddProductionLog(eqid, "DownloadMap", reply.Result.ToString(), $"Lot:{lotid},Message: {reply.Message}");
                    return Json(new { Result = reply.Result, Message = $"{reply.Message}" });
                }
                else
                {
                    AddProductionLog(eqid, "DownloadMap", "False", $"Lot:{lotid},Message: Api fail :{apiURL}");
                    return Json(new { Result = false, Message = $"Lot:{lotid},Message: Api fail :{apiURL}" });
                }
      
            }
            catch (Exception ex)
            {
                AddProductionLog(eqid, "DownloadMap", "False", $"Lot:{lotid},error:{ex.Message}");
                return Json(new { Result = false, Message = $"{ex.Message}" });
            }
        }

        public JsonResult UploadMapByLot(string eqid, string lotid)
        {
            try
            {
                string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/uploadmapbylot";
                var body = JsonConvert.SerializeObject(new UploadMapByLotRequest { EquipmentId = eqid, LotId = lotid });
                var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
                if (apiresult != null)
                {
                    var reply = JsonConvert.DeserializeObject<DownloadMapByLotResponse>(apiresult);
                    AddProductionLog(eqid, "UploadMap", reply.Result.ToString(), $"Lot:{lotid},Message: {reply.Message}");
                    return Json(new { Result = reply.Result, Message = $"{reply.Message}" });
                }
                else
                {
                    AddProductionLog(eqid, "UploadMap", "False", $"Lot:{lotid},Message: Api fail :{apiURL}");
                    return Json(new { Result = false, Message = $"Lot:{lotid},Message: Api fail :{apiURL}" });
                }

            }
            catch (Exception ex)
            {
                AddProductionLog(eqid, "UploadMap", "False", $"Lot:{lotid},error:{ex.Message}");
                return Json(new { Result = false, Message = $"{ex.Message}" });
            }
        }


        public override JsonResult LotEnd(string equipmentid, string lotid)
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
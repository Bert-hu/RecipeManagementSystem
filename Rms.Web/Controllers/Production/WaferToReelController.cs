using Newtonsoft.Json;
using Renci.SshNet;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using Rms.Web.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Production
{
    public class WaferToReelController : CommonProductionController
    {

        // GET: WaferGrinding
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetEquipments()
        {
            var db = DbFactory.GetSqlSugarClient();
            var data = db.Queryable<RMS_EQUIPMENT>().Where(it => it.TYPE == "WaferToReel_MERLIN50K").ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }



        public JsonResult DownloadMapByLot(string eqid, string lotid)
        {
            try
            {
                string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/downloadmapbylot";
                var body = JsonConvert.SerializeObject(new DownloadMapByLotRequest { EquipmentId = eqid, LotId = lotid });
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

    }
}
using Newtonsoft.Json;
using Rms.Models.DataBase.Pms;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using Rms.Web.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
            var body = JsonConvert.SerializeObject(new GetEquipmentStatusRequest {  EquipmentId = equipmentid });

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
            public string DATETIMESTR { get{ return DATETIME.ToString(); }  }
            public string STATUS { get; set; }
        }
    }
}
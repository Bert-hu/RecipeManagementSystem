using Rms.Models.DataBase.Pms;
using Rms.Models.DataBase.Rms;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Production
{
    public class WaferGrindingController : CommonProductionController
    {
      
        // GET: WaferGrinding
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetEquipments()
        {
            var db = DbFactory.GetSqlSugarClient();
            var data = db.Queryable<RMS_EQUIPMENT>().Where(it => it.TYPE == "WaferGrinding").ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
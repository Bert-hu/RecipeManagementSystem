using Rms.Models.DataBase.Pms;
using Rms.Models.DataBase.Rms;
using Rms.Utils;
using Rms.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Ems
{
    public class EquipmentController : BaseController
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
            return Json(new { eqpData, data =  eqpData, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Edit(string EQID, string TYPEID, string field, string value)
        {
            try
            {                
                return Json(new { message = "更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { message = "更新失败，" + ex.Message });
            }

        }

    }
}
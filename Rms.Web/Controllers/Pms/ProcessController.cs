
using Rms.Models.DataBase.Pms;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rms.Web.Controllers
{
    public class ProcessController : BaseController
    {
        // GET: Process
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EditRoles(string processid, string field)
        {
            ViewBag.processid = processid;
            ViewBag.field = field;
            return View();
        }

        public JsonResult Get(int page, int limit)
        {
            var db = DbFactory.GetSqlSugarClient();
            int totalnum = 0;
            var data = db.Queryable<PMS_PROCESS>().ToPageList(page, limit, ref totalnum);
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSelectedRoles(int page, int limit, string selectedID, string field)
        {
            var db = DbFactory.GetSqlSugarClient();
            int totalnum = 0;
            List<string> selectdata;
            if (field == "ROLES_SUBMIT")
            {
                selectdata = db.Queryable<PMS_PROCESS>().First(it => it.ID == int.Parse(selectedID))._ROLES_SUBMIT;
            }
            else
            {
                selectdata = db.Queryable<PMS_PROCESS>().First(it => it.ID == int.Parse(selectedID))._ROLES_NOTIFY;
            }
            var roles = db.Queryable<PMS_ROLE>().ToList();
            var data = roles.Select(it => new
            {
                ID = it.ID,
                NAME = it.NAME,
                DESCRIPTION = it.DESCRIPTION,
                LAY_CHECKED = selectdata.Contains(it.ID)
            }).ToList();
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetSubmitRoles(string processid, string[] roleids)
        {
            var db = DbFactory.GetSqlSugarClient();
            var data = db.Queryable<PMS_PROCESS>().In(processid).First();
            //插入新的
            data._ROLES_SUBMIT = roleids?.ToList() ?? new List<string>();
            db.Updateable<PMS_PROCESS>(data).UpdateColumns(it => new { it.ROLES_SUBMIT }).ExecuteCommand();
            return Json(new { result = true, message = "更新成功" });
        }

   
        public JsonResult SetNotifyRoles(string processid, string[] roleids)
        {
            var db = DbFactory.GetSqlSugarClient();
            var data = db.Queryable<PMS_PROCESS>().In(processid).First();
            //插入新的
            data._ROLES_NOTIFY =  roleids?.ToList()?? new List<string>();
            db.Updateable<PMS_PROCESS>(data).UpdateColumns(it => new { it.ROLES_NOTIFY }).ExecuteCommand();
            return Json(new { result = true, message = "更新成功" });
        }
    }
}
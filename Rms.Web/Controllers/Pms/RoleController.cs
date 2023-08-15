using Microsoft.Ajax.Utilities;
using Rms.Models.DataBase.Pms;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Mvc;

namespace Rms.Web.Controllers
{
    public class RoleController : BaseController
    {
        // GET: Role
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Add()
        {
            return View();
        }
        public ActionResult ModuleRole(string RoleID)
        {
            ViewBag.RoleID = RoleID;
            return View();
        }
        public JsonResult Get(int page, int limit)
        {
            var db = DbFactory.GetSqlSugarClient();
            int totalnum = 0;
            var data = db.Queryable<PMS_ROLE>().ToPageList(page, limit, ref totalnum);
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSelectedModuleRole(int page, int limit, string selectedID)
        {
            var db = DbFactory.GetSqlSugarClient();
            int totalnum = 0;
            var selectdata = db.Queryable<PMS_MODULEROLE>().Where(it => it.ROLE_ID == selectedID).Select(it => it.MODULE_ID).ToList();
            var modules = db.Queryable<PMS_MODULE>().ToList();
            var data = modules.Select(it => new
            {
                ID = it.ID,
                CLASSNAME = it.CLASSNAME,
                NAME = it.NAME,
                LINKURL = it.LINKURL,
                LAY_CHECKED = selectdata.Contains(it.ID)
            }).ToList();
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult SetModuleRole(string roleid, string[] moduleids)
        {
            var db = DbFactory.GetSqlSugarClient();
            //删除旧的
            db.Deleteable<PMS_MODULEROLE>().Where(it => it.ROLE_ID == roleid).ExecuteCommand();
            //插入新的
            var insertdata = moduleids.Select(it => new PMS_MODULEROLE() { ROLE_ID = roleid, MODULE_ID = it }).ToList();
            db.Insertable<PMS_MODULEROLE>(insertdata).ExecuteCommand();
            return Json(new { result = true, message = "更新成功" });

        }
        public JsonResult AddRole()
        {
            var db = DbFactory.GetSqlSugarClient();

            var id = Request["data[id]"];
            var name = Request["data[name]"];
            var description = Request["data[description]"];
            if (db.Queryable<PMS_ROLE>().First(it => it.ID == id) == null)
            {
                db.Insertable<PMS_ROLE>(
                    new PMS_ROLE
                    {
                        ID = id,
                        NAME = name,
                        DESCRIPTION = description,
                        INUSED = 1,
                    }
                    ).ExecuteCommand();
            }
            else
            {
                return Json(new { result = false, message = "ID重复" });
            }

            return Json(new { result = true, message = "添加成功" });
        }
    }

  
}
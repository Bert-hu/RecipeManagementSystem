

using Rms.Models.DataBase.Pms;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Rms.Web.Controllers
{
    public class ModuleController : BaseController
    {
        // GET: Module
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }



        public JsonResult Get(int page, int limit)
        {
            int totalCount = 0;
            var db = DbFactory.GetSqlSugarClient();

            var data = db.Queryable<PMS_MODULE>().ToList();
            return Json(new { data = data, code = 0, count = totalCount }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddModule()
        {
            var _class = Request["data[class]"];
            var name = Request["data[name]"];
            var linkurl = Request["data[linkurl]"];
            var controller = Request["data[controller]"];
            var description = Request["data[description]"];
            var db = DbFactory.GetSqlSugarClient();

            db.Insertable<PMS_MODULE>(
                    new PMS_MODULE
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        CLASSNAME = _class,
                        NAME = name,
                        DESCRIPTION = description,
                        LINKURL = linkurl,
                        INUSED = 1,
                        CONTROLLER = controller,
                    }
                    ).ExecuteCommand();


            return Json(new { result = true, message = "添加成功" });
        }

    }
}
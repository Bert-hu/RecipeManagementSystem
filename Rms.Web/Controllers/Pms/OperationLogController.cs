
using Rms.Models.DataBase.Pms;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Rms.Web.Controllers
{
    public class OperationLogController : BaseController
    {

        // GET: OperationLog
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetTableData(int page, int limit)
        {
            var db = DbFactory.GetSqlSugarClient();
            var totalNum = 0;
            var pageList = db.Queryable<PMS_OPERATIONLOG>().OrderByDescending(it => it.CREATETIME).ToPageList(page, limit,ref totalNum);
            return Json(new { data = pageList, code = 0, count = totalNum }, JsonRequestBehavior.AllowGet);
        }
    }
}
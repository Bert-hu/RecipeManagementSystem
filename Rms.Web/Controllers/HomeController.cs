//using FCT.ADC.Model.VMS;
//using FCT.Web.LightOffFloor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Utils;

namespace Rms.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //如果未登录，则跳转到登录页面
            if (Session["user_account"] == null)
            {
                return RedirectToAction("login", "account");
            }
            return View();
        }

        
        public JsonResult GetEQPs(int page, int limit)
        {
            var db = DbFactory.GetSqlSugarClient();
            var totalnum = 0;
            var eqpData = db.Queryable<RMS_EQUIPMENT>().Where(it => it.RECIPE_TYPE == "secsByte" || it.RECIPE_TYPE == "onlyName").OrderBy(it => it.ORDERSORT).ToPageList(page, limit,ref totalnum);


            return Json(new { eqpData, code = 0, count = totalnum}, JsonRequestBehavior.AllowGet);
        }
    }
}
using Rms.Models.DataBase.Rms;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Rms
{
    public class RecipeParameterController : Controller
    {
        // GET: RecipeParameter
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SetParams()
        {
            return View();
        }
        public ActionResult ParamsDictionary()
        {
            return View();
        }

        public ActionResult SetDictionary()
        {
            return View();
        }

        public JsonResult GetRcpParams(int page, int limit, string rcpFilter)
        {
            var db = DbFactory.GetSqlSugarClient();
            var scopedata = db.Queryable<RMS_PARAMETER_SCOPE>().Where(it => it.RecipeID == rcpFilter).OrderBy(it => new { it.SOURCE, it.ParamKey}).ToList();
            var recipe = db.Queryable<RMS_RECIPE>().In(rcpFilter).First();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
            var type =db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            var data_dictionary = db.Queryable<RMS_PARAMETER_DIC>().Where(it => it.EQ_TYPE_ID == type.ID).ToList();

            var result = from a in scopedata
                         join b in data_dictionary on a.ParamKey equals b.Key
                         select new 
                         {
                             ID = a.ID,
                             ParaKey = a.ParamKey,
                             Name = b.Name,
                             RecipeID = a.RecipeID,
                             Type = a.Type,
                             LCL = a.LCL,
                             UCL = a.UCL,
                             EnumValue = a.EnumValue,
                             LastEditor = a.LastEditor,
                             LastEditTime = a.LastEditTime
                         };

            var pageList = result.Skip((page - 1) * limit).Take(limit).ToList();

            return Json(new { data = pageList, code = 0, count = result.Count() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetParamsList(string paramFilter)
        {
            using (var db = DbFactory.GetSqlSugarClient())
            {

                var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.ID == paramFilter)?.First();
                var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
                var data = db.Queryable<RMS_PARAMETER_DIC>().Where(it => it.EQ_TYPE_ID == eqp.TYPE).ToList();
                string option = string.Empty;
                foreach (var item in data)
                {
                    option += "<option value=\"" + item.Key + "\" >" + item.Name + "</option>";
                }
                return Json(option);
            }

        }
    }
}
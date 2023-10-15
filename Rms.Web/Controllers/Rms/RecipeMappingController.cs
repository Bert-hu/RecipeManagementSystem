using Rms.Models.DataBase.Rms;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Rms.Models.DataBase.Pms;

namespace Rms.Web.Controllers.Rms
{
    public class RecipeMappingController : BaseController
    {
        // GET: RecipeMapping
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetRecipeGroups(int page, int limit)
        {
            var db = DbFactory.GetSqlSugarClient();
            var totalnum = 0;
            var data = db.Queryable<RMS_RECIPE_GROUP>().ToPageList(page, limit, ref totalnum);


            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLines()
        {
            var db = DbFactory.GetSqlSugarClient();
            var totalnum = 0;
            var data = db.Queryable<RMS_EQUIPMENT>().Select(it => it.LINE).Distinct().ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddRecipeGroup(string recipegroupname)
        {
            var db = DbFactory.GetSqlSugarClient();
            var group = db.Queryable<RMS_RECIPE_GROUP>().Where(it => it.NAME == recipegroupname).First();
            if (group == null)
            {
                db.Insertable<RMS_RECIPE_GROUP>(new RMS_RECIPE_GROUP { NAME = recipegroupname }).ExecuteCommand();
                return Json(new { Result = true, Message = "添加成功" });
            }
            else
            {
                return Json(new { Result = false, Message = "已存在Recipe Group，请勿重复添加。" });
            }
        }

        public JsonResult GetEquipment(int page, int limit, string recipegroup_id, string line)
        {
            var db = DbFactory.GetSqlSugarClient();
            var sql = string.Format(@"select equipment.ID EQUIPMENT_ID,equipment.NAME EQUIPMENT_NAME,'{1}' RECIPE_GROUP_ID,recipe.RECIPE_NAME,recipe.VERSION_EFFECTIVE_ID from 
(select * from RMS_EQUIPMENT
where line = '{0}' 
and RECIPE_TYPE = 'secsByte'
) equipment
LEFT JOIN
(select rr.RECIPE_GROUP_ID,rg.NAME RECIPE_GROUP_NAME,rr.NAME RECIPE_NAME,rr.VERSION_EFFECTIVE_ID ,rr.EQUIPMENT_ID 
from RMS_RECIPE_GROUP rg,RMS_RECIPE rr
where rg.ID = '{1}'
and rg.ID = rr.RECIPE_GROUP_ID) recipe
on equipment.ID = recipe.EQUIPMENT_ID
ORDER BY equipment.ORDERSORT", line, recipegroup_id);
            var totalnum = 0;
            var data = db.SqlQueryable<EqupmentTableData>(sql).ToPageList(page, limit, ref totalnum);
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BindingRecipePage(string EQUIPMENT_ID, string RECIPE_GROUP_ID)
        {
            ViewBag.EQUIPMENT_ID = EQUIPMENT_ID;
            ViewBag.RECIPE_GROUP_ID = RECIPE_GROUP_ID;
            return View();
        }

        public JsonResult GetBindingRecipe(int page, int limit, string EQUIPMENT_ID, string RECIPE_GROUP_ID)
        {
            var db = DbFactory.GetSqlSugarClient();
            int totalnum = 0;
            var data = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == EQUIPMENT_ID).ToPageList(page, limit, ref totalnum);
            var viewdata = data.Select(it => new
            {
                ID = it.ID,
                NAME = it.NAME,
                LAY_CHECKED = it.RECIPE_GROUP_ID == RECIPE_GROUP_ID,
            });
            return Json(new { data = viewdata, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetRecipeBinding(string EQUIPMENT_ID, string RECIPE_GROUP_ID, string RECIPE_ID)
        {
            var db = DbFactory.GetSqlSugarClient();
            //删除旧的绑定
            db.Updateable<RMS_RECIPE>().SetColumns(it => it.RECIPE_GROUP_ID ==null).Where(it => it.EQUIPMENT_ID == EQUIPMENT_ID && it.RECIPE_GROUP_ID == RECIPE_GROUP_ID).ExecuteCommand();

            //加入新的
            if (!string.IsNullOrEmpty(RECIPE_ID))
            {
                db.Updateable<RMS_RECIPE>().SetColumns(it => it.RECIPE_GROUP_ID == RECIPE_GROUP_ID).Where(it => it.EQUIPMENT_ID == EQUIPMENT_ID && it.ID == RECIPE_ID).ExecuteCommand();
            }
           
            return Json(new { result = true, message = "更新成功" });

        }

        public class EqupmentTableData
        {
            public string EQUIPMENT_ID { get; set; }
            public string EQUIPMENT_NAME { get; set; }
            public string RECIPE_GROUP_ID { get; set; }
            public string RECIPE_NAME { get; set; }
            public string VERSION_EFFECTIVE_ID { get; set; }
        }
    }
}
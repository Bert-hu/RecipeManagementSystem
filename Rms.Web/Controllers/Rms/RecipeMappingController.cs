using Rms.Models.DataBase.Pms;
using Rms.Models.DataBase.Rms;
using Rms.Utils;
using Rms.Web.Utils;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Rms
{
    public class RecipeMappingController : BaseController
    {
        // GET: RecipeMapping
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AliasMapping()
        {
            return View();
        }

        public JsonResult GetRecipeGroups(int page, int limit)
        {
            var totalnum = 0;
            var data = db.Queryable<RMS_RECIPE_GROUP>().ToPageList(page, limit, ref totalnum);


            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLines()
        {
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

            var sql = string.Format(@"select equipment.ID EQUIPMENT_ID,equipment.NAME EQUIPMENT_NAME,'{1}' RECIPE_GROUP_ID,recipe.RECIPE_NAME,recipe.VERSION_EFFECTIVE_ID from 
(select * from RMS_EQUIPMENT
where line = '{0}' 
and (RECIPE_TYPE = 'secsByte' or RECIPE_TYPE = 'onlyName')
) equipment
LEFT JOIN
(select rr.RECIPE_GROUP_ID,rg.NAME RECIPE_GROUP_NAME,rr.NAME RECIPE_NAME,rr.VERSION_EFFECTIVE_ID ,rr.EQUIPMENT_ID 
from RMS_RECIPE_GROUP rg,RMS_RECIPE rr,RMS_RECIPE_GROUP_MAPPING rgm
where rg.ID = '{1}'
and rg.ID = rgm.RECIPE_GROUP_ID
and rr.ID = rgm.RECIPE_ID
) recipe
on equipment.ID = recipe.EQUIPMENT_ID
ORDER BY equipment.ORDERSORT", line, recipegroup_id);
            var totalnum = 0;
            var data = db.SqlQueryable<EqupmentTableData>(sql).ToPageList(page, limit, ref totalnum);
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEquipments(int page, int limit, string recipegroup_id, string processfilter)
        {
            var totalnum = 0;
            var eqptypes = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipmenttypeids).ToList();
            if (!string.IsNullOrEmpty(processfilter))
            {
                eqptypes = eqptypes.Where(it => it.PROCESS == processfilter).OrderBy(it => it.ORDERSORT).ToList();
            }
            var eqps = db.Queryable<RMS_EQUIPMENT>().OrderBy(it => it.ORDERSORT).ToList();
            var mappings = db.Queryable<RMS_RECIPE_GROUP_MAPPING>().Where(it => it.RECIPE_GROUP_ID == recipegroup_id).ToList();

            var submappings = db.Queryable<RMS_RECIPE_GROUP_MAPPING_SUBRECIPE>().Where(it => it.RECIPE_GROUP_ID == recipegroup_id).ToList();
            var recipes = db.Queryable<RMS_RECIPE>().In(mappings.Select(it => it.RECIPE_ID)).ToList();
            var subrecipes = db.Queryable<RMS_RECIPE>().In(submappings.Select(it => it.RECIPE_ID_LIST).SelectMany(it => it).Distinct()).ToList();

            //var data = eqps.Join(eqptypes, eqp => eqp.TYPE, eqptype => eqptype.ID, (eqp, eqptype) => new { eqp = eqp, eqptype = eqptype }) //全连接筛选设备
            //     .OrderBy(x => x.eqptype.ORDERSORT)  //按type排序设备
            //     .GroupJoin(recipes, filtereqp => filtereqp.eqp.ID, rcp => rcp.EQUIPMENT_ID, (filtereqps, rcps) =>
            //     new EqupmentTableData
            //     {
            //         LINE = filtereqps.eqp.LINE,
            //         EQUIPMENT_ID = filtereqps.eqp.ID,
            //         EQUIPMENT_NAME = filtereqps.eqp.NAME,
            //         EQUIPMENT_TYPE_NAME = filtereqps.eqptype.NAME,
            //         RECIPE_GROUP_ID = recipegroup_id,
            //         RECIPE_NAME = rcps.Select(x => x.NAME).FirstOrDefault(),
            //         VERSION_EFFECTIVE_ID = rcps.Select(x => x.VERSION_EFFECTIVE_ID).FirstOrDefault(),

            //     }).ToList();

            var data = eqps.Join(eqptypes, eqp => eqp.TYPE, eqptype => eqptype.ID, (eqp, eqptype) => new { eqp = eqp, eqptype = eqptype })
                .OrderBy(x => x.eqptype.ORDERSORT)
                .GroupJoin(recipes, filtereqp => filtereqp.eqp.ID, rcp => rcp.EQUIPMENT_ID, (filtereqps, rcps) => new { filtereqps, rcps })      // 新增与子配方的连接
      .GroupJoin(subrecipes, combined => combined.filtereqps.eqp.ID, subrcp => subrcp.EQUIPMENT_ID,
          (combined, subrcps) => new EqupmentTableData
          {
              LINE = combined.filtereqps.eqp.LINE,
              EQUIPMENT_ID = combined.filtereqps.eqp.ID,
              EQUIPMENT_NAME = combined.filtereqps.eqp.NAME,
              EQUIPMENT_TYPE_NAME = combined.filtereqps.eqptype.NAME,
              RECIPE_GROUP_ID = recipegroup_id,
              RECIPE_NAME = combined.rcps.Select(x => x.NAME).FirstOrDefault(),
              VERSION_EFFECTIVE_ID = combined.rcps.Select(x => x.VERSION_EFFECTIVE_ID).FirstOrDefault(),
              // 新增子配方名称，用逗号拼接
              SUB_RECIPE_NAMES = string.Join(",", subrcps.Select(x => x.NAME).Where(name => !string.IsNullOrEmpty(name)))
          }).ToList();
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
            var data = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == EQUIPMENT_ID).ToList();
            var eqrecipeids = data.Select(it => it.ID).ToList();
            var oldbinding = db.Queryable<RMS_RECIPE_GROUP_MAPPING>().Where(it => it.RECIPE_GROUP_ID == RECIPE_GROUP_ID && eqrecipeids.Contains(it.RECIPE_ID)).First();
            var viewdata = data.Select(it => new
            {
                ID = it.ID,
                NAME = it.NAME,
                LAY_CHECKED = it.ID == oldbinding?.RECIPE_ID,
            });
            return Json(new { data = viewdata, code = 0, count = viewdata.Count() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetRecipeBinding(string EQUIPMENT_ID, string RECIPE_GROUP_ID, string RECIPE_ID)
        {
            var db = DbFactory.GetSqlSugarClient();
            //当前eqid下所有recipe
            var recipes = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == EQUIPMENT_ID).ToList();
            var eqrecipeids = recipes.Select(it => it.ID).ToList();
            //删除旧的绑定
            var oldbindings = db.Queryable<RMS_RECIPE_GROUP_MAPPING>().Where(it => it.RECIPE_GROUP_ID == RECIPE_GROUP_ID && eqrecipeids.Contains(it.RECIPE_ID)).ToList();
            db.Deleteable<RMS_RECIPE_GROUP_MAPPING>(oldbindings).ExecuteCommand();

            //加入新的
            if (!string.IsNullOrEmpty(RECIPE_ID))
            {
                db.Insertable<RMS_RECIPE_GROUP_MAPPING>(new RMS_RECIPE_GROUP_MAPPING { RECIPE_ID = RECIPE_ID, RECIPE_GROUP_ID = RECIPE_GROUP_ID }).ExecuteCommand();
            }

            //send mail
            var eqp = db.Queryable<RMS_EQUIPMENT>().Where(it => it.ID == EQUIPMENT_ID).First();
            var eqpType = db.Queryable<RMS_EQUIPMENT_TYPE>().Where(it => it.ID == eqp.TYPE).First();
            var roles = eqpType.FLOWROLEIDS;
            var users = db.Queryable<PMS_USER>().Where(it => roles.Contains(it.ROLEID)).ToList();
            var mailAddrs = users.Select(it => it.EMAIL).Where(it => !string.IsNullOrEmpty(it)).ToList();

            var recipeGroup = db.Queryable<RMS_RECIPE_GROUP>().Where(it => it.ID == RECIPE_GROUP_ID).First();
            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.ID == RECIPE_ID).First();
            var subject = $"Recipe Binding Modify Notification";

            StringBuilder tableBuilder = new StringBuilder();
            tableBuilder.Append($"<p>{EQUIPMENT_ID} recipe 绑定已修改");
            tableBuilder.Append("<table class=\"styled-table\">");
            tableBuilder.Append("<tr><th>EQID</th><th>Recipe Group</th><th>Recipe Name</th><th>User Name</th></tr>");
            tableBuilder.Append($"<tr><td>{eqp.NAME}</td><td>{recipeGroup.NAME}</td><td>{recipe.NAME}</td><td>{User?.TRUENAME}</td></tr>");

            tableBuilder.Append("</table>");
            string tableStyle = @"
    <style>
        .styled-table {
            border-collapse: collapse;
            font-size: 14px;
            font-family: Arial, sans-serif;
            min-width: 100%;
            width: auto;
            white-space: nowrap;
            background-color: #fff;
            color: #000;
            border: 1px solid #ccc;
        }
        .styled-table td, .styled-table th {
            padding: 12px 15px;
            border-right: 1px solid #ccc;
            text-align: left;
        }
        .styled-table th {
            background-color: #eee;
            color: #000;
            border: 1px solid #ccc;
            font-weight: bold;
        }
    </style>
";
            string content = tableStyle + tableBuilder.ToString();
            MailHelper.SendMail(mailAddrs.ToArray(), subject, content);
            return Json(new { result = true, message = "更新成功" });

        }

        public ActionResult BindingSubRecipePage(string EQUIPMENT_ID, string RECIPE_GROUP_ID)
        {
            ViewBag.EQUIPMENT_ID = EQUIPMENT_ID;
            ViewBag.RECIPE_GROUP_ID = RECIPE_GROUP_ID;
            return View();
        }

        public JsonResult GetBindingSubRecipe(int page, int limit, string EQUIPMENT_ID, string RECIPE_GROUP_ID)
        {
            var db = DbFactory.GetSqlSugarClient();
            var data = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == EQUIPMENT_ID).ToList();
            var eqrecipeids = data.Select(it => it.ID).ToList();
            var oldbinding = db.Queryable<RMS_RECIPE_GROUP_MAPPING_SUBRECIPE>().Where(it => it.RECIPE_GROUP_ID == RECIPE_GROUP_ID).First();
            var viewdata = data.Select(it => new
            {
                ID = it.ID,
                NAME = it.NAME,
                LAY_CHECKED = oldbinding?.RECIPE_ID_LIST.Contains(it.ID) ?? false,
            });
            return Json(new { data = viewdata, code = 0, count = viewdata.Count() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetSubRecipeBinding(string EQUIPMENT_ID, string RECIPE_GROUP_ID, string[] RECIPE_ID_LIST)
        {
            var db = DbFactory.GetSqlSugarClient();
            //当前eqid下所有recipe
            var recipes = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == EQUIPMENT_ID).ToList();
            var eqrecipeids = recipes.Select(it => it.ID).ToList();
            //删除旧的绑定
            var oldbindings = db.Queryable<RMS_RECIPE_GROUP_MAPPING_SUBRECIPE>().Where(it => it.RECIPE_GROUP_ID == RECIPE_GROUP_ID).ToList();
            db.Deleteable<RMS_RECIPE_GROUP_MAPPING_SUBRECIPE>(oldbindings).ExecuteCommand();
            //加入新的
            if (RECIPE_ID_LIST != null && RECIPE_ID_LIST.Length > 0)
            {
                db.Insertable<RMS_RECIPE_GROUP_MAPPING_SUBRECIPE>(new RMS_RECIPE_GROUP_MAPPING_SUBRECIPE
                {
                    RECIPE_ID_LIST = RECIPE_ID_LIST.Distinct().ToList(),
                    RECIPE_GROUP_ID = RECIPE_GROUP_ID
                }).ExecuteCommand();
            }
            return Json(new { result = true, message = "更新成功" });
        }

        public class EqupmentTableData
        {
            public string LINE { get; set; }
            public string EQUIPMENT_ID { get; set; }
            public string EQUIPMENT_NAME { get; set; }
            public string EQUIPMENT_TYPE_NAME { get; set; }
            public string RECIPE_GROUP_ID { get; set; }
            public string RECIPE_NAME { get; set; }
            public string VERSION_EFFECTIVE_ID { get; set; }
            public string SUB_RECIPE_NAMES { get; set; }
        }


        //****************以下是ProjectMapping的方法

        public JsonResult GetEquipmentType()
        {
            var eqTypes = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipmenttypeids).ToList();

            return Json(eqTypes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRecipeAlias(int page, int limit, string equipmentType)
        {
            var data = db.Queryable<RMS_RECIPE_NAME_ALIAS>().Where(it => it.EQUIPMENT_TYPE_ID == equipmentType).ToList();
            var totalnum = data.Count();
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult AddRecipeAliasConfig(string equipmentTypeId, string recipeName)
        {
            var config = db.Queryable<RMS_RECIPE_NAME_ALIAS>().First(it => it.EQUIPMENT_TYPE_ID == equipmentTypeId && it.RECIPE_NAME == recipeName);
            if (config != null)
            {
                return Json(new { result = false, message = $"Fail，Recipe {recipeName} already exists" });
            }

            db.Insertable(new RMS_RECIPE_NAME_ALIAS
            {
                EQUIPMENT_TYPE_ID = equipmentTypeId,
                RECIPE_NAME = recipeName,
                RECIPE_ALIAS = new System.Collections.Generic.List<string>()
            }).ExecuteCommand();

            return Json(new { result = true, message = "Add successfully" });
        }

        public JsonResult DeleteRecipeAliasConfig(string configId)
        {
            var config = db.Deleteable<RMS_RECIPE_NAME_ALIAS>().Where(it => it.ID == configId).ExecuteCommand();

            return Json(new { result = true, message = "Delete successfully" });
        }

        public ActionResult AliasConfig(string configId)
        {
            ViewBag.ConfigId = configId;
            return View();
        }

        public JsonResult GetAliasConfigById(string configId)
        {
            var config = db.Queryable<RMS_RECIPE_NAME_ALIAS>().First(it => it.ID == configId);
            var data = config.RECIPE_ALIAS.Select(it => new { RecipeAlias = it }).ToList();
            return Json(new { data = data, code = 0, count = data.Count }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateAliasConfig(string configId, string[] recipeAlias)
        {
            try
            {
                var config = db.Queryable<RMS_RECIPE_NAME_ALIAS>().InSingle(configId);

                config.RECIPE_ALIAS = recipeAlias.Distinct().ToList();
                db.Updateable(config).ExecuteCommand();

                return Json(new { result = true, message = "Update successfully" });
            }
            catch (System.Exception ex)
            {
                return Json(new { result = false, message = ex.Message });
            }
        }
    }
}
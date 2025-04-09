using Rms.Models.DataBase.Pms;
using Rms.Models.DataBase.Rms;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Rms.Web.Controllers.Ems
{
    public class EquipmentTypeController : BaseController
    {
        // GET: EquipmentType
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TypeRole(string TYPEID)
        {
            ViewBag.TYPEID = TYPEID;
            return View();
        }

        public ActionResult AuditProcessPage(string TYPEID)
        {
            ViewBag.TYPEID = TYPEID;
            return View();
        }

        public JsonResult GetProcesses()
        {
            var eqptypes = db.Queryable<RMS_EQUIPMENT_TYPE>().ToList();
            var processes = eqptypes.Select(it => it.PROCESS).Distinct().ToList();
            return Json(processes, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetEquipmentTypes(int page, int limit, string processfilter = "", string searchText = "")
        {
            var totalnum = 0;
            var eqptypes = db.Queryable<RMS_EQUIPMENT_TYPE>().ToList();
            if (!string.IsNullOrEmpty(processfilter))
            {
                eqptypes = eqptypes.Where(it => it.PROCESS == processfilter).OrderBy(it => it.ORDERSORT).ToList();
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                eqptypes = eqptypes.Where(it => (it.ID + it.NAME + it.PROCESS + it.VENDOR + it.TYPE + it.GOLDEN_EQID).ToUpper().Contains(searchText.ToUpper())).ToList();
            }

            totalnum = eqptypes.Count;
            var pageeqptypes = eqptypes.Skip((page - 1) * limit).Take(limit);
            return Json(new { pageeqptypes, data = pageeqptypes, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Edit(string ID, string field, string value)
        {
            try
            {
                var item = db.Queryable<RMS_EQUIPMENT_TYPE>().In(ID).First();
                PropertyInfo property = item.GetType().GetProperty(field);
                string message = "更新成功";
                if (property != null)
                {
                    // 修改字段的值
                    if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(item, value);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        int intValue;
                        if (int.TryParse(value, out intValue))
                        {
                            property.SetValue(item, intValue);
                        }
                        else
                        {
                            message = "Invalid value for int field";
                        }
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(item, value.ToLower() == "true");
                    }
                    else
                    {
                        message = "Unsupported field type";
                    }
                }
                else
                {
                    message = "Field not found";
                }
                db.Updateable<RMS_EQUIPMENT_TYPE>(item).ExecuteCommand();

                return Json(new { message = message });
            }
            catch (Exception ex)
            {
                return Json(new { message = "更新失败，" + ex.Message });
            }

        }


        public JsonResult GetTypeRoles(string TypeId)
        {
            int totalnum = 0;
            var selectdata = db.Queryable<RMS_EQUIPMENT_TYPE>().In(TypeId).First().ROLEIDS;
            var roles = db.Queryable<PMS_ROLE>().ToList();
            var data = roles.Select(it => new
            {
                ID = it.ID,
                NAME = it.NAME,
                DESCRIPTION = it.DESCRIPTION,
                LAY_CHECKED = selectdata.Contains(it.ID)
            }).ToList();
            totalnum = data.Count;
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetTypeRoles(string TypeId, string[] RoleIds = default)
        {
            var db = DbFactory.GetSqlSugarClient();
            var item = db.Queryable<RMS_EQUIPMENT_TYPE>().In(TypeId).First();
            item.ROLEIDS = RoleIds?.ToList() ?? new List<string>();
            db.Updateable<RMS_EQUIPMENT_TYPE>(item).ExecuteCommand();
            return Json(new { result = true, message = "更新成功" });

        }

        public JsonResult GetFlowRoles(string TypeId)
        {
            int totalnum = 0;
            var selectroles = db.Queryable<RMS_EQUIPMENT_TYPE>().In(TypeId).First().FLOWROLEIDS;
            var roles = db.Queryable<PMS_ROLE>().ToList();
            return Json(new { data = roles, seldata = selectroles, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetFlowRoles(string TypeId, string[] RoleIds = default)
        {
            var db = DbFactory.GetSqlSugarClient();
            var item = db.Queryable<RMS_EQUIPMENT_TYPE>().In(TypeId).First();
            item.FLOWROLEIDS = RoleIds?.ToList() ?? new List<string>();
            db.Updateable<RMS_EQUIPMENT_TYPE>(item).ExecuteCommand();
            return Json(new { result = true, message = "更新成功" });
        }

        public ActionResult SelectMachine(string EQUIPMENT_TYPE_ID)
        {
            ViewBag.EQUIPMENT_TYPE_ID = EQUIPMENT_TYPE_ID;
            return View();
        }


        public JsonResult GetMachineList(int page, int limit, string EQUIPMENT_TYPE_ID)
        {
            var db = DbFactory.GetSqlSugarClient();
            var type = db.Queryable<RMS_EQUIPMENT_TYPE>().InSingle(EQUIPMENT_TYPE_ID);
            var machines = db.Queryable<RMS_EQUIPMENT>().Where(it => it.TYPE == EQUIPMENT_TYPE_ID).ToList();

            var viewdata = machines.Select(it => new
            {
                ID = it.ID,
                NAME = it.NAME,
                LAY_CHECKED = it.ID == type.GOLDEN_EQID,
            });
            return Json(new { data = viewdata, code = 0, count = viewdata.Count() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetGoldenMachine(string EQID, string EQUIPMENT_TYPE_ID)
        {
            var db = DbFactory.GetSqlSugarClient();
            var type = db.Queryable<RMS_EQUIPMENT_TYPE>().InSingle(EQUIPMENT_TYPE_ID);
            type.GOLDEN_EQID = EQID;
            db.Updateable<RMS_EQUIPMENT_TYPE>(type).UpdateColumns(it => new { it.GOLDEN_EQID }).ExecuteCommand();
            return Json(new { result = true, message = "更新成功" });
        }
    }
}
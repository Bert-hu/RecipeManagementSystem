﻿using Rms.Models.DataBase.Rms;
using Rms.Models.DataBase.Mms;
using SqlSugar;
using System.Linq;
using System.Web.Mvc;
using Rms.Models.DataBase.Ams;
using Rms.Web.ViewModels;
using System.Collections.Generic;

namespace Rms.Web.Controllers.Ams
{
    public class MaterialController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddConfig(string equipmentTypeId, string configId)
        {
            ViewBag.equipmentTypeId = equipmentTypeId;
            ViewBag.configId = configId;
            return View();
        }

        public ActionResult EditBinding(string configId)
        {
            ViewBag.configId = configId;
            return View();
        }

        public JsonResult GetProcesses()
        {
            var eqptypes = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipmenttypeids).ToList();
            var processes = eqptypes.Select(it => it.PROCESS).Distinct().ToList();
            return Json(processes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEquipmentType(string process)
        {
            var eqptypes = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipmenttypeids).Where(it => it.PROCESS == process).OrderBy(it => it.ORDERSORT).ToList();
            return Json(new { data = eqptypes, code = 0, count = eqptypes.Count }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetEquipment(string equipmentTypeId)
        {
            var eqps = db.Queryable<RMS_EQUIPMENT>().Where(it => it.TYPE == equipmentTypeId).ToList();
            return Json(new { data = eqps, code = 0, count = eqps.Count }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaterialDicConfig(string equipmentTypeId)
        {
            var config = db.Queryable<MMS_MATERIAL_DIC>().Where(it => it.EQUIPMENT_TYPE_ID == equipmentTypeId).OrderBy(it => it.ORDER_SORT).ToList();
            return Json(new { data = config, code = 0, count = config.Count }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaterialDicConfigId(string configId)
        {
            var config = db.Queryable<MMS_MATERIAL_DIC>().InSingle(configId);
            return Json(config, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddOrUpdateConfig(MaterialConfig config, string equipmentTypeId, string configId)
        {
            if (string.IsNullOrEmpty(configId))
            {
                var configitem = new MMS_MATERIAL_DIC
                {
                    SHOWNAME = config.SHOWNAME,
                    EQUIPMENT_TYPE_ID = equipmentTypeId,
                    TYPE = config.TYPE,
                    ORDER_SORT = config.ORDER_SORT,
                    MATERIAL_TYPE = config.MATERIAL_TYPE,
                };
                db.Insertable(configitem).ExecuteCommand();
            }
            else
            {
                var configitem = new MMS_MATERIAL_DIC
                {
                    ID = configId,
                    SHOWNAME = config.SHOWNAME,
                    TYPE = config.TYPE,
                    EQUIPMENT_TYPE_ID = equipmentTypeId,
                    ORDER_SORT = config.ORDER_SORT,
                    MATERIAL_TYPE = config.MATERIAL_TYPE
                };
                db.Updateable(configitem).ExecuteCommand();
            }

            return Json(new { result = true, message = "" });
        }

        public JsonResult DeleteConfig(string configId)
        {
            db.Deleteable<MMS_MATERIAL_DIC>().In(configId).ExecuteCommand();
            return Json(new { result = true, message = "" });
        }


        public JsonResult GetRecipeGroups(string configId)
        {
            //var config = db.Queryable<MMS_MATERIAL_DIC>().InSingle(configId);
            var bindings = db.Queryable<MMS_GROUP_MAPPING>().Includes(it => it.RECIPE_GROUP).Where(it => it.MATERIAL_DIC_ID == configId).ToList();

            var recipeGroups = db.Queryable<RMS_RECIPE_GROUP>().OrderBy(it => it.NAME).ToList();

            var showdata = recipeGroups.Select(it =>
            new
            {
                LAY_CHECKED = bindings.Any(b => b.RECIPE_GROUP_ID == it.ID),
                ID = it.ID,
                NAME = it.NAME

            }).ToList();

            return Json(new { data = showdata, code = 0, count = showdata.Count }, JsonRequestBehavior.AllowGet);



        }


        public JsonResult SetGroups(string ConfigId, List<string> GroupIds)
        {
            db.Deleteable<MMS_GROUP_MAPPING>().Where(it => it.MATERIAL_DIC_ID == ConfigId).ExecuteCommand();
            if (GroupIds != null)
            {
                var mappings = GroupIds.Select(it => new MMS_GROUP_MAPPING
                {
                    MATERIAL_DIC_ID = ConfigId,
                    RECIPE_GROUP_ID = it
                }).ToList();
                db.Insertable(mappings).ExecuteCommand();
            }
            return Json(new { result = true, message = "OK" });

        }

    }
}
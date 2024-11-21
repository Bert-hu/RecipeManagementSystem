using Rms.Models.DataBase.Pms;
using Rms.Models.DataBase.Rms;
using Rms.Utils;
using Rms.Web.Utils;
using Rms.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Ems
{
    public class EquipmentController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EqpConfig(string equipmentId)
        {
            ViewBag.EquipmentId = equipmentId;
            return View();
        }

        public JsonResult GetProcesses()
        {
            var eqptypes = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipmenttypeids).ToList();
            var processes = eqptypes.Select(it => it.PROCESS).Distinct().ToList();
            return Json(processes, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetEQPs(int page, int limit, string processfilter = "", string searchText = "")
        {

            var totalnum = 0;
            var eqptypes = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipmenttypeids).ToList();

            if (!string.IsNullOrEmpty(processfilter))
            {
                eqptypes = eqptypes.Where(it => it.PROCESS == processfilter).OrderBy(it => it.ORDERSORT).ToList();
            }

            var rawdata = db.Queryable<RMS_EQUIPMENT>().OrderBy(it => it.ORDERSORT).ToList();

            var totaleqpData = rawdata.Join(eqptypes, item => item.TYPE, order => order.ID, (eqp, eqptype) => new { eqp = eqp, eqptype = eqptype })
                 .OrderBy(x => x.eqptype.ORDERSORT)
                 .Select(x => new EquipmentViewModel
                 {
                     ID = x.eqp.ID,
                     NAME = x.eqp.NAME,
                     CREATOR = x.eqp.CREATOR,
                     CREATETIME = x.eqp.CREATETIME,
                     LASTEDITOR = x.eqp.LASTEDITOR,
                     LASTEDITTIME = x.eqp.LASTEDITTIME,
                     RECIPE_TYPE = x.eqp.RECIPE_TYPE,
                     ORDERSORT = x.eqp.ORDERSORT,
                     TYPE = x.eqp.TYPE,
                     LINE = x.eqp.LINE,
                     LASTRUN_RECIPE_ID = x.eqp.LASTRUN_RECIPE_ID,
                     LASTRUN_RECIPE_TIME = x.eqp.LASTRUN_RECIPE_TIME,
                     TYPEID = x.eqptype.ID,
                     TYPENAME = x.eqptype.NAME,
                     TYPEPROCESS = x.eqptype.PROCESS,
                     TYPEORDERSORT = x.eqptype.ORDERSORT,
                     TYPEVENDOR = x.eqptype.VENDOR,
                     TYPETYPE = x.eqptype.TYPE,
                 }).ToList();

            if (!string.IsNullOrEmpty(searchText))
            {
                totaleqpData = totaleqpData.Where(it => (it.ID + it.NAME + it.RECIPE_TYPE + it.TYPE + it.LINE + it.TYPEID + it.TYPENAME + it.TYPEPROCESS + it.TYPEVENDOR + it.TYPETYPE).ToUpper().Contains(searchText.ToUpper())).ToList();


            }
            totalnum = totaleqpData.Count;
            var eqpData = totaleqpData.Skip((page - 1) * limit).Take(limit);
            return Json(new { eqpData, data = eqpData, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Edit(string EQID, string TYPEID, string field, string value)
        {
            try
            {
                return Json(new { message = "更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { message = "更新失败，" + ex.Message });
            }

        }


        public JsonResult GetEquipmentConfig(string equipmentId)
        {
            var config = db.Queryable<RMS_EQUIPMENT>().InSingle(equipmentId);
            return Json(config, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRoleEquipmentTypes()
        {
            var eqptypes = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipmenttypeids).ToList();
            return Json(eqptypes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEquipmentConfigs(string equipmentTypeId)
        {
            var configs = db.Queryable<RMS_EQUIPMENT>().Where(it => it.TYPE == equipmentTypeId).OrderBy(it => it.ORDERSORT).ToList();
            return Json(configs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddOrUpdateEquipment(RMS_EQUIPMENT config)
        {
            var currentConfig = db.Queryable<RMS_EQUIPMENT>().InSingle(config.ID);
            if (currentConfig == null)
            {
                config.CREATOR = User.TRUENAME;
                config.LASTEDITOR = User.TRUENAME;
                db.Insertable<RMS_EQUIPMENT>(config).ExecuteCommand();
            }
            else
            {
                currentConfig.NAME = config.NAME;
                currentConfig.RECIPE_TYPE = config.RECIPE_TYPE;
                currentConfig.TYPE = config.TYPE;
                currentConfig.LINE = config.LINE;
                currentConfig.ORDERSORT = config.ORDERSORT;
                currentConfig.RECIPE_PATH = config.RECIPE_PATH;
                currentConfig.USERNAME = config.USERNAME;
                currentConfig.PASSWORD = config.PASSWORD;
                currentConfig.LASTEDITOR = User.TRUENAME;
                currentConfig.LASTEDITTIME = DateTime.Now;
                db.Updateable<RMS_EQUIPMENT>(currentConfig).ExecuteCommand();
            }
            return Json(new { result = true, message = string.Empty }, JsonRequestBehavior.AllowGet); ;
        }

        public JsonResult TestSharedFolder(string directoryPath, string username, string password)
        {
            try
            {
                using (var conn = new NetworkConnection(directoryPath, new NetworkCredential(username, password)))
                {
                    //判断directoryPath是否能访问
                    DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                    var folderExist = directoryInfo.Exists;
                    var message = folderExist ? "测试访问Recipe文件夹成功" : "文件夹路径无法访问";
                    conn.Dispose();
                    return Json(new { result = folderExist, message = message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Error:" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
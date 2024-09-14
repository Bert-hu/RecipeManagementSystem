using Antlr.Runtime.Misc;
using Rms.Models.DataBase.Ams;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Web.ViewModels;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;

namespace Rms.Web.Controllers.Ams
{
    public class AlarmConfigController : BaseController
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

        public ActionResult AlarmRecords()
        {
            return View();
        }
        public ActionResult AlarmHandle()
        {
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

        public JsonResult GetAlarmConfig(string equipmentTypeId)
        {
            var config = db.Queryable<AMS_CONFIGURATION>().Where(it => it.EQUIPMENT_TYPE_ID == equipmentTypeId).ToList();
            return Json(new { data = config, code = 0, count = config.Count }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAlarmRecords(int page, int limit, string equipmentId)
        {
            int totalNum = 0;
            var data = db.Queryable<AMS_ALARM_RECORD>().Where(it => it.EQID == equipmentId).OrderByDescending(it => it.ALTIME).ToPageList(page, limit, ref totalNum);
            return Json(new { data = data, code = 0, count = totalNum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAlarmActions(int page, int limit)
        {
            var totalNum = 0;
            var eqps = db.Queryable<RMS_EQUIPMENT>().Where(it => equipmenttypeids.Contains(it.TYPE)).ToList().Select(it => it.ID).ToList();
            var actions = db.Queryable<AMS_ACTION_RECORD>().Where(it => eqps.Contains(it.EQID))
                .OrderBy(it => it.ISHANDLED).OrderBy(it => it.DATETIME, OrderByType.Desc)
                .ToPageList(page, limit, ref totalNum);
            var notHandledNum = actions.Where(it => !it.ISHANDLED).Count();
            return Json(new { data = actions, code = 0, count = totalNum, notHandledNum = notHandledNum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult HandleAction(string actionId, string remarkText)
        {
            var action = db.Queryable<AMS_ACTION_RECORD>().InSingle(actionId);
            if (!action.ISHANDLED)
            {
                if (action.CONFIRM_USERS.Contains(User.TRUENAME))
                    return Json(new { result = false, message = User.TRUENAME + "重复确认" }, JsonRequestBehavior.AllowGet);


                action.CONFIRMTIMES = action.CONFIRMTIMES + 1;
                action.CONFIRM_USERS.Add(User.TRUENAME);
                if (action.CONFIRMTIMES >= 2)
                {
                    action.ISHANDLED = true;
                }
                action.HANDLETIME = DateTime.Now;
                action.REMARK = action.REMARK+ DateTime.Now.ToString() + " " + User.TRUENAME + " " + remarkText + "\r\n";
                action.USERNAME = string.Join(",", action.CONFIRM_USERS);
                db.Updateable(action).ExecuteCommand();

                var unhandledActions = db.Queryable<AMS_ACTION_RECORD>().Where(it => it.EQID == action.EQID && !it.ISHANDLED).ToList();
                if (unhandledActions.Count == 0)
                {
                    var eqp = db.Queryable<RMS_EQUIPMENT>().InSingle(action.EQID);
                    eqp.ISLOCKED = false;
                    eqp.LOCKED_MESSAGE = string.Empty;

                    db.Updateable(eqp).ExecuteCommand();

                    var trans = new RabbitMqTransaction
                    {
                        TransactionName = $"UnlockMachine",
                        EquipmentID = action.EQID
                    };
                    var rabbitmqroute = $"EAP.SecsClient.{action.EQID}";
                    RabbitMqService.Produce(rabbitmqroute, trans);
                }
                return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { result = false, message = "此记录已被处理！" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddConfigItem(AlarmConfigVm config, string equipmentTypeId, string configId)
        {
            if (string.IsNullOrEmpty(configId))
            {
                var configitem = new AMS_CONFIGURATION
                {
                    NAME = config.NAME,
                    EQUIPMENT_TYPE_ID = equipmentTypeId,
                    ALID = config.ALID,
                    TRIGGER_INTERVAL = config.TRIGGER_INTERVAL,
                    TRIGGER_COUNT = config.TRIGGER_COUNT,
                    ACTIONID = "ContinuousAlarmLock",
                    ISVALID = config.ISVALID
                };
                db.Insertable(configitem).ExecuteCommand();
            }
            else
            {
                var configitem = new AMS_CONFIGURATION
                {
                    ID = configId,
                    NAME = config.NAME,
                    EQUIPMENT_TYPE_ID = equipmentTypeId,
                    ALID = config.ALID,
                    TRIGGER_INTERVAL = config.TRIGGER_INTERVAL,
                    TRIGGER_COUNT = config.TRIGGER_COUNT,
                    ACTIONID = "ContinuousAlarmLock",
                    ISVALID = config.ISVALID
                };
                db.Updateable(configitem).ExecuteCommand();
            }

            return Json(new { result = true, message = "" });
        }

        public JsonResult GetAlarmConfigById(string configId)
        {
            var config = db.Queryable<AMS_CONFIGURATION>().InSingle(configId);
            return Json(config, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteConfig(string configId)
        {
            db.Deleteable<AMS_CONFIGURATION>().In(configId).ExecuteCommand();
            return Json(new { result = true, message = "" });
        }
    }
}
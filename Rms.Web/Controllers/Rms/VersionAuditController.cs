using Pipelines.Sockets.Unofficial.Arenas;
using Rms.Models.DataBase.Rms;
using Rms.Utils;
using Rms.Web.Extensions;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Rms
{
    public class VersionAuditController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        class RMS_RECIPE_VERSION_VM : RMS_RECIPE_VERSION
        {
            public string RECIPE_NAME { get; set; }
        }

        public JsonResult GetCurrentAuditVersions(int page, int limit)
        {
            var db = DbFactory.GetSqlSugarClient();
            var totalnum = 0;
            var sql = @"SELECT R.NAME RECIPE_NAME,RV.* FROM RMS_RECIPE_VERSION RV,RMS_RECIPE R
where R.ID = RV.RECIPE_ID
AND RV.CURRENT_FLOW_INDEX >=0
AND RV.CURRENT_FLOW_INDEX <100
ORDER BY RV.CREATE_TIME";
            var data = db.SqlQueryable<RMS_RECIPE_VERSION_VM>(sql).ToList();
            totalnum = data.Count();
            data = data.Where(it => it._FLOW_ROLES[it.CURRENT_FLOW_INDEX] == User.ROLEID).ToList();
            if (totalnum > 0)
            {
                data = data.Skip((page - 1) * limit).Take(limit).ToList();
            }

            return Json(new
            {
                data = data,
                code = 0,
                count = totalnum,

            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVersionInfo(string versionid)
        {
            var db = DbFactory.GetSqlSugarClient();
            var versioninfo = db.Queryable<RMS_RECIPE_VERSION>().In(versionid).First();
            var projectinfo = db.Queryable<RMS_RECIPE>().In(versioninfo.RECIPE_ID).First();
            return Json(new { projectinfo, versioninfo }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFileTable(int page, int limit, string versionid)
        {
            var db = DbFactory.GetSqlSugarClient();
            //var totalnum = 0;
            var version = db.Queryable<RMS_RECIPE_VERSION>().In(versionid).First();
            var sql = $"SELECT ID,NAME,VERSION_ID,CREATOR,CREATE_TIME FROM USI_FCT.VMS_FILE WHERE ID = '{version.RECIPE_DATA_ID}'";
            var data = version.RECIPE_DATA_ID == null ? null : db.SqlQueryable<RMS_RECIPE_DATA>(sql).ToList();
            return Json(new { data = data, code = 0, count = 1 }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetProcessRecord(int page, int limit, string versionid)
        {
            var db = DbFactory.GetSqlSugarClient();
            

            var data = db.Queryable<RMS_FLOW_HIST>().Where(it => it.RECIPE_VERSION_ID == versionid).OrderBy(it => it.CREATE_TIME).ToList();

            return Json(new { data = data, code = 0, count = data.Count }, JsonRequestBehavior.AllowGet);

        }
        [LogAttribute]
        public JsonResult ApproveVersion(string versionid, string remark)
        {
            //同意
            var db = DbFactory.GetSqlSugarClient();
            db.BeginTran();//开启事务
            try
            {
                var version = db.Queryable<RMS_RECIPE_VERSION>().In(versionid).First();
                if (version.CURRENT_FLOW_INDEX < 0 || version.CURRENT_FLOW_INDEX >= 100) return Json(new ResponseResult { result = false, message = "当前状态禁止签核" });
                //插入签核记录
                var flowhist = new RMS_FLOW_HIST
                {
                    RECIPE_VERSION_ID = versionid,
                    FLOW_INDEX = version.CURRENT_FLOW_INDEX,
                    ACTION = FlowAction.Approve,
                    CREATOR = User.TRUENAME,
                    CREATE_TIME = DateTime.Now,
                    REMARK = remark
                };
                db.Insertable<RMS_FLOW_HIST>(flowhist).ExecuteCommand();
                //更新版本当前流程
                version.CURRENT_FLOW_INDEX = version.CURRENT_FLOW_INDEX + 1;

                if (version.CURRENT_FLOW_INDEX == version._FLOW_ROLES.Count)//签核最后一个环节
                {
                    version.CURRENT_FLOW_INDEX = 100;
                    //更新项目表中生效版本ID
                    var project = db.Queryable<RMS_RECIPE>().In(version.RECIPE_ID).First();
                    project.VERSION_EFFECTIVE_ID = version.ID;
                    db.Updateable<RMS_RECIPE>(project).UpdateColumns(it => it.VERSION_EFFECTIVE_ID).ExecuteCommand();
                }
                db.Updateable<RMS_RECIPE_VERSION>(version).UpdateColumns(it => it.CURRENT_FLOW_INDEX).ExecuteCommand();

                db.CommitTran();
                return Json(new ResponseResult { result = true, message = "签核成功" ,data = "签核版本："+versionid });
            }
            catch (Exception ex)
            {
                db.RollbackTran();
                return Json(new ResponseResult { result = false, message = ex.Message, data = "签核版本：" + versionid });
            }
        }
        [LogAttribute]
        public JsonResult RejectVersion(string versionid, string remark)
        {
            // 否决
            var db = DbFactory.GetSqlSugarClient();
            db.BeginTran();//开启事务
            try
            {
                var version = db.Queryable<RMS_RECIPE_VERSION>().In(versionid).First();
                if (version.CURRENT_FLOW_INDEX < 0 || version.CURRENT_FLOW_INDEX >= 100) return Json(new ResponseResult { result = false, message = "当前状态禁止签核" });
                //插入签核记录
                var flowhist = new RMS_FLOW_HIST
                {
                    RECIPE_VERSION_ID = versionid,
                    FLOW_INDEX = version.CURRENT_FLOW_INDEX,
                    ACTION = FlowAction.Reject,
                    CREATOR = User.TRUENAME,
                    CREATE_TIME = DateTime.Now,
                    REMARK = remark
                };
                db.Insertable<RMS_FLOW_HIST>(flowhist).ExecuteCommand();
                //更新版本当前流程
                version.CURRENT_FLOW_INDEX = -1;
                db.Updateable<RMS_RECIPE_VERSION>(version).UpdateColumns(it => new { it.CURRENT_FLOW_INDEX }).ExecuteCommand();
                db.CommitTran();
                return Json(new ResponseResult { result = true, message = "完成，回退到提交人" ,data = "签核版本：" + versionid });
            }
            catch (Exception ex)
            {
                db.RollbackTran();
                return Json(new ResponseResult { result = false, message = ex.Message, data = "签核版本：" + versionid });
            }
        }
    }
}
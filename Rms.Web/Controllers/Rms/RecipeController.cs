using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using Rms.Web.Extensions;
using Rms.Web.Utils;
using RMS.Domain.Rms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Rms
{
    public class RecipeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AddVersion()
        {
            return View();
        }
        public ActionResult GetRecipe(int page, int limit, string EQID)
        {
            var db = DbFactory.GetSqlSugarClient();
            var totalnum = 0;
            List<RecipeVersion> RecipeVersionList = db.Queryable<RMS_RECIPE>()
                .Where(it => it.EQUIPMENT_ID == EQID)
                .LeftJoin<RMS_RECIPE_VERSION>((r, rv) => r.VERSION_LATEST_ID == rv.ID)
                .LeftJoin<RMS_RECIPE_VERSION>((r, rv, rv2) => r.VERSION_EFFECTIVE_ID == rv2.ID)
                //.LeftJoin<RMS_RECIPE_GROUP>((r, rv, rv2, rrg) => r.RECIPE_GROUP_ID == rrg.ID)
                .Select((r, rv, rv2) => new RecipeVersion 
                { 
                    RECIPE_ID = r.ID,
                    RECIPE_NAME = r.NAME,
                    //RECIPE_GROUP_NAME = rrg.NAME,
                    RECIPE_LATEST_VERSION = (decimal)rv.VERSION, 
                    RECIPE_EFFECTIVE_VERSION = (decimal)rv2.VERSION 
                })
                .ToPageList(page, limit, ref totalnum);
            //var data = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == EQID);//.ToPageList(page, limit, ref totalnum);
            //var rcpList = db.Queryable<RMS_RECIPE_VERSION>().LeftJoin<RMS_RECIPE>(data,(v,r) => r.VERSION_LATEST_ID == v.ID).Select(v => v).ToList();
            //var project = db.Queryable<VMS_PROJECT>().In(projectid).First();
            return Json(new
            {
                data = RecipeVersionList,
                code = 0,
                count = totalnum,
                //newversionlock = project.VERSION_LATEST_ID != project.VERSION_EFFECTIVE_ID,
                //effective_version_id = project.VERSION_EFFECTIVE_ID
            }, JsonRequestBehavior.AllowGet);

            //return Json(new {RecipeId = 0});
        }

        public ActionResult GetRecipeFromEQP(string EQID)
        {
            try
            {
                string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/geteppd";
                var body = JsonConvert.SerializeObject(new GetEppdRequest { EuipmentId = EQID });

                var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
                var rep = JsonConvert.DeserializeObject<GetEppdResponse>(apiresult);
                if (rep.Result)
                {

                    List<RMS_RECIPE> recipelist = new List<RMS_RECIPE>();
                    foreach (var item in rep.EPPD)
                    {
                        RMS_RECIPE rcpItem = new RMS_RECIPE();
                        rcpItem.NAME = item;
                        rcpItem.EQUIPMENT_ID = EQID;
                        recipelist.Add(rcpItem);
                    };
                    //recipelist = recipelist.ToPageList(page, limit, ref totalnum);
                    return Json(new
                    {
                        data = recipelist,
                        code = 0,
                        count = recipelist.Count

                    }, JsonRequestBehavior.AllowGet);
                    //return Json(new { recipelist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        data = rep,
                        code = 0,
                        count = 0
                    }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception)
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);

            }

        }
        public ActionResult GetVersions(int page, int limit, string rcpID)
        {
            var db = DbFactory.GetSqlSugarClient();
            var totalnum = 0;

            var versionList = db.Queryable<RMS_RECIPE_VERSION>()
                .Where(it => it.RECIPE_ID == rcpID)
                .OrderBy(it => it.VERSION).ToPageList(page, limit, ref totalnum);
            return Json(new
            {
                data = versionList,
                code = 0,
                count = totalnum,
                //newversionlock = project.VERSION_LATEST_ID != project.VERSION_EFFECTIVE_ID,
                //effective_version_id = project.VERSION_EFFECTIVE_ID
            }, JsonRequestBehavior.AllowGet);

        }


        [LogAttribute]
        public JsonResult AddNewVersion(string projectid)
        {
            var db = DbFactory.GetSqlSugarClient();
            db.BeginTran();
            try
            {
                var rcp = db.Queryable<RMS_RECIPE>().In(projectid).First();
                if (rcp.VERSION_LATEST_ID != rcp.VERSION_EFFECTIVE_ID) return Json(new ResponseResult { result = false, message = "有未完成的签核版本，禁止新增！" });
                var eqp = db.Queryable<RMS_EQUIPMENT>().In(rcp.EQUIPMENT_ID).First();
                if (eqp.RECIPE_TYPE == "onlyName") { return Json( new ResponseResult { result = false, message = "Recipe Type 'onlyName' can not add new version!" }); }
                var lastversion = rcp.VERSION_LATEST_ID == null ? null : db.Queryable<RMS_RECIPE_VERSION>().In(rcp.VERSION_LATEST_ID).First();
                var flow = db.Queryable<RMS_FLOW>().In(rcp.FLOW_ID).First();
                //插入新版本
                var version = new RMS_RECIPE_VERSION
                {
                    RECIPE_ID = projectid,
                    FLOW_ID = rcp.FLOW_ID,
                    VERSION = (lastversion?.VERSION + 1) ?? 1,
                    FLOW_ROLES = flow.FLOW_ROLES,
                    CURRENT_FLOW_INDEX = -1,
                    CREATOR = User.TRUENAME,
                    //BASE_VERSION = lastversion?.VERSION,
                };
                db.Insertable<RMS_RECIPE_VERSION>(version).ExecuteCommand();

                db.CommitTran();
                //更新PROJECT表中记录的最新版ID
                rcp.VERSION_LATEST_ID = version.ID;
                db.Updateable<RMS_RECIPE>(rcp).UpdateColumns(it => new { it.VERSION_LATEST_ID }).ExecuteCommand();

                return Json(new ResponseResult { result = true, message = "添加成功", data = new { versionid = version.ID } });
            }
            catch (Exception ex)
            {
                db.RollbackTran();
                return Json(new ResponseResult { result = false, message = ex.Message });
            }
        }

        public JsonResult GetVersionInfo(string versionid)
        {
            var db = DbFactory.GetSqlSugarClient();
            var versioninfo = db.Queryable<RMS_RECIPE_VERSION>().In(versionid).First();
            var projectinfo = db.Queryable<RMS_RECIPE>().In(versioninfo.RECIPE_ID).First();
            return Json(new { projectinfo, versioninfo }, JsonRequestBehavior.AllowGet);
        }

        public class VersionForm
        {
            public string ID { get; set; }
            public string NAME { get; set; }
            public string REMARK { get; set; }
        }

        public JsonResult SaveForm(VersionForm data)
        {
            try
            {
                var db = DbFactory.GetSqlSugarClient();
                var version = db.Queryable<RMS_RECIPE_VERSION>().In(data.ID).First();

                //签核状态下无法编辑表单
                if (version.CURRENT_FLOW_INDEX > -1) return Json(new ResponseResult { result = false, message = "当前状态无法编辑与保存！" });

                //version.NAME = data.NAME;
                version.REMARK = data.REMARK;

                db.Updateable<RMS_RECIPE_VERSION>(version).ExecuteCommand();

                return Json(new ResponseResult { result = true, message = "保存成功" });
            }
            catch (Exception ex)
            {
                return Json(new ResponseResult { result = false, message = ex.Message });
            }
        }


        public JsonResult SubmitForm(VersionForm data)
        {
            var db = DbFactory.GetSqlSugarClient();
            db.BeginTran();
            try
            {
                if (string.IsNullOrEmpty(data.NAME)) return Json(new ResponseResult { result = false, message = "请填写版本号！" });
                if (string.IsNullOrEmpty(data.REMARK)) return Json(new ResponseResult { result = false, message = "请填写更新说明备注！" });

                var version = db.Queryable<RMS_RECIPE_VERSION>().In(data.ID).First();

                if (version.CURRENT_FLOW_INDEX > -1) return Json(new ResponseResult { result = false, message = "当前状态无法编辑与保存！" });


                if (version.RECIPE_DATA_ID == null) return Json(new { code = 10, result = false, message = "未上传文件，无法提交" });

                //插入提交记录
                var flowhist = new RMS_FLOW_HIST
                {
                    RECIPE_VERSION_ID = data.ID,
                    FLOW_ID = version.FLOW_ID,
                    FLOW_INDEX = version.CURRENT_FLOW_INDEX,
                    ACTION = FlowAction.Submit,
                    CREATOR = User.TRUENAME,
                    CREATE_TIME = DateTime.Now
                };
                db.Insertable<RMS_FLOW_HIST>(flowhist).ExecuteCommand();

                //更新
                //version.NAME = data.NAME;
                version.REMARK = data.REMARK;
                version.CURRENT_FLOW_INDEX = 0;

                db.Updateable<RMS_RECIPE_VERSION>(version).ExecuteCommand();
                db.CommitTran();
                return Json(new ResponseResult { result = true, message = "提交成功" });
            }
            catch (Exception ex)
            {
                db.RollbackTran();
                return Json(new ResponseResult { result = false, message = ex.Message });
            }
        }
        public JsonResult GetProcessRecord(int page, int limit, string versionid)
        {
            var db = DbFactory.GetSqlSugarClient();

            var data = db.Queryable<RMS_FLOW_HIST>().Where(it => it.RECIPE_VERSION_ID == versionid).OrderBy(it => it.CREATE_TIME).ToList();

            return Json(new { data, code = 0, count = data.Count }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetFileTable(int page, int limit, string versionid)
        {
            var db = DbFactory.GetSqlSugarClient();

            var version = db.Queryable<RMS_RECIPE_VERSION>().In(versionid).First();
            var sql = $"SELECT ID,NAME,VERSION_ID,CREATOR,CREATE_TIME FROM USI_DPSRMS.RMS_RECIPE_DATA WHERE ID = '{version.RECIPE_DATA_ID}'";
            var data = version.RECIPE_DATA_ID == null ? null : db.SqlQueryable<RMS_RECIPE_DATA>(sql).ToList();
            return Json(new { data, code = 0, count = 1 }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult UploadRcpFromEQP(string versionid, string rcpname)
        {
            //var vid = versionid;
            string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/reloadrecipebody";
            //string url = $"http://192.168.53.210:8085" + "/api/reloadrecipebody";
            var body = JsonConvert.SerializeObject(new ReloadRecipeBodyRequest { TrueName = User.TRUENAME, VersionId = versionid, RecipeName = rcpname });

            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
            var res = JsonConvert.DeserializeObject<ReloadRecipeBodyResponse>(apiresult);
            //GetFileTable(1, 1, versionid);
            return Json(new { res });
        }

        [LogAttribute]
        public JsonResult AddNewRecipeToApi(string EQID, string rcpname)
        {

            string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/addnewrecipe";
            var body = JsonConvert.SerializeObject(new AddNewRecipeRequest { TrueName = User.TRUENAME, EquipmentId = EQID, RecipeName = rcpname });

            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
            var replyItem = JsonConvert.DeserializeObject<AddNewRecipeResponse>(apiresult);
            return Json(new { replyItem });
        }

        public JsonResult DeleteFile(string fileid)
        {
            var db = DbFactory.GetSqlSugarClient();
            try
            {
                var version = db.Queryable<RMS_RECIPE_VERSION>().Where(it => it.RECIPE_DATA_ID == fileid).First();
                if (version.CURRENT_FLOW_INDEX > -1) return Json(new ResponseResult { result = false, message = "当前状态无法编辑与保存！" });
                var data = db.Deleteable<RMS_RECIPE_DATA>().Where(it => it.ID == fileid).ExecuteCommand();

                return Json(new ResponseResult { result = true, message = "删除成功" });
            }
            catch (Exception ex)
            {

                return Json(new ResponseResult { result = false, message = ex.Message });
            }

        }

        public ActionResult AddRecipe()
        {
            return View();
        }

        public JsonResult AddNewRecipe(string EQID)
        {
            return Json(new { });
        }
        [LogAttribute]
        public JsonResult DownloadRecipeToApi(string rcpID)
        {
            var db = DbFactory.GetSqlSugarClient();
            var recipe = db.Queryable<RMS_RECIPE>().In(rcpID).First();

            string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/downloadeffectiverecipetomachine";
            var body = JsonConvert.SerializeObject(new DownloadEffectiveRecipeToMachineRequest { TrueName = User.TRUENAME, EquipmentId = recipe.EQUIPMENT_ID, RecipeName = recipe.NAME });

            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
            var replyItem = JsonConvert.DeserializeObject<DownloadEffectiveRecipeToMachineResponse>(apiresult);
            return Json(new { replyItem });
        }

        [LogAttribute]
        public JsonResult DownloadRecipeByLot(string eqid, string lotid)
        {
            string sfis_step7_req = $"{eqid},{lotid},7,M068397,JORDAN,,OK,MODEL_NAME=???";
            string sfis_step7_res = string.Empty;
            string errmsg = string.Empty;
            if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg))
            {
                if (sfis_step7_res.ToUpper().StartsWith("OK"))
                {

                    Dictionary<string, string> sfispara = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                    .Where(keyValueArray => keyValueArray.Length == 2)
               .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                    string rcpgroupname = sfispara["MODEL_NAME"];

                    var db = DbFactory.GetSqlSugarClient();
                    //var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == eqid && it.NAME.TrimEnd() == rcpname).First();

                    string apiURL = ConfigurationManager.AppSettings["EAP.API"].ToString() + "/api/downloadeffectiverecipebyrecipegroup";
                    var body = JsonConvert.SerializeObject(new DownloadEffectiveRecipeByRecipeGroupRequest { TrueName = User.TRUENAME, EquipmentId = eqid, RecipeGroupName = rcpgroupname });

                    var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(apiURL, body);
                    var replyItem = JsonConvert.DeserializeObject<DownloadEffectiveRecipeByRecipeGroupResponse>(apiresult);

                    return Json(new { Result = replyItem.Result, Message = replyItem.Message,RecipeGroupName = rcpgroupname, RecipeName = replyItem.RecipeName });
                }
                else
                {
                    return Json(new { Result = false, Message = $"Sfis error:{sfis_step7_res}" });
                }
            }
            else
            {
                return Json(new { Result = false, Message = $"{errmsg}" });
            }
        }

        
        private bool SendMessageToSfis(string message, ref string receiveMsg, ref string errMsg)
        {
            try
            {
                TcpClient tcpSync;
                string sfisip = ConfigurationManager.AppSettings["SfisIp"].ToString();
                int sfisport = int.Parse(ConfigurationManager.AppSettings["SfisPort"]);
                IPAddress serverip = IPAddress.Parse(sfisip);
                IPEndPoint point = new IPEndPoint(serverip, sfisport);
                tcpSync = new TcpClient();
                tcpSync.Connect(point);
                if (tcpSync != null && tcpSync.Connected)
                {
                    NetworkStream nStream = tcpSync.GetStream();
                    byte[] data = new byte[1024];
                    data = Encoding.UTF8.GetBytes(message);
                    nStream.Write(data, 0, data.Length);
                    nStream.Flush();

                    int len = 0;
                    int trytimes = 3;
                    while (len == 0 && trytimes > 0)
                    {
                        data = new byte[1024];
                        nStream = tcpSync.GetStream();
                        len = nStream.Read(data, 0, data.Length);
                        trytimes--;
                    }
                    if (len > 0)
                    {
                        receiveMsg = Encoding.UTF8.GetString(data, 0, len);
                        nStream.Flush();
                        tcpSync.Close();
                        return true;
                    }
                    else
                    {
                        errMsg = "SFIS Timeout";
                        return false;
                    }
                }
                else
                {
                    errMsg = "Can not connect to SFIS";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errMsg = "EAP send message to SFIS fail.";
                return false;
            }
        }


    }
}
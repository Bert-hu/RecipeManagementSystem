using Rms.Models.DataBase.Rms;
using Rms.Utils;
using Rms.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Media.Media3D;

namespace Rms.Web.Controllers.Rms
{
    public class MarkingManageController : BaseController
    {
        // GET: MarkingManage
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MarkingFields()
        {
            return View();
        }


        public JsonResult GetMarkingFields(int page, int limit)
        {
            var db = DbFactory.GetSqlSugarClient();
            var totalnum = 0;
            var data = db.Queryable<RMS_MARKING_FIELD>().ToPageList(page, limit, ref totalnum);
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AddMarkingVersion(string recipeid)
        {
            var db = DbFactory.GetSqlSugarClient();
            var recipe = db.Queryable<RMS_RECIPE>().In(recipeid).First();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
            var eqtype = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            if (recipe.MARKING_LATEST_ID != null && recipe.MARKING_EFFECTIVE_ID != recipe.MARKING_LATEST_ID)
            {
                return Json(new ResponseResult { result = false, message = "请先完成未提交版本" });
            }
            RMS_MARKING_VERSION lastmarkingversion = null;
            if (recipe.MARKING_LATEST_ID != null)
            {
                lastmarkingversion = db.Queryable<RMS_MARKING_VERSION>().In(recipe.MARKING_LATEST_ID).First();
            }
            var markingversion = new RMS_MARKING_VERSION
            {
                RECIPE_ID = recipeid,

                VERSION = (lastmarkingversion?.VERSION ?? 0) + 1,
                FLOW_ROLES = eqtype.FLOWROLEIDS,
                CURRENT_FLOW_INDEX = 0,
                CREATOR = User.TRUENAME
            };
            db.Insertable<RMS_MARKING_VERSION>(markingversion).ExecuteCommand();
            recipe.MARKING_LATEST_ID = markingversion.ID;
            db.Updateable<RMS_RECIPE>(recipe).UpdateColumns(it => it.MARKING_LATEST_ID).ExecuteCommand();
            return Json(new { result = true, message = "添加成功", markingversionid = markingversion.ID });
        }
        public class MarkingConfig
        {
            public string recipeid { get; set; }
            public string version { get; set; }
            public int textselect { get; set; }
            public string marktype { get; set; }
            public string fixedtext { get; set; }
            public string field { get; set; }
            public int startindex { get; set; }
            public int length { get; set; }
        }


        public JsonResult AddMarkingConfig(MarkingConfig data, bool isMainConfig)
        {
            if (data.version == null) return Json(new ResponseResult { result = false, message = "请选择版本" });
            var db = DbFactory.GetSqlSugarClient();
            var version = db.Queryable<RMS_MARKING_VERSION>().In(data.version).First();
            if (version.CURRENT_FLOW_INDEX > 0) return Json(new { result = false, message = "已提交版本禁止修改" });

            if (!isMainConfig && version.CREATOR == User.TRUENAME) return Json(new { result = false, message = "版本创建者禁止修改Backup版本！" });
            if (isMainConfig && version.CREATOR != User.TRUENAME) return Json(new { result = false, message = "版本创建者才能修改Main版本！" });

            if (isMainConfig)
            {
                var textconfigs = db.Queryable<RMS_MARKING_CONFIG>().Where(it => it.MARKING_VERSION_ID == data.version && it.TEXTINDEX == data.textselect).OrderBy(it => it.TEXTORDER).ToList();
                var isfixed = data.marktype == "Fixed";
                var config = new RMS_MARKING_CONFIG
                {
                    MARKING_VERSION_ID = data.version,
                    TEXTINDEX = data.textselect,
                    TEXTORDER = textconfigs.Count + 1,
                    TYPE = data.marktype,
                    CONTENT = isfixed ? data.fixedtext : data.field,
                    START_INDEX = isfixed ? -1 : data.startindex,
                    LENGTH = isfixed ? -1 : data.length,
                    CREATOR = User.TRUENAME
                };
                db.Insertable<RMS_MARKING_CONFIG>(config).ExecuteCommand();
            }
            else//备用模板
            {
                var textconfigs = db.Queryable<RMS_MARKING_CONFIG_BACKUP>().Where(it => it.MARKING_VERSION_ID == data.version && it.TEXTINDEX == data.textselect).OrderBy(it => it.TEXTORDER).ToList();
                var isfixed = data.marktype == "Fixed";
                var config = new RMS_MARKING_CONFIG_BACKUP
                {
                    MARKING_VERSION_ID = data.version,
                    TEXTINDEX = data.textselect,
                    TEXTORDER = textconfigs.Count + 1,
                    TYPE = data.marktype,
                    CONTENT = isfixed ? data.fixedtext : data.field,
                    START_INDEX = isfixed ? -1 : data.startindex,
                    LENGTH = isfixed ? -1 : data.length,
                    CREATOR = User.TRUENAME
                };
                db.Insertable<RMS_MARKING_CONFIG_BACKUP>(config).ExecuteCommand();
            }

            return Json(new { result = true, message = "添加成功" });
        }
        public JsonResult DeleteMarkingConfig(string configid, bool isMainConfig)
        {
            var db = DbFactory.GetSqlSugarClient();

            if (isMainConfig)
            {
                var config = db.Queryable<RMS_MARKING_CONFIG>().In(configid).First();
                var version = db.Queryable<RMS_MARKING_VERSION>().In(config.MARKING_VERSION_ID).First();
                if (version.CURRENT_FLOW_INDEX > 0) return Json(new { result = false, message = "已提交版本禁止修改" });

                var textconfigs = db.Queryable<RMS_MARKING_CONFIG>().Where(it => it.TEXTINDEX == config.TEXTINDEX && it.TEXTORDER > config.TEXTORDER).OrderBy(it => it.TEXTORDER).ToList();
                textconfigs.ForEach(it => it.TEXTORDER = it.TEXTORDER - 1);

                db.Deleteable<RMS_MARKING_CONFIG>(config).ExecuteCommand();
                db.Updateable<RMS_MARKING_CONFIG>(textconfigs).UpdateColumns(it => it.TEXTORDER).ExecuteCommand();
            }
            else
            {
                var config = db.Queryable<RMS_MARKING_CONFIG_BACKUP>().In(configid).First();
                var version = db.Queryable<RMS_MARKING_VERSION>().In(config.MARKING_VERSION_ID).First();
                if (version.CURRENT_FLOW_INDEX > 0) return Json(new { result = false, message = "已提交版本禁止修改" });

                var textconfigs = db.Queryable<RMS_MARKING_CONFIG_BACKUP>().Where(it => it.TEXTINDEX == config.TEXTINDEX && it.TEXTORDER > config.TEXTORDER).OrderBy(it => it.TEXTORDER).ToList();
                textconfigs.ForEach(it => it.TEXTORDER = it.TEXTORDER - 1);

                db.Deleteable<RMS_MARKING_CONFIG_BACKUP>(config).ExecuteCommand();
                db.Updateable<RMS_MARKING_CONFIG_BACKUP>(textconfigs).UpdateColumns(it => it.TEXTORDER).ExecuteCommand();

            }

            return Json(new { result = true, message = "删除成功" });
        }
        public JsonResult GetMarkingRecipe(int page, int limit, string filter)
        {
            var db = DbFactory.GetSqlSugarClient();
            var totalnum = 0;
            var sql = @"select eq.ID EQID,eq.NAME EQNAME,eq.FLOW_ID FLOW_ID,r.ID RECIPEID,r.NAME RECIPENAME,
r.MARKING_LATEST_ID MARKING_LATEST_ID,r.MARKING_EFFECTIVE_ID MARKING_EFFECTIVE_ID,
mv.VERSION LATEST_VERSION,
mv.CREATOR LATEST_CREATOR,
mv2.VERSION EFFECTIVE_VERSION,
mv2.CREATOR EFFECTIVE_CREATOR
 from RMS_EQUIPMENT eq
inner join RMS_RECIPE r  on eq.ID = r.EQUIPMENT_ID 
left join RMS_MARKING_VERSION mv  on mv.ID = r.MARKING_LATEST_ID
left join RMS_MARKING_VERSION mv2  on mv2.ID = r.MARKING_EFFECTIVE_ID
where eq.TYPE = 'WaferMarking'";
            var data = db.SqlQueryable<MarkingRecipe>(sql).ToPageList(page, limit, ref totalnum);
            if (!string.IsNullOrEmpty(filter))
            {
                data = data.Where(it => string.Join("", new string[] { it.RECIPENAME, it.EQID }).ToUpper().Contains(filter.ToUpper())).Skip((page - 1) * limit).Take(limit).ToList();
                totalnum = data.Count();
            }
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMarkingConfigs(int page, int limit, string markingversionid, bool isMainConfig)
        {
            var db = DbFactory.GetSqlSugarClient();
            var version = db.Queryable<RMS_MARKING_VERSION>().In(markingversionid).First();
            if (version.CURRENT_FLOW_INDEX != 100 && (version.CREATOR == User.TRUENAME && !isMainConfig))
            {
                return Json(new { data = new List<RMS_MARKING_CONFIG>(), code = 0, count = 0, errmsg = "版本创建者无法查看Backup版本" }, JsonRequestBehavior.AllowGet);
            }
            if (version.CURRENT_FLOW_INDEX != 100 && (version.CREATOR != User.TRUENAME && isMainConfig))
            {
                return Json(new { data = new List<RMS_MARKING_CONFIG>(), code = 0, count = 0, errmsg = "非版本创建者无法查看Main版本" }, JsonRequestBehavior.AllowGet);
            }

            var totalnum = 0;
            if (isMainConfig)
            {
                var data = db.Queryable<RMS_MARKING_CONFIG>().Where(it => it.MARKING_VERSION_ID == markingversionid).OrderBy(it => new { it.TEXTINDEX, it.TEXTORDER }).ToList();
                return Json(new { data = data, code = 0, count = totalnum, errmsg = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = db.Queryable<RMS_MARKING_CONFIG_BACKUP>().Where(it => it.MARKING_VERSION_ID == markingversionid).OrderBy(it => new { it.TEXTINDEX, it.TEXTORDER }).ToList();
                return Json(new { data = data, code = 0, count = totalnum, errmsg = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetMarkingTexts(string markingversionid, bool isMainConfig)
        {
            try
            {
                Dictionary<string, string> markingtexts = GetMarkingTextsDict(markingversionid, isMainConfig);
                //var db = DbFactory.GetSqlSugarClient();
                //var codevalues = db.Queryable<RMS_MARKING_FIELD>().ToList().ToDictionary(it => it.NAME, va => va.SAMPLE);

                //var data = db.Queryable<RMS_MARKING_CONFIG>().Where(it => it.MARKING_VERSION_ID == markingversionid).OrderBy(it => new { it.TEXTINDEX, it.TEXTORDER }).ToList();
                //var markingindexs = data.Select(it => it.TEXTINDEX).Distinct().OrderBy(it => it).ToList();
                //Dictionary<string, string> markingtexts = new Dictionary<string, string>();
                //markingindexs.ForEach(it =>//遍历每一行内容
                //{
                //    var rowconfigs = data.Where(o => o.TEXTINDEX == it).OrderBy(o => o.TEXTORDER).ToList();
                //    string rowtext = string.Empty;
                //    rowconfigs.ForEach(o =>//遍历每一行的配置内容并拼接
                //    {
                //        if (o.TYPE == "Fixed")
                //        {
                //            rowtext += o.CONTENT;
                //        }
                //        else if (o.TYPE == "Code")
                //        {
                //            string codevalue;
                //            if (codevalues.TryGetValue(o.CONTENT, out codevalue))
                //            {
                //                if (o.START_INDEX + o.LENGTH > codevalue.Length)
                //                {
                //                    throw new Exception($"Config error: Field:{o.CONTENT}, Value '{codevalue}', StartIndex: {o.START_INDEX}, Length: {o.LENGTH}.");
                //                }
                //                else
                //                {
                //                    rowtext += codevalue.Substring(o.START_INDEX, o.LENGTH);
                //                }
                //            }
                //            else
                //            {
                //                throw new Exception($"Can not get the value of code '{o.CONTENT}'");
                //            }

                //        }

                //    });
                //    markingtexts.Add(it.ToString(), rowtext);
                //});

                return Json(new { result = true, markingtexts = markingtexts, message = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private Dictionary<string, string> GetMarkingTextsDict(string markingversionid, bool isMainConfig)
        {
            var db = DbFactory.GetSqlSugarClient();
            var codevalues = db.Queryable<RMS_MARKING_FIELD>().ToList().ToDictionary(it => it.NAME, va => va.SAMPLE);
            List<RMS_MARKING_CONFIG> data;
            if (isMainConfig)
            {
                data = db.Queryable<RMS_MARKING_CONFIG>().Where(it => it.MARKING_VERSION_ID == markingversionid).OrderBy(it => new { it.TEXTINDEX, it.TEXTORDER }).ToList();
            }
            else
            {
                var data1 = db.Queryable<RMS_MARKING_CONFIG_BACKUP>().Where(it => it.MARKING_VERSION_ID == markingversionid).OrderBy(it => new { it.TEXTINDEX, it.TEXTORDER }).ToList();
                data = data1.Select(it => (RMS_MARKING_CONFIG)it).ToList();
            }
            var markingindexs = data.Select(it => it.TEXTINDEX).Distinct().OrderBy(it => it).ToList();
            Dictionary<string, string> markingtexts = new Dictionary<string, string>();
            markingindexs.ForEach(it =>//遍历每一行内容
            {
                var rowconfigs = data.Where(o => o.TEXTINDEX == it).OrderBy(o => o.TEXTORDER).ToList();
                string rowtext = string.Empty;
                rowconfigs.ForEach(o =>//遍历每一行的配置内容并拼接
                {
                    if (o.TYPE == "Fixed")
                    {
                        rowtext += o.CONTENT;
                    }
                    else if (o.TYPE == "Code")
                    {
                        string codevalue;
                        if (codevalues.TryGetValue(o.CONTENT, out codevalue))
                        {
                            if (o.START_INDEX + o.LENGTH > codevalue.Length)
                            {
                                throw new Exception($"Config error: Field:{o.CONTENT}, Value '{codevalue}', StartIndex: {o.START_INDEX}, Length: {o.LENGTH}.");
                            }
                            else
                            {
                                rowtext += codevalue.Substring(o.START_INDEX, o.LENGTH);
                            }
                        }
                        else
                        {
                            throw new Exception($"Can not get the value of code '{o.CONTENT}'");
                        }

                    }

                });
                markingtexts.Add(it.ToString(), rowtext);
            });

            return markingtexts;

        }


        public JsonResult GetSubpageData(string recipeid)
        {
            var db = DbFactory.GetSqlSugarClient();

            var recipe = db.Queryable<RMS_RECIPE>().In(recipeid).First();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
            var markingversions = db.Queryable<RMS_MARKING_VERSION>().Where(it => it.RECIPE_ID == recipeid).OrderByDescending(it => it.VERSION).ToList();
            return Json(new { eqp = eqp, recipe = recipe, markingversions = markingversions });
        }


        public JsonResult SubmitVersion(string markingversionid)
        {
            //TODO: 比较Main和Backup版本
            Dictionary<string, string> mainMarkingTexts = GetMarkingTextsDict(markingversionid, true);
            Dictionary<string, string> backupMarkingtexts = GetMarkingTextsDict(markingversionid, false);
            if (!CompareDictionary(mainMarkingTexts, backupMarkingtexts))
            {
                return Json(new { result = false, message = "Main和Backup内容不一致，无法提交！" });
            }

            var db = DbFactory.GetSqlSugarClient();
            var version = db.Queryable<RMS_MARKING_VERSION>().In(markingversionid).First();
            //if(version.CURRENT_FLOW_INDEX > 0) return Json(new { result = false, message = "请勿重复提交" });
            version.CURRENT_FLOW_INDEX = 100;
            var recipe = db.Queryable<RMS_RECIPE>().In(version.RECIPE_ID).First();
            recipe.MARKING_EFFECTIVE_ID = version.ID;

            db.Updateable<RMS_MARKING_VERSION>(version).UpdateColumns(it => it.CURRENT_FLOW_INDEX).ExecuteCommand();
            db.Updateable<RMS_RECIPE>(recipe).UpdateColumns(it => it.MARKING_EFFECTIVE_ID).ExecuteCommand();
            return Json(new { result = true, message = "提交成功" });
        }

        private bool CompareDictionary(Dictionary<string, string> mainMarkingTexts, Dictionary<string, string> backupMarkingtexts)
        {
            if (mainMarkingTexts.Count != backupMarkingtexts.Count) return false;
            foreach (var item in mainMarkingTexts)
            {
                if (!backupMarkingtexts.ContainsKey(item.Key)) return false;
                if (backupMarkingtexts[item.Key] != item.Value) return false;
            }
            return true;
        }
    }


}
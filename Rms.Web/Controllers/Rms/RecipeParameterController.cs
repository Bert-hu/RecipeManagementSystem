using Renci.SshNet.Security;
using Rms.Models.DataBase.Rms;
using Rms.Utils;
using Rms.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Rms.Web.Controllers.Rms
{
    public class RecipeParameterController : BaseController
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
            try
            {
                //var db = DbFactory.GetSqlSugarClient();
                var scopedata = db.Queryable<RMS_PARAMETER_SCOPE>().Where(it => it.RecipeID == rcpFilter).OrderBy(it => new { it.SOURCE, it.ParamKey }).ToList();
                var recipe = db.Queryable<RMS_RECIPE>().In(rcpFilter).First();
                //var ParamName = String.IsNullOrEmpty(Request.Params["Name"]) ? "all" : Request.Params["Name"];
               // var ParamKey = String.IsNullOrEmpty(Request.Params["ParaKey"]) ? "all" : Request.Params["ParaKey"];

                if (recipe == null)
                {
                    return Json(new { data = new List<dynamic>(), code = 0, count = 0 }, JsonRequestBehavior.AllowGet);
                }
                var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
                var type = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
                var data_dictionary = db.Queryable<RMS_PARAMETER_DIC>().Where(it => it.EQ_TYPE_ID == type.ID).ToList();

                var result = (from a in scopedata
                             join b in data_dictionary on a.ParamKey equals b.ID
                             select new
                             {
                                 ID = a.ID,
                                 ParaKeyID = a.ParamKey,
                                 ParaKey = b.Key,
                                 Name = b.Name,
                                 RecipeID = a.RecipeID,
                                 Type = a.Type,
                                 LCL = a.LCL,
                                 UCL = a.UCL,
                                 EnumValue = a.EnumValue,
                                 LastEditor = a.LastEditor,
                                 LastEditTime = a.LastEditTime
                             }//).Where(it =>
                 ////(station.Equals("all") || it.EQID.Equals(station))
                 //(ParamName.Equals("all") || it.Name.Equals(ParamName))
                //&& (ParamKey.Equals("all") || it.ParaKey.Equals(ParamKey))
                ).ToList();

                var pageList = result.Skip((page - 1) * limit).Take(limit).ToList();

                return Json(new { data = pageList, code = 0, count = result.Count() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var pageList = new List<dynamic>();
                return Json(new { data = pageList, code = 0, count = pageList.Count() }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetParamsList(string paramFilter)
        {


            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.ID == paramFilter)?.First();
            var scopedata = db.Queryable<RMS_PARAMETER_SCOPE>().Where(it => it.RecipeID == recipe.ID).OrderBy(it => new { it.SOURCE, it.ParamKey }).ToList();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
            var type = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            var dic_data = db.Queryable<RMS_PARAMETER_DIC>().Where(it => it.EQ_TYPE_ID == type.ID).ToList();
            //筛选出没有设置过SPEC的参数，保证用户从前端界面添加的SPEC参数不会重复
            var data = dic_data.Where(it => !scopedata.Any(d => d.ParamKey == it.ID)).ToList();
            string option = string.Empty;
            foreach (var item in data)
            {
                option += "<option value=\"" + item.ID + "\" >" + item.Name + "</option>";
            }
            return Json(option);


        }

        public JsonResult AddNewScope()
        {
            var rcpid = Request.Params["data[rcpid]"];
            var paramkey = Request.Params["data[paramkey]"];
            var paramname = Request.Params["data[paramname]"];
            var type = Request.Params["data[type]"];
            var uplimit = Request.Params["data[uplimit]"];
            var lowlimit = Request.Params["data[lowlimit]"];
            var enumvalue = Request.Params["data[enum]"];
            if (type == "enum")
            {
                uplimit = "0";
                lowlimit = "0";
            }

            string param_dic_id = db.Queryable<RMS_PARAMETER_DIC>().Where(it => it.ID == paramkey).Select(it => it.ID).First();
            var exist = db.Queryable<RMS_PARAMETER_SCOPE>().First(it => it.RecipeID.Equals(rcpid) && it.ParamKey.Equals(param_dic_id));
            if (exist != null)
            {
                return Json(new ResponseResult { result = false, message = $"SEPC exists. Please update it directly." });
            }
            string scopeid = Guid.NewGuid().ToString("N");

            try
            {
                db.BeginTran(); //重载指定事务的级别
                db.Insertable<RMS_PARAMETER_SCOPE>(new RMS_PARAMETER_SCOPE()
                {
                    ID = scopeid,
                    ParamKey = param_dic_id,
                    LCL = double.Parse(lowlimit),
                    UCL = double.Parse(uplimit),
                    EnumValue = enumvalue,
                    RecipeID = rcpid,
                    Type = type,
                    LastEditor = User.TRUENAME,
                    LastEditTime = DateTime.Now
                }).ExecuteCommand();

                //提交事务
                db.CommitTran();
                return Json(new ResponseResult { result = true, message = $"Successfully added SPEC：{paramname}." });
            }
            catch (Exception ex)
            {
                db.RollbackTran();//回滚
                //message = ex.Message;
                return Json(new ResponseResult { result = false, message = $"Failed to add, please retry. Details: {ex.Message}" });
            }

        }

        public JsonResult EditScope()
        {
            try
            {
                var ID = Request["ID"];
                var field = Request["field"];
                var value = Request["value"];
                var obj = db.Queryable<RMS_PARAMETER_SCOPE>().Where(it => it.ID == ID).First();

                string property = "";
                switch (field)
                {
                    case "UCL":
                        property = "UCL";
                        obj.GetType().GetProperty(property).SetValue(obj, double.Parse(value), null);
                        break;
                    case "LCL":
                        property = "LCL";
                        obj.GetType().GetProperty(property).SetValue(obj, double.Parse(value), null);

                        break;
                    case "EnumValue":
                        property = "EnumValue";
                        obj.GetType().GetProperty(property).SetValue(obj, value, null);
                        break;
                }
                //:TODO:log
                //obj.GetType().GetProperty(property).SetValue(obj, double.Parse(value), null);
                obj.LastEditor = User.TRUENAME;
                obj.LastEditTime = System.DateTime.Now;


                db.BeginTran();
                db.Updateable(obj).ExecuteCommand();
                db.CommitTran();
            }
            catch (Exception ex)
            {
                return Json(new ResponseResult { result = false, message = $"Failed to update, please retry. Details: {ex.Message}" });
            }

            return Json(new ResponseResult { result = true, message = $"SPEC Updated!" });
        }


        public JsonResult DeleteScope()
        {
            try
            {
                var scopeID = Request.Params["data[ID]"];
                db.BeginTran();
                db.Deleteable<RMS_PARAMETER_SCOPE>().In(scopeID).ExecuteCommand();
                db.CommitTran();
                return Json(new ResponseResult { result = true, message = $"SPEC Deleted!" });
            }
            catch (Exception ex)
            {
                db.RollbackTran();
                return Json(new ResponseResult { result = false, message = $"Failed to delete, please retry. Details: {ex.Message}" });
            }



        }

        public JsonResult GetParamsDictionary(int page, int limit, string EQID)
        {

            var eqp = db.Queryable<RMS_EQUIPMENT>().In(EQID).First();
            var type = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            var data = db.Queryable<RMS_PARAMETER_DIC>().Where(it => it.EQ_TYPE_ID == type.ID).ToList();
            var pageList = data.Skip((page - 1) * limit).Take(limit).ToList();
            return Json(new { data = pageList, code = 0, count = data.Count }, JsonRequestBehavior.AllowGet);


        }

        public JsonResult AddNewParamsDictionary()
        {


            try
            {
                db.BeginTran();
                //var test = Request.Params["data"];
                var EQID = Request.Params["data[eqpid]"];
                var paramname = Request.Params["data[paramname]"];
                var paramkey = Request.Params["data[paramkey]"];
                var eqp = db.Queryable<RMS_EQUIPMENT>().In(EQID).First();
                var type = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
                RMS_PARAMETER_DIC item = new RMS_PARAMETER_DIC()
                {
                    EQ_TYPE_ID = type.ID,
                    Key = paramkey,
                    Name = paramname
                };
                var exist = db.Queryable<RMS_PARAMETER_DIC>().Where(it => it.Key == paramkey && it.EQ_TYPE_ID == type.ID)?.First();
                if (exist != null)
                {
                    return Json(new ResponseResult { result = false, message = "Failed to add, parameter existed，please go to set the SPEC." });
                }


                db.Insertable(item).ExecuteCommand();
                db.CommitTran();
                return Json(new ResponseResult { result = true, message = $"Parameter Added: {paramname}" });
            }
            catch (Exception ex)
            {
                db.RollbackTran();

                return Json(new ResponseResult { result = false, message = $"Failed to add, please retry. Details: {ex.Message}" });
            }







        }
        public JsonResult AddNewParamsDictionaryList()
        {
            try
            {

                var addedRows = new List<RMS_PARAMETER_DIC>();
                var source_eqid = string.Empty;
                int index = 0;
                //string eqpid = string.Empty;
                var source_typeid = string.Empty;

                List<RMS_PARAMETER_DIC> updateList, deleteList, insertList = new List<RMS_PARAMETER_DIC>();

                db.BeginTran();
                while (true)
                {
                    // 根据索引动态获取参数
                    var idParam = $"data[{index}][ID]";
                    var keyParam = $"data[{index}][key]";
                    var valueParam = $"data[{index}][value]";
                    var eqpidParam = $"data[{index}][eqpid]";

                    var id = Request.Params[idParam];
                    var key = Request.Params[keyParam];
                    var value = Request.Params[valueParam];
                    var eqpid = Request.Params[eqpidParam];


                    if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value) && string.IsNullOrEmpty(eqpid))
                    {
                        break; // 参数为空，说明没有更多数据
                    }
                    source_eqid = eqpid;
                    var eqp_tmp = db.Queryable<RMS_EQUIPMENT>().In(eqpid).First();
                    source_typeid = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp_tmp.TYPE).First().ID;

                    addedRows.Add(new RMS_PARAMETER_DIC
                    {
                        ID = id.StartsWith("newAdd") ? Guid.NewGuid().ToString("N") : id,
                        EQ_TYPE_ID = source_typeid,
                        Key = key,
                        Name = value,
                        SOURCE = eqpid

                    });

                    index++;
                }
                //var eqp = db.Queryable<RMS_EQUIPMENT>().In(source_eqid).First();
                //var typeid = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First().ID;
                var dbList = db.Queryable<RMS_PARAMETER_DIC>().Where(it => it.EQ_TYPE_ID == source_typeid).ToList();

                insertList = addedRows.Where(it => !dbList.Any(d => d.ID == it.ID || d.Key == it.Key)).ToList(); //数据库中没有的Key，新增
                deleteList = dbList.Where(it => !addedRows.Any(d => d.ID == it.ID)).ToList(); //前端提交没有的Key，删除

                updateList = addedRows.Join(dbList, f => f.ID, d => d.ID, (f, d) => new { d.ID, FrontKey = f.Key, DbKey = d.Key, FrontValue = f.Name, DbValue = d.Name, d.EQ_TYPE_ID, d.SOURCE })
                    .Where(x => x.FrontKey != x.DbKey || x.FrontValue != x.DbValue) // 如果 value 不相等，则更新
                    .Select(x => new RMS_PARAMETER_DIC
                    {
                        ID = x.ID,
                        Key = x.FrontKey,
                        Name = x.FrontValue,
                        EQ_TYPE_ID = x.EQ_TYPE_ID,
                        SOURCE = source_eqid
                    })
                    .ToList();
                db.Insertable(insertList).ExecuteCommand();
                db.Updateable(updateList).ExecuteCommand();
                db.Deleteable(deleteList).ExecuteCommand();
                db.CommitTran();
                return Json(new ResponseResult { result = true, message = $"成功添加 {insertList.Count} 项参数; 更新 {updateList.Count} 项参数; 删除 {deleteList.Count} 项参数。" });
            }
            catch
            {
                db.RollbackTran();

                return Json(new ResponseResult { result = false, message = $"提交失败，请重试！" });
            }





        }

        public JsonResult DeleteDictionaryItem()
        {

            var dictionaryID = Request.Params["data[ID]"];
            try
            {
                db.BeginTran();
                db.Deleteable<RMS_PARAMETER_DIC>().Where(it => it.ID == dictionaryID).ExecuteCommand();
                db.CommitTran();
                return Json(new { message = "删除成功！" });
            }
            catch
            {
                db.RollbackTran();
                return Json(new { message = "删除失败，请重试！" });
            }

        }
    }
}
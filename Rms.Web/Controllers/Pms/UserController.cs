
using Rms.Models.DataBase.Pms;
using Rms.Utils;
using Rms.Web.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Rms.Web.Controllers
{
    public class UserController : BaseController
    {

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        public JsonResult Get(int page, int limit, string filter)
        {
            var db = DbFactory.GetSqlSugarClient();
            List<PMS_USER> data;
            int totalnum = 0;
            if (string.IsNullOrEmpty(filter))
            {
                data = db.Queryable<PMS_USER>().ToPageList(page, limit, ref totalnum);
            }
            else
            {
                data = db.Queryable<PMS_USER>().Where(it => SqlFunc.MergeString(it.ID, it.USERNAME, it.TRUENAME).Contains(filter)).ToList();

            }

            // var data = _userService.GetPage( page, limit, ref totalCount,user => user.ROLENAME);
            return Json(new { data = data, code = 0, count = totalnum }, JsonRequestBehavior.AllowGet);
        }
        [LogAttribute]
        public JsonResult Edit(string id, string field, string value)
        {

            try
            {
                var db = DbFactory.GetSqlSugarClient();
                string sql = string.Format(@"update PMS_USER set {0}='{1}' where  ID='{3}' ", field, value, User.TRUENAME, id);
                var count = db.Ado.ExecuteCommand(sql);

                return Json(new ResponseResult { result = (count == 1), message = "修改成功", data = new { id = id, filed = field, value = value } });
            }
            catch (Exception ex)
            {
                return Json(new ResponseResult { result = false, message = ex.Message });
            }


        }

        public JsonResult AddUser(string id, string name, string role)
        {


            try
            {
                var db = DbFactory.GetSqlSugarClient();
                db.Insertable<PMS_USER>(new PMS_USER
                {
                    ID = id,
                    USERNAME = id,
                    TRUENAME = name,
                    PASSWORD = EncrypHelper.Encrypt32(id),
                    ROLEID = role,
                    LOCALUSER = true
                }).ExecuteCommand();

                return Json(new { result = true, message = "添加成功" });
            }
            catch (Exception ex)
            {
                return Json(new ResponseResult { result = false, message = ex.Message });
            }

        }
    }
}
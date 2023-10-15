
using Rms.Web.ViewModels;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Mvc;
using Rms.Models.DataBase.Pms;
using Rms.Utils;

namespace Rms.Web.Controllers
{
    /// <summary>
    /// 用户中心控制器
    /// </summary>
    public class AccountController : Controller
    {

        /// <summary>
        /// 登录页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Login()
        {

            var ipAddress = Request.UserHostAddress;
            var ssouser = SsoHelper.GetUserWithIp(ipAddress);

            if (ssouser != null)
            {
                var db = DbFactory.GetSqlSugarClient();
                var user = db.Queryable<PMS_USER>().First(it => it.ID == ssouser.userId && it.LOCALUSER == false);

                if (user != null)
                {
                    LoginTransaction(user);
                    return RedirectToAction("Index", "Home");
                }
            }
            return View("login");
        }

        public ActionResult NoPermission()
        {
            return View();
        }

        public ActionResult ChangePassword(AuthorizationContext filterContext)
        {
            //如果未登录，则跳转到登录页面
            if (Session["user_account"] == null)
            {
                return RedirectToAction("login", "account");
            }
            return View();
        }

        /// <summary>
        /// 提交登录请求
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            var db = DbFactory.GetSqlSugarClient();

            model.UserName = model.UserName.ToLower();
            Newtonsoft.Json.Linq.JObject jobj = null;

            var ipAddress = Request.UserHostAddress;
            var user = db.Queryable<PMS_USER>().First(it => it.USERNAME.ToLower() == model.UserName);
            var ssouser = SsoHelper.GetUserWithUP(ipAddress, model.UserName, model.Password);

            if (ssouser == null && user != null && user.LOCALUSER == true && EncrypHelper.Encrypt32(model.Password) == user.PASSWORD)//SSO验证未通过的本地账户
            {
                LoginTransaction(user);
                return RedirectToAction("index", "Home");
            }
            else if (ssouser == null && user != null && user.LOCALUSER == false && EncrypHelper.Encrypt32(model.Password) == user.PASSWORD) //非本地用户，且SSO验证失败，符合保留的密码
            {
                LoginTransaction(user);
                return RedirectToAction("index", "Home");
            }
            else if (ssouser != null)//SSO 验证通过
            {
                if (user == null)
                {
                    var roleid = ssouser.department;
                    //TODO: 自动判断创建角色
                    var role = db.Queryable<PMS_ROLE>().First(it => it.ID == roleid);
                    if (role == null)
                    {
                        role = new PMS_ROLE()
                        {
                            ID = roleid,
                            NAME = roleid,
                            DESCRIPTION = roleid,
                            INUSED = 1
                        };
                        db.Insertable<PMS_ROLE>(role).ExecuteCommand();
                    }
                    //TODO: 自动注册
                    user = new PMS_USER()
                    {
                        ID = ssouser.userId,
                        USERNAME = model.UserName,
                        TRUENAME = ssouser.name,
                        EMAIL = ssouser.mail,
                        INUSED = 1,
                        ROLEID = roleid,
                        LOCALUSER = false
                    };
                    db.Insertable<PMS_USER>(user).ExecuteCommand();
                }
                if (EncrypHelper.Encrypt32(model.Password) != user.PASSWORD)//密码变更，则更新
                {
                    user.PASSWORD = EncrypHelper.Encrypt32(model.Password);
                    db.Updateable<PMS_USER>(user).UpdateColumns(it => new { it.PASSWORD }).ExecuteCommand();
                }

                LoginTransaction(user);
                return RedirectToAction("index", "Home");
            }
            else//SSO和本地验证均失败
            {
                ModelState.AddModelError("error_message", "密码错误,请重新登录");
                return View(model);
            }

        }

        private void LoginTransaction(PMS_USER user)
        {
            var db = DbFactory.GetSqlSugarClient();
            Session["user_account"] = user;

            List<string> moduleids;

            if (user.ROLEID?.Equals("SuperAdmin") ?? false)//管理员默认拥有所有权限，不需要添加
            {
                moduleids = db.Queryable<PMS_MODULE>().Select(it => it.ID).ToList();
            }
            else
            {
                moduleids = db.Queryable<PMS_MODULEROLE>().Where(it => it.ROLE_ID == user.ROLEID).Select(it => it.MODULE_ID).ToList();
            }
            var modules = db.Queryable<PMS_MODULE>().Where(it => moduleids.Contains(it.ID)).OrderBy(it => it.ORDERSORT).ToList();
            var controllers = modules.Select(it => it.CONTROLLER).ToList();
            Session["modules"] = modules;
            Session["controllers"] = controllers;
            Session.Timeout = 30;
            var ipAddress = HttpContext.Request.ServerVariables["REMOTE_ADDR"] ?? HttpContext.Request.ServerVariables["REMOTE_ADDR"];
            db.Insertable(new RMS_PRODUCTIONLOG
            {
                IP = ipAddress,
                MODULENAME = "Account",
                ACTIONNAME = "login",
                //MESSAGE = responseContent,
                RESULT = "200",
                CREATOR = user?.TRUENAME,
                CREATETIME = DateTime.Now
            }).ExecuteCommand();
        }

        [HttpPost]
        public JsonResult Logout()
        {
            try
            {
                if (!(Session["user_account"] as PMS_USER).LOCALUSER)
                {
                    string ipAddress = Request.UserHostAddress;
                    SsoHelper.SignOut(ipAddress);
                }

                Session.Abandon();
                return Json(true);
            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }


        [HttpPost, ValidateAntiForgeryToken, AllowAnonymous]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            //如果视图模型中的属性没有验证通过，则返回到注册页面，要求用户重新填写
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var db = DbFactory.GetSqlSugarClient();

            var user = db.Queryable<PMS_USER>().Where(it => it.USERNAME == model.UserName.Trim()).First();
            if (!(user.LOCALUSER))
            {
                ModelState.AddModelError("error_message", "域账号不允许修改密码");
                return View(model);
            }
            //如果密码不匹配，则携带错误消息并返回登录页面
            if (user.PASSWORD != EncrypHelper.Encrypt32(model.OldPassword.Trim()))
            {
                ModelState.AddModelError("error_message", "原密码错误,请重新输入");
                return View(model);
            }

            if (model.Password == model.ConfirmPassword)
            {
                user.PASSWORD = EncrypHelper.Encrypt32(model.Password.Trim());
                db.Updateable<PMS_USER>(user).ExecuteCommand();
                Session.Abandon();
            }

            return RedirectToAction("login");
        }


    }
}
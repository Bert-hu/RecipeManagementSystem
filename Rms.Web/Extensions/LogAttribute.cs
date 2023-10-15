
using Rms.Models.DataBase.Pms;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Rms.Web.Extensions
{
    public class ResponseResult
    {
        public bool result { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
    public class LogAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            var user = context.HttpContext.Session["user_account"] as PMS_USER;

            // 获取请求内容
            //var requestContent = HttpUtility.UrlDecode(new StreamReader(context.HttpContext.Request.InputStream).ReadToEnd());
            //var aa );= Newtonsoft.Json.JsonConvert.SerializeObject(requestContent
            var ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            var controllerName = context.Controller.GetType().Name.Replace("Controller", "");
            var actionName = context.ActionDescriptor.ActionName;
            // 获取响应内容
            var responseContent = "";
            var result = context.Result;
            var contentResult = result as ContentResult;
            if (contentResult != null)
            {
                responseContent = contentResult.Content;
            }
            else
            {
                var jsonResult = result as JsonResult;
                if (jsonResult != null)
                {
                    responseContent = Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult.Data);
                }
                else
                {
                    responseContent = result.ToString();
                }
            }
            // 获取状态码
            var status = context.HttpContext.Response.StatusCode;
            var db = DbFactory.GetSqlSugarClient();
            db.Insertable(new RMS_PRODUCTIONLOG
            {
                IP = ipAddress,
                MODULENAME = controllerName,
                ACTIONNAME = actionName,
                MESSAGE = responseContent,
                RESULT = status.ToString(),
                CREATOR = user?.TRUENAME,
                CREATETIME = DateTime.Now
            }).ExecuteCommand();
        }
    }
}
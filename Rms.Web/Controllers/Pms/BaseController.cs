using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Newtonsoft.Json.Converters;
using System.Text;
using Rms.Models.DataBase.Pms;

namespace Rms.Web.Controllers
{

    public class BaseController : Controller
    {
        public BaseController()
        { }
        /// <summary>
        ///     
        /// </summary>
        protected string controllerName;
        /// <summary>
        ///     
        /// </summary>
        protected string actionName;

        protected new PMS_USER User => Session["user_account"] as PMS_USER;

        protected List<string> controllers => Session["controllers"] as List<string>;


        /// <summary>
        ///     重写OnActionExecuting，在调用action之前获取路由和账户信息
        /// </summary>
        /// <param name="filterContext">过滤器</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
       
            //获取controller
            if (string.IsNullOrEmpty(controllerName))
            {
                controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
            }
            //获取action
            if (string.IsNullOrEmpty(actionName))
            {
                actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            }

       
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            //base.OnAuthorization(filterContext);  
            //从session中获取登录用户信息，如果用户未登录则跳转到登录界面
            if (!(filterContext.HttpContext.Session["user_account"] is PMS_USER))
            {
                ContentResult Content = new ContentResult
                {
                    Content = "<script type='text/javascript'>top.location = '/Account/login';</script>"
                };
                filterContext.Result = Content;
                return;
            }

            if (!controllers.Contains(filterContext.RouteData.Values["controller"].ToString()))
            {
                ContentResult Content = new ContentResult
                {
                    Content = "<script type='text/javascript'>top.location = '/Account/NoPermission';</script>"
                };
                filterContext.Result = Content;
                return;
            }
        }

      

    }
}
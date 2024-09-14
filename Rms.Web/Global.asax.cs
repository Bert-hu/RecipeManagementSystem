using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Rms.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            // GlobalConfiguration.Configure(WebApiConfig.Register);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            var RabbitMqHost = CommonConfiguration.Configs["RabbitMqHost"];
            var RabbitMqPort = int.Parse(CommonConfiguration.Configs["RabbitMqPort"]);
            var RabbitMqUser = CommonConfiguration.Configs["RabbitMqUser"];
            var RabbitMqPassword = CommonConfiguration.Configs["RabbitMqPassword"];
            var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];


            var isdebugmode = ConfigurationManager.AppSettings["IsDebugMode"].ToUpper() == "TRUE";
            CommonConfiguration.UpdateConfig(isdebugmode);

#if DEBUG

           // RabbitMqService.Initiate("192.168.53.174", "admin", "admin", 5672);
            //RabbitMqService.BeginConsume(ListenChannel);       //debugger »·¾³

#else
      RabbitMqService.Initiate(RabbitMqHost, RabbitMqUser, RabbitMqPassword, RabbitMqPort);
                RabbitMqService.BeginConsume(ListenChannel);
#endif

        }
    }
}

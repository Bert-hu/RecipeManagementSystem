using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rms.Services
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            var isdebugmode = ConfigurationManager.AppSettings["IsDebugMode"].ToUpper() == "TRUE";
            if (isdebugmode)
            {
                MainService service = new MainService();
                service.OnStart();
                while (true) { Thread.Sleep(10000); } ;
            }

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MainService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

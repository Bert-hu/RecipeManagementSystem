using EAP.Services.Utils;
using Quartz;
using Quartz.Impl;

using Rms.Services.Jobs;
using Rms.Services.Services;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rms.Services
{
    public partial class MainService : ServiceBase
    {

        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        public MainService()
        {
            QuartzUtil.Init();
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            try
            {
                StartQuartzJob();
                StartRabbitMqService();
                StartWebApiListener();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message,ex);
                //TODO : 生产环境初始化失败！！！  报警逻辑

            }
        }

        public void OnStart()
        {


            //NetworkCredential networkCredential = new NetworkCredential("admin", "usi");
            //var recipepath = @"\\172.19.11.116\eap\test\rcp1";
            //DirectoryInfo directoryInfo = new DirectoryInfo(recipepath);
            //if (directoryInfo.Exists)
            //{
            //    var body = CompressDirectory(recipepath);

            //    DecompressDirectory(body, recipepath + "1");
            //}
            //else
            //{
            //    using (new NetworkConnection(recipepath, networkCredential))
            //    {
            //        var body = CompressDirectory(recipepath);

            //        DecompressDirectory(body, recipepath + "1");
            //    }
            //}




            StartQuartzJob();
            StartRabbitMqService();
            StartWebApiListener();
        }

        protected override void OnStop()
        {
        }
        private void StartQuartzJob()
        {
            try
            {
                var isdebugmode = ConfigurationManager.AppSettings["IsDebugMode"].ToUpper() == "TRUE";
                if (isdebugmode) //测试模式
                {
                }
                else
                {
                    _ = QuartzUtil.AddJob<RegularDayJob>("RegularDayJob", $"0 0 0 * * ?");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }

        }

        private void StartRabbitMqService()
        {
            try
            {
                var RabbitMqHost = ConfigurationManager.AppSettings["RabbitMqHost"];
                var RabbitMqPort = int.Parse(ConfigurationManager.AppSettings["RabbitMqPort"]);
                var RabbitMqUser = ConfigurationManager.AppSettings["RabbitMqUser"];
                var RabbitMqPassword = ConfigurationManager.AppSettings["RabbitMqPassword"];
                var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
                var ListenReplyChannel = ConfigurationManager.AppSettings["ListenReplyChannel"];
                var isdebugmode = ConfigurationManager.AppSettings["IsDebugMode"].ToUpper() == "TRUE";
                if (isdebugmode) //测试模式
                {
                    RabbitMqService.Initiate("192.168.53.174", "admin", "admin", 5672);
                    RabbitMqService.BeginConsume(ListenChannel);
                }
                else
                {
                    Log.Debug($"{RabbitMqHost}, {RabbitMqUser}, {RabbitMqPassword}, {RabbitMqPort}");
                    RabbitMqService.Initiate(RabbitMqHost, RabbitMqUser, RabbitMqPassword, RabbitMqPort);
                    RabbitMqService.BeginConsume(ListenChannel);
                    RabbitMqService.BeginConsume(ListenReplyChannel);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        private void StartWebApiListener()
        {
            try
            {
                var ApiPort = ConfigurationManager.AppSettings["ApiPort"];
                var isdebugmode = ConfigurationManager.AppSettings["IsDebugMode"].ToUpper() == "TRUE";
                if (isdebugmode) //测试模式
                {
                    APIListener listener = new APIListener(ApiPort, "api");
                    listener.StartListen();
                }
                else
                {
                    APIListener listener = new APIListener(ApiPort, "api");
                    listener.StartListen();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }
    }
}

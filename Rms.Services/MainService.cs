using EAP.Services.Utils;

using Rms.Services.Jobs;
using Rms.Services.Services;
using Rms.Utils;
using System;
using System.Configuration;
using System.ServiceProcess;

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
                Log.Error(ex.Message, ex);
                //TODO : 生产环境初始化失败！！！  报警逻辑

            }
        }

        public void OnStart()
        {
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
                var RabbitMqHost = CommonConfiguration.Configs["RabbitMqHost"];
                var RabbitMqPort = int.Parse(CommonConfiguration.Configs["RabbitMqPort"]);
                var RabbitMqUser = CommonConfiguration.Configs["RabbitMqUser"];
                var RabbitMqPassword = CommonConfiguration.Configs["RabbitMqPassword"];
                var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
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
                var ApiPort = CommonConfiguration.Configs["ApiPort"];
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

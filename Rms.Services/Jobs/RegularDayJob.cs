using Quartz;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Rms.Services.Jobs
{
    public class RegularDayJob : IJob
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
                CleanLog();
                
            });
        }

        private void CleanLog()
        {
            try
            {
                Log.Info("Start Cleaning up the log.");
                var path = AppDomain.CurrentDomain.BaseDirectory + "logs\\"; //文件夹路径
                if (!Directory.Exists(path)) return;
                var dyInfo = new DirectoryInfo(path);
                foreach (var feInfo in dyInfo.GetFiles("*.log*", SearchOption.AllDirectories))
                {
                    if (feInfo.LastWriteTime < DateTime.Now.AddDays(-30)) feInfo.Delete();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

     
    }
}

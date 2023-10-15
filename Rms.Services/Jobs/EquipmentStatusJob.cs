using Quartz;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Rms.Services.Jobs
{
    public class EquipmentStatusJob : IJob
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
            });
        }

  

     
    }
}

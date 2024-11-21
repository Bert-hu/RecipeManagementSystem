
using SqlSugar;
using System.Reflection;

namespace Rms.Services.Core.Services
{
    public class CommonWorker : BackgroundService
    {
        private readonly ISqlSugarClient sqlSugarClient;
            
        public CommonWorker(ISqlSugarClient sqlSugarClient)
        {
            this.sqlSugarClient = sqlSugarClient;
        }   
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //sqlSugarClient.DbFirst.IsCreateAttribute().StringNullable().CreateClassFile(@"..\WaferMapSystem.Models\DataBase\Wms");
            //throw new NotImplementedException();


            //Code First 生成表
            //Assembly wrsmodels = Assembly.Load("Rms.Models");
            //var typesInNamespace = wrsmodels.GetTypes()
            //   .Where(t => t.Namespace != null && t.Namespace.StartsWith("Rms.Models.DataBase.Mms"))
            //   .ToList();
            //
            //foreach (var type in typesInNamespace)
            //{
            //    sqlSugarClient.CodeFirst.InitTables(type);
            //}

            return Task.CompletedTask;
        }
    }
}

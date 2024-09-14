
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

            //Assembly wrsmodels = Assembly.Load("ICOSEAP.Models");
            //var typesInNamespace = wrsmodels.GetTypes()
            //   .Where(t => t.Namespace != null && t.Namespace.StartsWith("ICOSEAP.Models.DataBase"))
            //   .ToList();

            //foreach (var type in typesInNamespace)
            //{
            //    sqlSugarClient.CodeFirst.InitTables(type);
            //}

            return Task.CompletedTask;
        }
    }
}

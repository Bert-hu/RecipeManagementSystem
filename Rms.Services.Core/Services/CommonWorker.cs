
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


            //Code First 生成表,只有第一次采用
            //Assembly wrsmodels = Assembly.Load("Rms.Models");
            //var typesInNamespace = wrsmodels.GetTypes()
            //   .Where(t => t.Namespace != null && t.IsClass && t.Namespace.StartsWith("Rms.Models.DataBase"))
            //   .ToList();

            //foreach (var type in typesInNamespace)
            //{
            //    try
            //    {
            //        if (type.Name == "RecipeBody")
            //        {
            //            continue;
            //        }

            //        sqlSugarClient.CodeFirst.InitTables(type);
            //    }
            //    catch (Exception)
            //    {
            //        continue;
            //    }
            //}

            return Task.CompletedTask;
        }
    }
}

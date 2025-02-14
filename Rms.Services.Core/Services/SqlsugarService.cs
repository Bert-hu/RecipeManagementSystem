using SqlSugar;

namespace Rms.Services.Core.Services
{
    public class SqlsugarService
    {

    }

    public static class SqlSugarServiceExtensions
    {
        public static IServiceCollection AddSqlSugarService(this IServiceCollection services)
        {
            services.AddSingleton<ISqlSugarClient>(ConfigureSqlSugar);


            return services;
        }

        static ISqlSugarClient ConfigureSqlSugar(IServiceProvider serviceProvider)
        {
            IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
            string? oracleConnectionString = configuration.GetConnectionString("OracleDb");

            ConnectionConfig connectionConfig = new ConnectionConfig()
            {
                DbType = DbType.Oracle,
                ConnectionString = oracleConnectionString,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                MoreSettings = new ConnMoreSettings()
                {
                    IsAutoToUpper = false // 是否转大写，默认是转大写的可以禁止转大写
                },
                ConfigureExternalServices = new ConfigureExternalServices//把不包含id的字段设为可空
                {
                    EntityService = (t, column) =>
                    {
                        if (!column.PropertyName.ToLower().Contains("id"))
                        {
                            column.IsNullable = true;
                        }
                    }
                }
            };

            Action<string, SugarParameter[]> onLogExecuting = (sql, pars) =>
            {
                // OnLogExecuting 的逻辑
            };

            SqlSugarScope sqlSugar = new SqlSugarScope(connectionConfig, db =>
            {
                db.Aop.OnLogExecuting = onLogExecuting;
            });

            return sqlSugar;
        }
    }
}

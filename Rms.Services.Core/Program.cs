
using Rms.Services.Core.RabbitMq;
using Rms.Services.Core.Services;
using log4net.Config;
using Microsoft.OpenApi.Models;
using SqlSugar;
using System.Reflection;
using Rms.Services.Core.Rms;
using Rms.Services.Core.Utils;
using Rms.Services.Core.Services.AMS;



namespace WaferMapSystem.Services
{
    public class Program
    {
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSqlSugarService();



            builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            builder.Services.AddSwaggerGen(option =>

            {

                #region SwaggerGen�汾����


                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Rms Api", Version = "v1" });


                #endregion

                #region ���SwaggerGenע��

                //// ʹ�÷����ȡxml�ļ�����������ļ���·��

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                //// ����xmlע�ͣ��÷����ڶ����������ÿ�������ע�ͣ�Ĭ��Ϊfalse��

                option.IncludeXmlComments(xmlPath, true);

                ////��action�����ƽ�����������ж�����Ϳ��Կ���Ч���ˡ�

                //option.OrderActionsBy(o => o.RelativePath);

                #endregion

            });

            //RabbitMq
            builder.Services.AddSingleton<RabbitMqService>();
            builder.Services.AddScoped<RmsTransactionService>();

            builder.Services.AddHostedService<CommonWorker>();
            builder.Services.AddHostedService<RabbitMqWorker>();

            builder.Services.AddHostedService<LockMachineJob>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {

            }
            app.Use(next => context =>
            {
                context.Request.EnableBuffering();
                return next(context);
            });
            app.UseMiddleware<BlockedIpMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(
                c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "AMS Api V1"); }
            );
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});

            app.Run();
        }


    }
}


using Rms.Services.Core.Extension;
using Rms.Services.Core.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Core.Rms;
using Rms.Services.Core.RabbitMq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Rms.Services.Core.Controllers
{
    /// <summary>
    /// Rms相关Api
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public partial class ApiController : Controller
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        private static log4net.ILog ApiLog = log4net.LogManager.GetLogger("ApiLogger");

        private readonly ISqlSugarClient db;
        private readonly RabbitMqService rabbitMq;
        private readonly RmsTransactionService rmsTransactionService;
        private readonly CommonConfiguration commonConfiguration;


        public ApiController(ISqlSugarClient _sqlSugarClient, RabbitMqService _rabbitMq, RmsTransactionService _rmsTransactionService)
        {

            this.db = _sqlSugarClient;
            this.rabbitMq = _rabbitMq;
            this.rmsTransactionService = _rmsTransactionService;
            commonConfiguration = new CommonConfiguration(_sqlSugarClient);
            //this.commonConfiguration = _commonConfiguration;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var requestBody = JsonConvert.SerializeObject(context.ActionArguments.Values.FirstOrDefault());
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var path = context.HttpContext.Request.Path;
            Log.Info($"Request:{ip},{path},{requestBody}");
            ApiLog.Info($"Request:{ip},{path},{requestBody}");
            base.OnActionExecuting(context);

        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var responseBody = JsonConvert.SerializeObject((context.Result as JsonResult)?.Value);
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var path = context.HttpContext.Request.Path;         
            ApiLog.Info($"{ip},{path},{responseBody}");
            base.OnActionExecuted(context);
        }
    }
}

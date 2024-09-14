using log4net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Policy;
using System.Text;

namespace Rms.Services.Core.Extension
{
    public class LogAttribute : ActionFilterAttribute
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            try
            {
                var requestip = context.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
                var statuscode = context.HttpContext.Response.StatusCode; 
                if (statuscode == 200)
                {

                    string url = context.HttpContext.Request.GetDisplayUrl();
                    string requestmethod = context.HttpContext.Request.Method;

                    //Request Body的内容
                    var requestBody = string.Empty;
                    var request = context.HttpContext.Request;
                    request.Body.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(request.Body, Encoding.UTF8))
                    {
                        requestBody = reader.ReadToEndAsync().Result;
                    }

                    //Response Body的内容
                    var responseBody = string.Empty;
                    var result = context.Result;
                    var contentResult = result as ContentResult;
                    if (contentResult != null)
                    {
                        responseBody = contentResult.Content;
                    }
                    else
                    {
                        var jsonResult = result as JsonResult;
                        if (jsonResult != null)
                        {
                            responseBody = Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult.Value);
                        }
                        else
                        {
                            responseBody = result?.ToString();
                        }
                    }
                    //Log record
                    Log.Info($"Request IP:{requestip}, Request URL:{url}, Request Method:{requestmethod}, Request Body:{requestBody}, Response Body:{responseBody}"); 
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

    }
}
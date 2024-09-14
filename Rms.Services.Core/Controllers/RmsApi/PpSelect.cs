
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
using Rms.Models.RabbitMq;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        [HttpPost]
        public JsonResult PpSelect(PpSelectRequest req)
        {
            var res = new PpSelectResponse();
            string rabbitmqroute = $"EAP.SecsClient.{req.EquipmentId}";

            var trans = new RabbitMqTransaction
            {
                TransactionName = "PpSelect",
                EquipmentID = req.EquipmentId,
                NeedReply = true,
                ExpireSecond = 30,
                Parameters = new Dictionary<string, object>() { { "RecipeName", req.RecipeName } }
            };
            var rabbitres = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);
            if (rabbitres != null)
            {
                var result = rabbitres.Parameters["Result"]?.ToString()?.ToUpper() == "TRUE";
                rabbitres.Parameters.TryGetValue("Message", out object? message);
                res.Result = result;
                res.Message = message.ToString();
                res.RecipeName = req.RecipeName;
            }
            else//Rabbit Mq失败
            {
                Log.Error($"{req.EquipmentId} PP-Select timeout!");
                res.Result = false;
                res.Message = $"EAP RabbitMq Request timeout";
            }
            return Json(res);
        }

    }
}

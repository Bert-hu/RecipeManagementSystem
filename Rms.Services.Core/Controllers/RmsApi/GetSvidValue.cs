
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
        public JsonResult GetSvidValue(GetSvidValueRequest req)
        {

            var res = new GetSvidValueResponse();

            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetSvidValue",
                EquipmentID = req.EquipmentId,
                NeedReply = true,
                ExpireSecond = 3,
                Parameters = new Dictionary<string, object>() { { "VidList", req.VidList } }
            };

            var rabbitmqroute = $"EAP.SecsClient.{req.EquipmentId}";
            var rabbitRes = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);

            if (rabbitRes != null)
            {
            }
            else//Rabbit Mq失败
            {
                res.Message = "Equipment offline or EAP client error!";
            }
            return Json(res);
        }

    }
}

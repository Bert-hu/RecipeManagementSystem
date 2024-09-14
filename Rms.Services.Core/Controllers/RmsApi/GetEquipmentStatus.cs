
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
        public JsonResult GetEquipmentStatus(GetEquipmentStatusRequest req)
        {
            var res = new GetEquipmentStatusResponse();

            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetEquipmentStatus",
                EquipmentID = req.EquipmentId,
                NeedReply = true,
                ExpireSecond = 2,
            };

            var rabbitmqroute = $"EAP.SecsClient.{req.EquipmentId}";
            var rabbitRes = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);

            if (rabbitRes != null)
            {
                if (rabbitRes.Parameters.TryGetValue("Status", out object status))
                {
                    res.Result = true;
                    res.Status = status.ToString();
                }
                else
                {
                    res.Message = "EAP Error!";
                }
            }
            else//Rabbit Mq失败
            {
                res.Result = true;
                res.Status = "No reply";
            }
            return Json(res);
        }

    }
}

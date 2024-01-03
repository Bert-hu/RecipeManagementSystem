using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Services.Services.ApiHandle.MessageHandler;
using Rms.Utils;
using RMS.Domain.Rms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    public partial class GetEquipmentStatus:CommonHandler
    {
        /// <summary>
        /// 新建Recipe
        /// </summary>
        /// <param name="jsoncontent"></param>
        /// <returns></returns>
        public override ResponseMessage Handle(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new GetEquipmentStatusResponse();
            var req = JsonConvert.DeserializeObject<GetEquipmentStatusRequest>(jsoncontent);

            var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetEquipmentStatus",
                EquipmentID = req.EquipmentId,
                NeedReply = true,
                ReplyChannel = ListenChannel,
            };

            var rabbitmqroute = $"EAP.SecsClient.{req.EquipmentId}";
            var rabbitRes = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 1);
         
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
                res.Status ="No reply";
            }


            return res;
        }

    }
}

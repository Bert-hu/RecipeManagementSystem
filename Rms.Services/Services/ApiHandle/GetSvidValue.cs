using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;
using RMS.Domain.Rms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rms.Services.Services.ApiHandle
{
    public partial class ApiMessageHandler
    {
        /// <summary>
        /// 新建Recipe
        /// </summary>
        /// <param name="jsoncontent"></param>
        /// <returns></returns>
        public ResponseMessage GetSvidValue(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new GetSvidValueResponse();
            var req = JsonConvert.DeserializeObject<GetSvidValueRequest>(jsoncontent);

            var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetSvidValue",
                EquipmentID = req.EquipmentId,
                NeedReply = true,
                ReplyChannel = ListenChannel,
                Parameters = new Dictionary<string, object>() { { "VidList", req.VidList } }
            };

            var rabbitmqroute = $"EAP.SecsClient.{req.EquipmentId}";
            var rabbitRes = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 3);

            if (rabbitRes != null)
            { 
            }
            else//Rabbit Mq失败
            {
                res.Message = "Equipment offline or EAP client error!";
            }


            return res;
        }

    }
}

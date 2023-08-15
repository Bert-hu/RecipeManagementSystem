using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Services.ApiHandle
{
    public partial class ApiMessageHandler
    {
        public ResponseMessage GetEPPD(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new GetEppdResponse();
            var req = JsonConvert.DeserializeObject<GetEppdRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EuipmentId)?.First();
            string rabbitmqroute = string.Empty;
            if (eqp == null)
            {
                res.Message = "EQID not exists!";
                return res;
            }
            //识别设备类型和恢复queue
            switch (eqp.RECIPE_TYPE)
            {
                default:
                    rabbitmqroute = $"EAP.SecsClient.{req.EuipmentId}";
                    break;
            }
            var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetEPPD",
                EquipmentID = req.EuipmentId,
                NeedReply = true,
                ReplyChannel = ListenChannel,
            };
            var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 5);
            if (rabbitres != null)
            {
                res.EPPD = JsonConvert.DeserializeObject<List<string>>(rabbitres.Parameters["EPPD"].ToString());
            }
            else//Rabbit Mq失败
            {
                Log.Debug($"Rabbit Mq send to {rabbitmqroute}\n:{JsonConvert.SerializeObject(trans, Formatting.Indented)}");
                Log.Error($"GetEPPD Time out!");
                res.Message = "GetEPPD Time out!";
            }

            res.Result = true;
            return res;
        }
    }
}

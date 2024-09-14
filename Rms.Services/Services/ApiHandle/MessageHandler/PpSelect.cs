using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    internal class PpSelect : CommonHandler
    {
        public override ResponseMessage Handle(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new PpSelectResponse();
            var req = JsonConvert.DeserializeObject<PpSelectRequest>(jsoncontent);
            string rabbitmqroute = $"EAP.SecsClient.{req.EquipmentId}";

            var trans = new RabbitMqTransaction
            {
                TransactionName = "PpSelect",
                EquipmentID = req.EquipmentId,
                NeedReply = true,
                Parameters = new Dictionary<string, object>() { { "RecipeName", req.RecipeName } }
            };
            var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 30);
            if (rabbitres != null)
            {
                var result = rabbitres.Parameters["Result"].ToString().ToUpper() == "TRUE";
                rabbitres.Parameters.TryGetValue("Message", out object message);
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
            return res;
        }
    }
}

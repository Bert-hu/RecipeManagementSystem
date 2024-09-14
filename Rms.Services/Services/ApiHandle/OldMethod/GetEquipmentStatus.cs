using Newtonsoft.Json;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;

namespace Rms.Services.Services.ApiHandle
{
    public partial class ApiMessageHandler
    {
        /// <summary>
        /// 新建Recipe
        /// </summary>
        /// <param name="jsoncontent"></param>
        /// <returns></returns>
        public ResponseMessage GetEquipmentStatus(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new GetEquipmentStatusResponse();
            var req = JsonConvert.DeserializeObject<GetEquipmentStatusRequest>(jsoncontent);

            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetEquipmentStatus",
                EquipmentID = req.EquipmentId,
                NeedReply = true,
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
                    res.Message = "EAP client error!";
                }          
            }
            else//Rabbit Mq失败
            {
                res.Result = true;
                res.Status ="EAP not reply";
            }


            return res;
        }

    }
}

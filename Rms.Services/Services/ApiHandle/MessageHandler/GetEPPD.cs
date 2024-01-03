using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;
using System.Collections.Generic;
using System.Configuration;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    public partial class GetEPPD : CommonHandler
    {
        public override ResponseMessage Handle(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new GetEppdResponse();
            var req = JsonConvert.DeserializeObject<GetEppdRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId)?.First();
            string rabbitmqroute = string.Empty;
            if (eqp == null)
            {
                res.Message = "EQID not exists!";
                return res;
            }
            //识别设备类型和恢复queue
            (res.Result, res.Message, res.EPPD) = GetEppd(eqp.ID);
            return res;
        }
    }
}

using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Services.Services.ApiHandle.MessageHandler;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    public partial class DeleteAllRecipes : CommonHandler
    {
        public override ResponseMessage Handle(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new DeleteAllRecipesResponse();
            var req = JsonConvert.DeserializeObject<DeleteAllRecipesRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId)?.First();
            string rabbitmqroute = string.Empty;
            if (eqp == null)
            {
                res.Message = "EQID not exists!";
                return res;
            }
            //TODO FILE和DIRECTORY类型处理删除
            switch (eqp.RECIPE_TYPE)
            {
                case "secsByte":
                case "secsSml":
                    return SecsDeleteAllRecipeTrans(eqp.RECIPE_TYPE, req.EquipmentId);
                default:
                    break;
            }
            return res;
        }

        public ResponseMessage SecsDeleteAllRecipeTrans(string RecipeType, string EquipmentID)
        {
            var res = new DeleteAllRecipesResponse();

            string rabbitmqroute = string.Empty;
            rabbitmqroute = $"EAP.SecsClient.{EquipmentID}";

            var trans = new RabbitMqTransaction
            {
                TransactionName = "DeleteAllRecipes",
                EquipmentID = EquipmentID,
                NeedReply = true,
            };
            var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 30);
            if (rabbitres != null)
            {
                res.Result = rabbitres.Parameters["Result"].ToString().ToUpper() == "TRUE";
            }
            else//Rabbit Mq失败
            {
                Log.Error($"Delete all recipe Time out!");
                res.Message = "Equipment offline or EAP client error!";
            }
            return res;
        }
    }
}

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
        public ResponseMessage DeleteAllRecipes(string jsoncontent)
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

            var rabbitres = DeleteAllRecipes(eqp.RECIPE_TYPE, req.EquipmentId);

            #region 返回是否是离线
            bool isOffline = false;
            if (rabbitres.Parameters.TryGetValue("Status", out object status))
            {
                isOffline = status.ToString().ToUpper() == "OFFLINE";
            }
            if (isOffline)
            {
                res.Result = false;
                res.Message = "Equipment offline!";
                return res;
            }
            #endregion

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

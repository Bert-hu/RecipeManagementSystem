using Newtonsoft.Json;
using Rms.Models.RabbitMq;
using Rms.Utils;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Services.ApiHandle.RecipeTypeFunction
{
    internal class OnlyName : IRecipeType
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        SqlSugarClient db = DbFactory.GetSqlSugarClient();
        public (bool result, string message) DeleteAllMachineRecipes(string EquipmentId)
        {
            throw new NotImplementedException();
        }

        public (bool result, string message) DownloadRecipeToMachine(string EquipmentId, string RecipeVersionId)
        {
            throw new NotImplementedException();
        }

        public (bool result, string message, List<string> eppd) GetEppd(string EquipmentId)
        {
            try
            {
                var rabbitmqroute = $"EAP.SecsClient.{EquipmentId}";
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "GetEPPD",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                };
                var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 5);
                if (rabbitres != null)
                {
                    if (rabbitres.Parameters.TryGetValue("EPPD", out object eppdstr))
                    {
                        var eppd = JsonConvert.DeserializeObject<List<string>>(eppdstr.ToString());

                        return (true, string.Empty, eppd);
                    }
                    else
                    {
                        rabbitres.Parameters.TryGetValue("Message", out object message);
                        return (false, message?.ToString(), null);
                    }
                }
                else//Rabbit Mq失败
                {
                    Log.Debug($"Rabbit Mq send to {rabbitmqroute}\n:{JsonConvert.SerializeObject(trans, Formatting.Indented)}");
                    Log.Error($"GetEPPD Time out!");
                    return (false, "EAP Reply Timeout", null);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, "Rms Service Error", null);
            }
        }

        public (bool result, string message, byte[] body) UploadRecipeToServer(string EquipmentId, string RecipeName)
        {
            return (true, string.Empty, null);
        }
    }
}

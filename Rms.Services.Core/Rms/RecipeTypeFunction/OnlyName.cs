using Newtonsoft.Json;
using Rms.Models.RabbitMq;
using Rms.Services.Core.RabbitMq;
using SqlSugar;

namespace Rms.Services.Core.Rms.RecipeTypeFunction
{
    internal class OnlyName : IRecipeType
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        protected ISqlSugarClient db;
        protected RabbitMqService rabbitMq;

        public OnlyName(ISqlSugarClient _db, RabbitMqService _rabbitMq)
        {
            db = _db;
            rabbitMq = _rabbitMq;
        }

        public (bool result, string message) DeleteMachineRecipe(string EquipmentId, string RecipeName)
        {
            throw new NotImplementedException();
        }
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
                    ExpireSecond = 5,
                    NeedReply = true,

                };
                var rabbitres = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);
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

        public (bool result, string message) CompareRecipe(string EquipmentId, string RecipeVersionId)
        {
            throw new NotImplementedException();
        }
    }
}

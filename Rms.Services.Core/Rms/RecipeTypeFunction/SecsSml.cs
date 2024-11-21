using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Services.Core.RabbitMq;
using Rms.Services.Core.Utils;
using SqlSugar;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Rms.Services.Core.Rms.RecipeTypeFunction
{
    internal class SecsSml : SecsByte, IRecipeType
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        public SecsSml(ISqlSugarClient _db, RabbitMqService _rabbitMq) : base(_db, _rabbitMq)
        {
        }


        public new (bool result, string message) DownloadRecipeToMachine(string EquipmentId, string RecipeVersionId)
        {
            try
            {
                var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().InSingle(RecipeVersionId);
                if (recipe_version.RECIPE_DATA_ID == null)
                {
                    return (false, "Recipe do not have data.");
                }
                var eqp = db.Queryable<RMS_EQUIPMENT>().InSingle(EquipmentId);
                var eqtpye = db.Queryable<RMS_EQUIPMENT_TYPE>().InSingle(eqp.TYPE);
                if (eqtpye.DELETEBEFOREDOWNLOAD)
                {
                    DeleteAllMachineRecipes(EquipmentId);//暂时不处理删除的答复
                }
                var recipe = db.Queryable<RMS_RECIPE>().InSingle(recipe_version.RECIPE_ID);
                var serverdata = db.Queryable<RMS_RECIPE_DATA>().InSingle(recipe_version.RECIPE_DATA_ID)?.CONTENT;
                var dataobj = ByteArrayToObject(serverdata);

                string rabbitMqRoute = $"EAP.SecsClient.{EquipmentId}";
                var body = Convert.ToBase64String((dataobj as RecipeBody).UnformattedBody);
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "SetUnformattedRecipe",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ExpireSecond = 60,
                    Parameters = new Dictionary<string, object>() { { "RecipeName", recipe.NAME }, { "RecipeBody", body } }
                };
                var rabbitRes = rabbitMq.ProduceWaitReply(rabbitMqRoute, trans);
                if (rabbitRes != null)
                {
                    var result = rabbitRes.Parameters["Result"].ToString().ToUpper() == "TRUE";
                    rabbitRes.Parameters.TryGetValue("Message", out object message);
                    return (result, message?.ToString());
                }
                else//Rabbit Mq失败
                {
                    Log.Error($"Delete all recipe Time out!");
                    return (false, "Equipment offline or EAP client error!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, "Rms Service Error");
            }
        }


        public new (bool result, string message, byte[] body) UploadRecipeToServer(string EquipmentId, string RecipeName)
        {
            string rabbitmqRoute = $"EAP.SecsClient.{EquipmentId}";

            var trans1 = new RabbitMqTransaction
            {
                TransactionName = "GetUnformattedRecipe",
                EquipmentID = EquipmentId,
                NeedReply = true,
                ExpireSecond = 60,
                Parameters = new Dictionary<string, object>() { { "RecipeName", RecipeName } }

            };
            var rabbitRes1 = rabbitMq.ProduceWaitReply(rabbitmqRoute, trans1);

            var trans2 = new RabbitMqTransaction
            {
                TransactionName = "GetFormattedRecipe",
                EquipmentID = EquipmentId,
                NeedReply = true,
                ExpireSecond = 60,
                Parameters = new Dictionary<string, object>() { { "RecipeName", RecipeName } }
            };
            var rabbitRes2 = rabbitMq.ProduceWaitReply(rabbitmqRoute, trans2);
            if (rabbitRes1 != null && rabbitRes2 != null)
            {
                var result = rabbitRes1.Parameters["Result"].ToString().ToUpper() == "TRUE" && rabbitRes2.Parameters["Result"].ToString().ToUpper() == "TRUE";

                rabbitRes1.Parameters.TryGetValue("Message", out object message);
                rabbitRes2.Parameters.TryGetValue("Message", out message);

                var unformattedBody = Convert.FromBase64String(rabbitRes1.Parameters["RecipeBody"].ToString());
                var formattedBody = System.Text.Encoding.Unicode.GetBytes(rabbitRes2.Parameters["RecipeBody"].ToString());
                var body = ObjectToByteArray(new RecipeBody { UnformattedBody = unformattedBody, FormattedBody = formattedBody });

                return (result, message?.ToString(), body);
            }
            else//Rabbit Mq失败
            {
                Log.Error($"Get Recipe Body Time out!");
                return (false, "EAP Reply Timeout", null);
            }

        }

        public (bool result, string message) CompareRecipe(string EquipmentId, string RecipeVersionId)
        {
            try
            {
                var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().InSingle(RecipeVersionId);
                if (recipe_version.RECIPE_DATA_ID == null)
                {
                    return (false, "Recipe do not have data.");
                }
                var recipe = db.Queryable<RMS_RECIPE>().InSingle(recipe_version.RECIPE_ID);

                var serverdata = db.Queryable<RMS_RECIPE_DATA>().InSingle(recipe_version.RECIPE_DATA_ID)?.CONTENT;
                //从serverdata从取出Unformatted Body
                var dataobj = ByteArrayToObject(serverdata);

                //var data = db.Queryable<RMS_RECIPE_DATA>().InSingle(recipe_version.RECIPE_DATA_ID)?.CONTENT;
                string rabbitMqRoute = $"EAP.SecsClient.{EquipmentId}";
                var body = Convert.ToBase64String((dataobj as RecipeBody).UnformattedBody);
                var paras = Encoding.Unicode.GetString((dataobj as RecipeBody).FormattedBody);
                //var body = Convert.ToBase64String(data);

                var trans = new RabbitMqTransaction
                {
                    TransactionName = "CompareRecipe",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ExpireSecond = 15,
                    Parameters = new Dictionary<string, object>() { { "RecipeName", recipe.NAME }, { "RecipeBody", body }, { "RecipeParameters", paras } }
                };
                var rabbitRes = rabbitMq.ProduceWaitReply(rabbitMqRoute, trans);
                if (rabbitRes != null)
                {
                    var result = rabbitRes.Parameters["Result"].ToString().ToUpper() == "TRUE";
                    rabbitRes.Parameters.TryGetValue("Message", out object message);
                    return (result, message?.ToString());
                }
                else//Rabbit Mq失败
                {
                    Log.Error($"Compare recipe Time out!");
                    return (false, "Equipment offline or EAP client error!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, "Rms Service Error");
            }
        }


        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;

            IFormatter formatter = new BinaryFormatter();
            formatter.Binder = new USerializationBinder();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }



        }

        public object ByteArrayToObject(byte[] data)
        {
            if (data == null)
                return null;

            IFormatter formatter = new BinaryFormatter();
            formatter.Binder = new USerializationBinder();
            using (MemoryStream stream = new MemoryStream(data))
            {
                return formatter.Deserialize(stream);
            }


        }
    }



}

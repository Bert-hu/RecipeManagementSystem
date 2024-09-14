using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Utils;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Rms.Services.Services.ApiHandle.RecipeTypeFunction
{
    internal class SecsSml : SecsByte, IRecipeType
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        SqlSugarClient db = DbFactory.GetSqlSugarClient();


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
                    Parameters = new Dictionary<string, object>() { { "RecipeName", recipe.NAME }, { "RecipeBody", body } }
                };
                var rabbitRes = RabbitMqService.ProduceWaitReply(rabbitMqRoute, trans, 60);
                if (rabbitRes != null)
                {
                    var result = rabbitRes.Parameters["Result"].ToString().ToUpper() == "TRUE";
                    rabbitRes.Parameters.TryGetValue("Message", out object message);
                    return (result, message?.ToString());
                }
                else//Rabbit Mq失败
                {
                    Log.Error($"Time out!");
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
                Parameters = new Dictionary<string, object>() { { "RecipeName", RecipeName } }
            };
            var rabbitRes1 = RabbitMqService.ProduceWaitReply(rabbitmqRoute, trans1, 60);

            var trans2 = new RabbitMqTransaction
            {
                TransactionName = "GetFormattedRecipe",
                EquipmentID = EquipmentId,
                NeedReply = true,
                Parameters = new Dictionary<string, object>() { { "RecipeName", RecipeName } }
            };
            var rabbitRes2 = RabbitMqService.ProduceWaitReply(rabbitmqRoute, trans2, 60);
            if (rabbitRes1 != null && rabbitRes2 != null)
            {
                var result = (rabbitRes1.Parameters["Result"].ToString().ToUpper() == "TRUE") && (rabbitRes2.Parameters["Result"].ToString().ToUpper() == "TRUE");

                rabbitRes1.Parameters.TryGetValue("Message", out object message);
                rabbitRes2.Parameters.TryGetValue("Message", out message);

                var unformattedBody = Convert.FromBase64String(rabbitRes1.Parameters["RecipeBody"].ToString());
                var formattedBody = System.Text.Encoding.Unicode.GetBytes(rabbitRes2.Parameters["RecipeBody"].ToString());
                var body = ObjectToByteArray(new RecipeBody { UnformattedBody = unformattedBody, FormattedBody = formattedBody });

                var obj = (RecipeBody)ByteArrayToObject(body);

                return (result, message?.ToString(), body);
            }
            else//Rabbit Mq失败
            {
                Log.Error($"Get Recipe Body Time out!");
                return (false, "EAP Reply Timeout", null);
            }

        }
 

        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter formatter = new BinaryFormatter();
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

            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(data))
            {
                return formatter.Deserialize(stream);
            }
        }
    }
}

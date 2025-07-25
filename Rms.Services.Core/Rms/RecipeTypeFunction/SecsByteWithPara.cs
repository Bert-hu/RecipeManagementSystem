﻿using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Services.Core.RabbitMq;
using Rms.Services.Core.Utils;
using SqlSugar;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;

namespace Rms.Services.Core.Rms.RecipeTypeFunction
{
    internal class SecsByteWithPara : IRecipeType
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        protected ISqlSugarClient db;
        protected RabbitMqService rabbitMq;

        public SecsByteWithPara(ISqlSugarClient _db, RabbitMqService _rabbitMq)
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
            try
            {
                string rabbitmqroute = $"EAP.SecsClient.{EquipmentId}";

                var trans = new RabbitMqTransaction
                {
                    TransactionName = "DeleteAllRecipes",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ExpireSecond = 30
                };
                var rabbitres = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);
                if (rabbitres != null)
                {
                    var result = rabbitres.Parameters["Result"].ToString().ToUpper() == "TRUE";
                    rabbitres.Parameters.TryGetValue("Message", out object message);
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

        public (bool result, string message) DownloadRecipeToMachine(string EquipmentId, string RecipeVersionId)
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
                //从serverdata从取出Unformatted Body
                var dataobj = ByteArrayToObject(serverdata);

                string rabbitMqRoute = $"EAP.SecsClient.{EquipmentId}";
                //var body = Convert.ToBase64String(data);
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
                    Log.Error($"Download recipe Time out!");
                    return (false, "Equipment offline or EAP client error!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, "Rms Service Error");
            }
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
                    ExpireSecond = 5
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
            string rabbitmqRoute = $"EAP.SecsClient.{EquipmentId}";
            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetUnformattedRecipe",
                EquipmentID = EquipmentId,
                NeedReply = true,
                ExpireSecond = 60,
                Parameters = new Dictionary<string, object>() { { "RecipeName", RecipeName } }
            };
            var rabbitRes = rabbitMq.ProduceWaitReply(rabbitmqRoute, trans);
            if (rabbitRes != null)
            {
                var result = rabbitRes.Parameters["Result"].ToString().ToUpper() == "TRUE";
                rabbitRes.Parameters.TryGetValue("Message", out object message);
                byte[] body = null;
                if (result)
                {
                    var unformattedBody = Convert.FromBase64String(rabbitRes.Parameters["RecipeBody"].ToString());
                    //Formatted Body 或者字符串
                    var formattedBody = System.Text.Encoding.Unicode.GetBytes(rabbitRes.Parameters["RecipeParameters"].ToString());
                    body = ObjectToByteArray(new RecipeBody { UnformattedBody = unformattedBody, FormattedBody = formattedBody });

                }
                //Unformatted Body


                // var body = Convert.FromBase64String(rabbitRes.Parameters["RecipeBody"].ToString());
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
                var eqp = db.Queryable<RMS_EQUIPMENT>().InSingle(EquipmentId);

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

                var specList = db.Queryable<RMS_PARAMETER_SCOPE>().Where(it => it.RecipeID == recipe.ID).ToList();
                var specDictList = db.Queryable<RMS_PARAMETER_DIC>().Where(it => it.EQ_TYPE_ID == eqp.TYPE).ToList();
                var specResult = (from a in specList
                                  join b in specDictList on a.ParamKey equals b.ID
                                  select new RMS_PARAMETER_SCOPE
                                  {
                                      ID = a.ID,
                                      PARAMETER_NAME = b.Key,
                                      Type = a.Type,
                                      LCL = a.LCL,
                                      UCL = a.UCL,
                                      EnumValue = a.EnumValue,

                                  }).ToList();


                var trans = new RabbitMqTransaction
                {
                    TransactionName = "CompareRecipe",
                    EquipmentID = EquipmentId,
                    NeedReply = true,
                    ExpireSecond = 15,
                    Parameters = new Dictionary<string, object>() { { "RecipeName", recipe.NAME }, { "RecipeBody", body },{ "RecipeParameters" , paras } ,{ "RecipeParameterScope", specResult } }
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

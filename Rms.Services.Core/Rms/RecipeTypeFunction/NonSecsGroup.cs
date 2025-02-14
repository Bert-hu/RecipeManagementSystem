using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Services.Core.RabbitMq;
using SqlSugar;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rms.Model;

namespace Rms.Services.Core.Rms.RecipeTypeFunction
{
    internal class NonSecsGroup : IRecipeType
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        protected ISqlSugarClient db;
        protected RabbitMqService rabbitMq;

        //Deburr #9 EQP.Client = EQRMS00001
        //其余组合设备，以EQID+'_'+分站号命名
        protected Dictionary<string,string> channelDictionary = new Dictionary<string, string>()
        {
            {"EQAMR00012_01","EQRMS00001" },
            {"EQAMR00012_02","EQRMS00001" },
            {"EQAMR00012_03","EQRMS00001" }
        };
        public NonSecsGroup(ISqlSugarClient _db, RabbitMqService _rabbitMq)
        {
            db = _db;
            rabbitMq = _rabbitMq;
        }

        public (bool result, string message) DeleteAllMachineRecipes(string EquipmentId)
        {
            throw new NotImplementedException();
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

                var recipe = db.Queryable<RMS_RECIPE>().InSingle(recipe_version.RECIPE_ID);
                var data = db.Queryable<RMS_RECIPE_DATA>().InSingle(recipe_version.RECIPE_DATA_ID)?.CONTENT;

                var channelID = channelDictionary.TryGetValue(EquipmentId, out var channel)? channel : EquipmentId.Split('_')[0];
                var rabbitmqroute = $"EAP.Client.{channelID}";
                var body = Encoding.UTF8.GetString(data);

                var para = new Dictionary<string, object> {
                                    {"EQID", EquipmentId},
                                    {"RecipeName", recipe.NAME},
                                    {"RecipeBody", body}
                };
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "SetRecipeBody",
                    EquipmentID = channelID,
                    NeedReply = true,
                    ExpireSecond = 60,
                    Parameters = para
                };
                var rabbitRes = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);
                if (rabbitRes != null)
                {
                    var result = rabbitRes.Parameters["Result"].ToString().Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase);
                    string message = result? "Download Successfully!": "Download Failed！Please retry.";
                    return (result, message.ToString());
                }
                else//Rabbit Mq失败
                {
                    Log.Error($"Download recipe Time out!");
                    return (false, "EAP Reply Timeout: Equipment offline or EAP client error.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, "RMS Service Error");
            }
        }

        public (bool result, string message, List<string> eppd) GetEppd(string EquipmentId)
        {
            try
            {

                var channelID = channelDictionary.TryGetValue(EquipmentId, out var channel) ? channel : EquipmentId.Split('_')[0];

                var rabbitmqroute = $"EAP.Client.{channelID}";//$"EAP.Client.{req.EuipmentId}"; //EQRMS00001
                var para = new Dictionary<string, object> {
                                    {"EQID", EquipmentId}
                                };
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "GetEPPD",
                    EquipmentID = channelID,
                    NeedReply = true,
                    //ReplyChannel = ListenChannel,
                    Parameters = para,
                    ExpireSecond = 60
                };

                Log.Info("Send to:" + rabbitmqroute + "; Message: " + JsonConvert.SerializeObject(trans));
                var rabbitres = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);
                Log.Info("Received:" + JsonConvert.SerializeObject(rabbitres));


                if (rabbitres != null)
                {
                    //这里可能有问题，需要实际测试调整
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
                else
                {
                    Log.Debug($"Rabbit Mq send to {rabbitmqroute}\n:{JsonConvert.SerializeObject(trans, Formatting.Indented)}");
                    Log.Error($"GetEPPD Time out!");
                    return (false, "EAP Reply Timeout: Equipment offline or EAP client error.", null);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, "Rms Service Error", null);
            }
        }

        /// <summary>
        /// AddNewRecipe/ReloadRecipeBody
        /// </summary>
        /// <param name="EquipmentId"></param>
        /// <param name="RecipeName"></param>
        /// <returns></returns>
        public (bool result, string message, byte[] body) UploadRecipeToServer(string EquipmentId, string RecipeName)
        {
            var channelID = channelDictionary.TryGetValue(EquipmentId, out var channel) ? channel : EquipmentId.Split('_')[0];
            var rabbitmqroute = $"EAP.Client.{channelID}";
            var para = new Dictionary<string, object> {
                    {"EQID", EquipmentId },
                    {"RecipeName", RecipeName }
                };
            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetRecipeBodyToUpgrade",
                EquipmentID = channelID,
                NeedReply = true,
                ExpireSecond = 60,
                Parameters = para
            };
            Log.Debug("Send:" + JsonConvert.SerializeObject(trans));
            var rabbitRes = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);
            Log.Debug("Received:" + JsonConvert.SerializeObject(rabbitRes));

            if (rabbitRes != null)
            {

                rabbitRes.Parameters.TryGetValue("Message", out object message);
                var recipebody = string.Empty;
                if (rabbitRes.Parameters.TryGetValue("RecipeBody", out object _body)) recipebody = _body?.ToString();
               

                if (!String.IsNullOrEmpty(recipebody))
                {
                    //非标设备需要检查recipe版本之间的差异
                    //确认reload recipebody的场景
                    //Add New Recipe / Add Reipce Version
                    var eqp = db.Queryable<RMS_EQUIPMENT>().In(EquipmentId).First();
                    var eqType = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
                    var recipeRecord = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == EquipmentId && it.NAME == RecipeName).First();
                    if (recipeRecord != null)
                    {
                        //var recipe_latest_version = db.Queryable<RMS_RECIPE_VERSION>().InSingle(recipeRecord.VERSION_LATEST_ID);
                        var recipe_effective_version = db.Queryable<RMS_RECIPE_VERSION>().InSingle(recipeRecord.VERSION_EFFECTIVE_ID);
                        if (recipe_effective_version.RECIPE_DATA_ID != null)
                        {
                            //当recipe有effective版本的RECIPE_DATA的时候，判断为Add Version，比对内容
                            var recipe_effective_data = db.Queryable<RMS_RECIPE_DATA>().InSingle(recipe_effective_version.RECIPE_DATA_ID)?.CONTENT;
                            var recipe_effective_body = Encoding.UTF8.GetString(recipe_effective_data);

                            var specList = db.Queryable<RMS_PARAMETER_SCOPE>().Where(it => it.RecipeID == recipeRecord.ID).ToList();
                            var specDictList = db.Queryable<RMS_PARAMETER_DIC>().Where(it => it.EQ_TYPE_ID == eqType.ID).ToList();
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
                            CompareRecipeBody(EquipmentId, recipeRecord.NAME, recipe_effective_body, recipebody, specResult, out bool isDifferent, out RMS_DIFFERENT_PARAMETER different_list);
                            //将有变化的内容转成JSON String，抛到前端处理
                            if (isDifferent) message = JsonConvert.SerializeObject(different_list);
                            
                        }

                    }
                }

                byte[] RecipeBody = Encoding.UTF8.GetBytes(recipebody);
                return (true, message?.ToString(), RecipeBody);


            }
            else//Rabbit Mq失败
            {
                Log.Error($"Get Recipe Body Time out!");
                return (false, "EAP Reply Timeout", null);
            }

        }
        public (bool result, string message) CompareRecipe(string EquipmentId, string RecipeVersionId)
        {
            throw new NotImplementedException();
        }

        public static void CompareRecipeBody(string EQID, string RecipeName, string golden, string recipebody, List<RMS_PARAMETER_SCOPE> specList, out bool isDifferent, out RMS_DIFFERENT_PARAMETER rep_list)
        {
            isDifferent = false;
            var different_list = new List<Parameter>();
            rep_list = new RMS_DIFFERENT_PARAMETER()
            {
                EQID = EQID,
                RecipeName = RecipeName,
                Parameter = different_list
            };

            try
            {
                JObject recipeObj = JObject.Parse(recipebody);
                JObject goldenObj = JObject.Parse(golden);

                foreach (var property in recipeObj.Properties())
                {
                    var key = property.Name;
                    var value1 = property.Value;
                    var spec = specList.Find(it => it.PARAMETER_NAME == key);

                    if (goldenObj.TryGetValue(key, out var goldenValue))
                    {
                        if (spec == null && !JToken.DeepEquals(value1, goldenValue))
                        {
                            //isDifferent = true;
                            //Log.Info($"{EQID}_{RecipeName}_ParameterDifferent_{key}_{value1}_{goldenValue}");
                            var item = new Parameter()
                            {
                                ErrorCode = "ParameterDifferent",
                                Key = key,
                                Value = value1.ToString(),
                                SPEC = goldenValue.ToString()
                            };

                            different_list.Add(item);
                        }
                        else if (spec != null && !JToken.DeepEquals(value1, goldenValue))
                        {
                            if (spec.Type == "floatvalue")
                            {
                                //是一个浮动范围
                                if (((double)goldenValue - spec.LCL) > (double)value1)
                                {

                                    //isDifferent = true;
                                    //Log.Info($"{EQID}_{RecipeName}_ParameterExceedLCL_{key}_{value1}_{goldenValue}");
                                    var item = new Parameter()
                                    {
                                        ErrorCode = "ParameterExceedLCL",
                                        Key = key,
                                        Value = value1.ToString(),
                                        SPEC = goldenValue.ToString()
                                    };

                                    different_list.Add(item);

                                }
                                else if (((double)goldenValue + spec.UCL) < (double)value1)
                                {


                                    //Log.Info($"{EQID}_{RecipeName}_ParameterExceedUCL_{key}_{value1}_{goldenValue}");
                                    var item = new Parameter()
                                    {
                                        ErrorCode = "ParameterExceedUCL",
                                        Key = key,
                                        Value = value1.ToString(),
                                        SPEC = goldenValue.ToString()
                                    };


                                    different_list.Add(item);

                                }

                            }
                            else if (spec.Type == "percent")
                            {
                                if (((double)goldenValue * (1 - spec.LCL * 0.01)) > (double)value1)
                                {
                                    //isDifferent = true;
                                    //Log.Info($"{EQID}_{RecipeName}_ParameterExceedLCL_{key}_{value1}_{goldenValue}");
                                    var item = new Parameter()
                                    {
                                        ErrorCode = "ParameterExceedLCL",
                                        Key = key,
                                        Value = value1.ToString(),
                                        SPEC = goldenValue.ToString()
                                    };


                                    different_list.Add(item);

                                }
                                else if (((double)goldenValue * (1 + spec.LCL * 0.01)) < (double)value1)
                                {
                                    //isDifferent = true;
                                    //Log.Info($"{EQID}_{RecipeName}_ParameterExceedUCL_{key}_{value1}_{goldenValue}");

                                    var item = new Parameter()
                                    {
                                        ErrorCode = "ParameterExceedUCL",
                                        Key = key,
                                        Value = value1.ToString(),
                                        SPEC = goldenValue.ToString()
                                    };


                                    different_list.Add(item);

                                }

                            }
                        }
                    }
                    else
                    {
                        //isDifferent = true;
                        //Log.Info($"{EQID}_{RecipeName}_SPECNotFound_{key}_{value1}_'null'");

                        var item = new Parameter()
                        {
                            ErrorCode = "SPECNotFound",
                            Key = key,
                            Value = value1.ToString(),
                            SPEC = goldenValue.ToString()
                        };

                        different_list.Add(item);

                    }



                }

                isDifferent = different_list.Count > 0;
                rep_list = new RMS_DIFFERENT_PARAMETER()
                {
                    EQID = EQID,
                    RecipeName = RecipeName,
                    Parameter = different_list
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }



        }

    }
}

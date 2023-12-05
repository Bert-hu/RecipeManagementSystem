using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Production
{
    public class WaferGrindingController : CommonProductionController
    {

        // GET: WaferGrinding
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetEquipments()
        {
            var db = DbFactory.GetSqlSugarClient();
            var data = db.Queryable<RMS_EQUIPMENT>().Where(it => it.TYPE == "WaferGrinding").ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PortLotStart(string equipmentid, string lotid, string port)
        {
            string sfis_step1_req = $"{equipmentid},{lotid},1,M068397,JORDAN,,OK,";
            string sfis_step1_res = string.Empty;
            string errmsg = string.Empty;
            string repmsg = $"Port:{port},Lot:{lotid}";
            bool result = false;

            if (SendMessageToSfis(sfis_step1_req, ref sfis_step1_res, ref errmsg))
            {
                if (sfis_step1_res.ToUpper().StartsWith("OK"))//SFIS获取LOT INFO成功
                {
                    string sfis_step7_req = $"{equipmentid},{lotid},7,M068397,JORDAN,,OK,MODEL_NAME=???";
                    string sfis_step7_res = string.Empty;
                    if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg)) //获取modelname（recipe group）方便后面pp-select调用
                    {
                        Dictionary<string, string> sfispara = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                       .Where(keyValueArray => keyValueArray.Length == 2)
                  .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                        string recipegroup = sfispara["MODEL_NAME"];
                        repmsg += $",Recipe Group:{recipegroup}";
                        var recipe = GetRecipeWithGroupName(equipmentid, recipegroup, out string msg);
                        if (recipe == null)//recipegroup mapping recipe 失败
                        {
                            repmsg += msg;
                        }
                        else
                        {
                            repmsg += $",Recipe Name:{recipe.NAME}";
                            var db = DbFactory.GetSqlSugarClient();
                            var eqp = db.Queryable<RMS_EQUIPMENT>().In(equipmentid).First();
                            if (eqp.LASTRUN_RECIPE_ID != recipe.ID)
                            {
                                repmsg += ",当前Recipe和上次不同，请重新下载";
                            }
                            else if (eqp.LASTRUN_RECIPE_TIME < DateTime.Now.AddHours(-2))
                            {
                                repmsg += ",当前Recipe 2小时未运行，请重新下载";
                            }
                            else
                            {
                                var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];
                                var para = new Dictionary<string, object>() { { "LotName", lotid }, { "RecipeName", recipe.NAME } };
                                var trans = new RabbitMqTransaction
                                {
                                    TransactionName = $"Port{port}LotStart",
                                    EquipmentID = equipmentid,
                                    NeedReply = true,
                                    ReplyChannel = ListenChannel,
                                    Parameters = para
                                };
                                var rabbitmqroute = $"EAP.SecsClient.{equipmentid}";
                                var rabbitRes = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 5);
                                if (rabbitRes == null)
                                {
                                    repmsg += "Machine do not reply, pls check EAP client";
                                }
                                else
                                {
                                    if (rabbitRes.Parameters.TryGetValue("Result", out object _rec)) result = (bool)_rec;
                                    if (rabbitRes.Parameters.TryGetValue("Message", out object _rec1)) repmsg += _rec1?.ToString();
                                    if (result)
                                    {
                                        eqp.LASTRUN_RECIPE_TIME = DateTime.Now;
                                        db.Updateable<RMS_EQUIPMENT>(eqp).ExecuteCommand();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        repmsg += $"Can not connect to SFIS";
                    }
                }
                else//SFIS获取LOT INFO失败
                {
                    repmsg += $"Sfis reply FAIL:{sfis_step1_res}";
                }
            }
            else
            {
                repmsg += $"Can not connect to SFIS";
            }
            AddProductionLog(equipmentid, "PortLotStart", result.ToString(), repmsg);
            return Json(new { Result = result, Message = repmsg }, JsonRequestBehavior.AllowGet);
        }

      

    }
}
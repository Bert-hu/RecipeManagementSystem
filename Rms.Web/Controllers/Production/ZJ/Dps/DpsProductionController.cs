using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Utils;
using Rms.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Production
{
    [Route("[controller]/[action]")]
    public class DpsProductionController : CommonProductionController
    {

        // GET: WaferGrinding
        public ActionResult Index()
        {
            return View("~/Views/Production/DPS/index.cshtml");
        }

        public JsonResult GetEquipments()
        {
            var db = DbFactory.GetSqlSugarClient();

            var sql = @"select RE.* from RMS_EQUIPMENT RE,RMS_EQUIPMENT_TYPE RET
where RE.TYPE = RET.ID
AND RE.LINE = 'DPS'
ORDER BY RET.ORDERSORT,RE.ORDERSORT";
            var data = db.SqlQueryable<RMS_EQUIPMENT>(sql).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PortLotStart(string equipmentid, string lotid, string port)
        {
            var empno = User.ID.Length >= 8 ? "M" + User.ID.Substring(2, 6) : User.ID;

            string sfis_step1_req = $"{equipmentid},{lotid},1,{empno},JORDAN,,OK,";
            string sfis_step1_res = string.Empty;
            string errmsg = string.Empty;
            string repmsg = $"Port:{port},Lot:{lotid}";
            bool result = false;

            if (SendMessageToSfis(sfis_step1_req, ref sfis_step1_res, ref errmsg))
            {
                if (sfis_step1_res.ToUpper().StartsWith("OK"))//SFIS获取LOT INFO成功
                {
                    string sfis_step7_req = $"{equipmentid},{lotid},7,{empno},JORDAN,,OK,MODEL_NAME=???";
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


        public virtual JsonResult LotIn(string equipmentid, string lotid)
        {
            if (User == null)
            {
                return Json(new { Result = false, Message = "未登录" }, JsonRequestBehavior.AllowGet);
            }
            bool result = false;
            string repmsg = $"Lot:{lotid},";

            try
            {

                var empno = User.ID.Length >= 8 ? "M" + User.ID.Substring(2, 6) : User.ID;

                var baymaxEquipmentId = equipmentid + "-IN";
                //STEP7---STEP1---STEP2
                string errmsg = string.Empty;


                string sfis_step7_req = $"{baymaxEquipmentId},{lotid},7,{empno},JORDAN,,OK,MODEL_NAME=???";
                string sfis_step7_res = string.Empty;
                if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg))
                {
                    if (sfis_step7_res.ToUpper().StartsWith("OK"))//SFIS获取LOT INFO成功
                    {

                        Dictionary<string, string> sfisParameters = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
              .Where(keyValueArray => keyValueArray.Length == 2)
              .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                        string modelName = sfisParameters["MODEL_NAME"];



                        var db = DbFactory.GetSqlSugarClient();
                        var sql = string.Format(@"SELECT  
    RE.ID AS EQID, 
    RET.ID AS ETID, 
    MMD.ID AS MID,
    MMD.SHOWNAME AS SHOWNAME,
    MMD.TYPE AS MTYPE,
    MMC.ID AS MMCID,
    MMC.VALUE AS VALUE,
    MMC.LASTEDITOR AS LASTEDITOR,
    MMC.LASTEDITTIME AS LASTEDITTIME,
		MMD.ORDER_SORT
FROM RMS_EQUIPMENT RE
JOIN RMS_EQUIPMENT_TYPE RET
    ON RE.TYPE = RET.ID
JOIN MMS_MATERIAL_DIC MMD
    ON MMD.EQUIPMENT_TYPE_ID = RET.ID
LEFT JOIN MMS_MACHINE_CONFIG MMC
    ON MMC.MMDID = MMD.ID
WHERE RE.ID = '{0}'
ORDER BY ORDER_SORT", equipmentid);
                        var data = db.SqlQueryable<MachineConfigVM>(sql).ToList();

                        var materials = data.Where(x => x.MTYPE == "Material").ToList();
                        var toolings = data.Where(x => x.MTYPE == "Tooling").ToList();
                        var materialString = materials.Count > 0 ? "REEL_ID=" + string.Join(";", materials.Select(x => $"{x.VALUE}")) : string.Empty;
                        var toolingString = toolings.Count > 0 ? "TOOLING=" + string.Join(";", toolings.Select(x => $"{x.SHOWNAME}={x.VALUE}")) : string.Empty;
                        //都不为空时空格连接
                        var materialAndToolingString = !string.IsNullOrEmpty(materialString) && !string.IsNullOrEmpty(toolingString) ? $"{materialString} {toolingString}" : $"{materialString}{toolingString}";



                        string sfis_step1_req = $"{baymaxEquipmentId},{lotid},1,{empno},JORDAN,,OK,{materialAndToolingString};";
                        string sfis_step1_res = string.Empty;
                        if (SendMessageToSfis(sfis_step1_req, ref sfis_step1_res, ref errmsg))
                        {
                            if (sfis_step1_res.ToUpper().StartsWith("OK"))//SFIS获取LOT INFO成功
                            {
                                var recipe = GetRecipeWithGroupName(equipmentid, modelName, out string msg);
                                if (recipe == null)//recipegroup mapping recipe 失败
                                {
                                    repmsg += msg;
                                }
                                else
                                {
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
                                        string sfis_step2_req = $"{baymaxEquipmentId},{lotid},2,{empno},JORDAN,,OK,";
                                        string sfis_step2_res = string.Empty;
                                        if (SendMessageToSfis(sfis_step2_req, ref sfis_step2_res, ref errmsg))
                                        {
                                            if (sfis_step2_res.ToUpper().StartsWith("OK"))//SFIS获取LOT INFO成功
                                            {
                                                repmsg += " LotIn OK";
                                                result = true;
                                                eqp.CURRENT_PRODUCT = lotid;
                                                eqp.LASTRUN_RECIPE_ID = recipe.ID;
                                                eqp.LASTRUN_RECIPE_TIME = DateTime.Now;
                                                db.Updateable<RMS_EQUIPMENT>(eqp).UpdateColumns(it => new { it.CURRENT_PRODUCT, it.LASTRUN_RECIPE_ID, it.LASTRUN_RECIPE_TIME });
                                            }
                                            else//SFIS获取LOT INFO失败
                                            {
                                                repmsg += $"Sfis reply FAIL:{sfis_step2_req},{sfis_step2_res}";
                                            }
                                        }
                                        else
                                        {
                                            repmsg += $"Can not connect to SFIS";
                                        }
                                    }
                                }
                            }
                            else//SFIS获取LOT INFO失败
                            {
                                repmsg += $"Sfis reply FAIL:{sfis_step1_req},{sfis_step1_res}";
                            }
                        }
                        else
                        {
                            repmsg += $"Can not connect to SFIS";
                        }
                    }
                    else
                    {
                        repmsg += $"Sfis reply FAIL:{sfis_step7_res}";
                    }
                }
                else
                {
                    repmsg += $"Can not connect to SFIS";
                }
            }
            catch (Exception ex)
            {
                repmsg = $"Lot In 异常：{ex.Message}";
            }

            AddProductionLog(equipmentid, "LotIn", result.ToString(), repmsg);
            return Json(new { Result = result, Message = repmsg }, JsonRequestBehavior.AllowGet);
        }


        public virtual JsonResult LotOut(string equipmentid, string lotid)
        {
            if (User == null)
            {
                return Json(new { Result = false, Message = "未登录" }, JsonRequestBehavior.AllowGet);
            }
            var empno = User.ID.Length >= 8 ? "M" + User.ID.Substring(2, 6) : User.ID;

            //STEP7---STEP1---STEP2
            string errmsg = string.Empty;
            string repmsg = $"Lot:{lotid},";
            bool result = false;
            try
            {
                string sfis_step2_req = $"{equipmentid},{lotid},2,{empno},JORDAN,,OK,";
                string sfis_step2_res = string.Empty;
                if (SendMessageToSfis(sfis_step2_req, ref sfis_step2_res, ref errmsg))
                {
                    if (sfis_step2_res.ToUpper().StartsWith("OK"))//SFIS获取LOT INFO成功
                    {
                        repmsg += " LotOut OK";
                        result = true;

                        var eqp = db.Queryable<RMS_EQUIPMENT>().In(equipmentid).First();
                        eqp.CURRENT_PRODUCT = lotid;
                        eqp.LASTRUN_RECIPE_TIME = DateTime.Now;
                        db.Updateable<RMS_EQUIPMENT>(eqp).UpdateColumns(it => new { it.CURRENT_PRODUCT, it.LASTRUN_RECIPE_TIME });
                    }
                    else//SFIS获取LOT INFO失败
                    {
                        repmsg += $"Sfis reply FAIL:{sfis_step2_req},{sfis_step2_res}";
                    }
                }
                else
                {
                    repmsg += $"Can not connect to SFIS";
                }


            }
            catch (Exception ex)
            {
                repmsg = $"Lot Out 异常：{ex.Message}";

            }
            AddProductionLog(equipmentid, "LotOut", result.ToString(), repmsg);
            return Json(new { Result = result, Message = repmsg }, JsonRequestBehavior.AllowGet);
        }

    }
}
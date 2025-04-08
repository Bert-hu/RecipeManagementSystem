using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        /// <summary>
        /// 删除所有Recipe
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteAllRecipes(DeleteAllRecipesRequest req)
        {
            var res = new DeleteAllRecipesResponse();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId)?.First();
            string rabbitmqroute = string.Empty;
            if (eqp == null)
            {
                res.Message = "EQID not exists!";
                return Json(res);
            }

            (res.Result, res.Message) = rmsTransactionService.DeleteAllMachineRecipes(eqp);

            //TODO FILE和DIRECTORY类型处理删除
            //switch (eqp.RECIPE_TYPE)
            //{
            //    case "secsByte":
            //    case "secsSml":
            //        return Json(SecsDeleteAllRecipeTrans(eqp.RECIPE_TYPE, req.EquipmentId));
            //    default:
            //        break;
            //}
            return Json(res);

           
        }
        //ResponseMessage SecsDeleteAllRecipeTrans(string RecipeType, string EquipmentID)
        //{
        //    var res = new DeleteAllRecipesResponse();

        //    string rabbitmqroute = string.Empty;
        //    rabbitmqroute = $"EAP.SecsClient.{EquipmentID}";

        //    var trans = new RabbitMqTransaction
        //    {
        //        TransactionName = "DeleteAllRecipes",
        //        EquipmentID = EquipmentID,
        //        NeedReply = true,
        //        ExpireSecond = 30
        //    };
        //    var rabbitres = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);
        //    if (rabbitres != null)
        //    {
        //        res.Result = rabbitres.Parameters["Result"].ToString().ToUpper() == "TRUE";
        //    }
        //    else//Rabbit Mq失败
        //    {
        //        Log.Error($"Delete all recipe Time out!");
        //        res.Message = "Equipment offline or EAP client error!";
        //    }
        //    return res;
        //}

    }
}

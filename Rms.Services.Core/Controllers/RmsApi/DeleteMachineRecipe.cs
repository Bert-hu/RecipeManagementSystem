using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Services.Core.Rms;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        /// <summary>
        /// 删除Recipe
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteMachineRecipe(DeleteMachineRecipeRequest req)
        {
            var res = new DeleteMachineRecipeResponse();

            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId)?.First();
            RmsTransactionService service = new RmsTransactionService(db, rabbitMq);
            (res.Result, res.Message) = service.DeleteMachineRecipe(eqp, req.RecipeName);
            return Json(res);
        }


    }
}

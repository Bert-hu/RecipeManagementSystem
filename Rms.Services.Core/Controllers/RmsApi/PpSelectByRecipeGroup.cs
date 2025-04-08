
using Rms.Services.Core.Extension;
using Rms.Services.Core.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Core.Rms;
using Rms.Models.RabbitMq;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        [HttpPost]
        public JsonResult PpSelectByRecipeGroup(PpSelectByRecipeGroupRequest req)
        {
            var res = new PpSelectByRecipeGroupResponse();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            if (eqp == null)
            {
                res.Message = $"Euipment '{req.EquipmentId}' not exists!";
                return Json(res);
            }

            var eqpType = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            RMS_EQUIPMENT goldenEqp = eqp;//默认为自己

            if (!string.IsNullOrEmpty(eqpType.GOLDEN_EQID) && eqpType.GOLDEN_RECIPE_TYPE)
            {
                goldenEqp = db.Queryable<RMS_EQUIPMENT>().In(goldenEqp.FATHER_EQID).First();
            }
            var recipegroup = db.Queryable<RMS_RECIPE_GROUP>().First(it => it.NAME == req.RecipeGroupName);
            if (recipegroup == null)
            {
                db.Insertable<RMS_RECIPE_GROUP>(new RMS_RECIPE_GROUP { NAME = req.RecipeGroupName }).ExecuteCommand();
                res.Result = false;
                res.Message = $"Unable to find recipe bound to '{req.RecipeGroupName}'";
                return Json(res);
            }

            var allRecipes = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == goldenEqp.ID).ToList();
            var allRecipeIds = allRecipes.Select(it => it.ID).ToList();
            var binding = db.Queryable<RMS_RECIPE_GROUP_MAPPING>().Where(it => it.RECIPE_GROUP_ID == recipegroup.ID && allRecipeIds.Contains(it.RECIPE_ID)).First();
            if (binding == null)
            {
                res.Result = false;
                res.Message = $"Unable to find recipe bound to '{req.RecipeGroupName}'";
                return Json(res);
            }
            var recipe = db.Queryable<RMS_RECIPE>().In(binding.RECIPE_ID).First();

            string rabbitmqroute = $"EAP.SecsClient.{req.EquipmentId}";

            var trans = new RabbitMqTransaction
            {
                TransactionName = "PpSelect",
                EquipmentID = req.EquipmentId,
                NeedReply = true,
                ExpireSecond = 30,
                Parameters = new Dictionary<string, object>() { { "RecipeName", recipe.NAME } }
            };
            var rabbitres = rabbitMq.ProduceWaitReply(rabbitmqroute, trans);
            if (rabbitres != null)
            {
                var result = rabbitres.Parameters["Result"].ToString()?.ToUpper() == "TRUE";
                rabbitres.Parameters.TryGetValue("Message", out object? message);
                res.Result = result;
                res.Message = message.ToString();
                res.RecipeName = recipe.NAME;
            }
            else//Rabbit Mq失败
            {
                Log.Error($"{req.EquipmentId} PP-Select timeout!");
                res.Result = false;
                res.Message = $"EAP RabbitMq Request timeout";
            }
            return Json(res);
        }

    }
}

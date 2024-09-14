using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    internal class PpSelectByRecipeGroup : CommonHandler
    {
        public override ResponseMessage Handle(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new PpSelectByRecipeGroupResponse();
            var req = JsonConvert.DeserializeObject<PpSelectByRecipeGroupRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            var eqtype = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            var recipegroup = db.Queryable<RMS_RECIPE_GROUP>().First(it => it.NAME == req.RecipeGroupName);
            if (recipegroup == null)
            {
                db.Insertable<RMS_RECIPE_GROUP>(new RMS_RECIPE_GROUP { NAME = req.RecipeGroupName }).ExecuteCommand();
                res.Result = false;
                res.Message = $"Unable to find recipe bound to '{req.RecipeGroupName}'";
                return res;
            }
            if (eqp == null)
            {
                res.Result = false;
                res.Message = "Equipment does not exist in RMS";
                return res;
            }
            var data = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == req.EquipmentId).ToList();
            var eqrecipeids = data.Select(it => it.ID).ToList();
            var binding = db.Queryable<RMS_RECIPE_GROUP_MAPPING>().Where(it => it.RECIPE_GROUP_ID == recipegroup.ID && eqrecipeids.Contains(it.RECIPE_ID)).First();
            if (binding == null)
            {
                res.Result = false;
                res.Message = $"Unable to find recipe bound to '{req.RecipeGroupName}'";
                return res;
            }
            var recipe = db.Queryable<RMS_RECIPE>().In(binding.RECIPE_ID).First();

            string rabbitmqroute = $"EAP.SecsClient.{req.EquipmentId}";

            var trans = new RabbitMqTransaction
            {
                TransactionName = "PpSelect",
                EquipmentID = req.EquipmentId,
                NeedReply = true,
                Parameters = new Dictionary<string, object>() { { "RecipeName", recipe.NAME } }
            };
            var rabbitres = RabbitMqService.ProduceWaitReply(rabbitmqroute, trans, 30);
            if (rabbitres != null)
            {
                var result = rabbitres.Parameters["Result"].ToString().ToUpper() == "TRUE";
                rabbitres.Parameters.TryGetValue("Message", out object message);
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
            return res;
        }
    }
}

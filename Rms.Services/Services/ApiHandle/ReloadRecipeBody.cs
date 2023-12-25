using Newtonsoft.Json;
using RMS.Domain.Rms;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Services.ApiHandle
{
    public partial class ApiMessageHandler
    {
        public ResponseMessage ReloadRecipeBody(string jsonContent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new ReloadRecipeBodyResponse();
            var req = JsonConvert.DeserializeObject<ReloadRecipeBodyRequest>(jsonContent);
            var recipeVersion = db.Queryable<RMS_RECIPE_VERSION>().In(req.VersionId).First();
            var recipe = db.Queryable<RMS_RECIPE>().In(recipeVersion.RECIPE_ID).First();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
            if (recipe.VERSION_LATEST_ID != req.VersionId)
            {
                res.Message = "Only the latest version can reload body!";
                return res;
            }
            if (recipeVersion.CURRENT_FLOW_INDEX != -1)
            {
                res.Message = "Only the unsubmitted recipe version can reload body!";
                return res;
            }
            var rabbitRes = GetSecsRecipe(eqp.RECIPE_TYPE, eqp.ID, recipe.NAME);
            bool isOffline = false;
            if (rabbitRes != null)
            {
                if (!IsDebugMode)
                {
                    if (rabbitRes.Parameters.TryGetValue("Status", out object status))
                    {
                        isOffline = status.ToString().ToUpper() == "OFFLINE";
                    }
                    if (isOffline)
                    {
                        res.Result = false;
                        res.Message = "Equipment offline!";
                        return res;
                    }
                }

                if (!IsDebugMode && rabbitRes.Parameters["RecipeName"].ToString() != req.RecipeName)
                {
                    res.Message = $"The RecipeName of the response({rabbitRes.Parameters["RecipeName"].ToString()}) is inconsistent with the request({req.RecipeName})";
                    return res;
                }

                byte[] body;
                switch (eqp.RECIPE_TYPE)
                {
                    case "secsByte":
                        body = Convert.FromBase64String(rabbitRes.Parameters["RecipeBody"].ToString());
                        break;
                    case "secsSml":
                        body = System.Text.Encoding.Unicode.GetBytes(rabbitRes.Parameters["RecipeBody"].ToString());
                        break;
                    default:
                        body = Convert.FromBase64String(rabbitRes.Parameters["RecipeBody"].ToString());
                        break;
                }

                db.BeginTran();
                try
                {
                    if (!string.IsNullOrEmpty(recipeVersion.RECIPE_DATA_ID))
                    {
                        db.Deleteable<RMS_RECIPE_DATA>().In(recipeVersion.RECIPE_DATA_ID).ExecuteCommand();
                    }
                    var data = new RMS_RECIPE_DATA
                    {
                        NAME = req.RecipeName,
                        CONTENT = body,
                        CREATOR = req.TrueName,
                    };
                    db.Insertable<RMS_RECIPE_DATA>(data).ExecuteCommand();
                    recipeVersion.RECIPE_DATA_ID = data.ID;
                    db.Updateable<RMS_RECIPE_VERSION>(recipeVersion).UpdateColumns(it => new { it.RECIPE_DATA_ID }).ExecuteCommand();
                    res.RECIPE_DATA_ID = data.ID;
                }
                catch
                {
                    db.RollbackTran();
                    throw;
                }
                db.CommitTran();
                res.Result = true;
            }
            else
            {
                res.Message = "Equipment offline or EAP client error!";
            }

            return res;
        }

    }
}

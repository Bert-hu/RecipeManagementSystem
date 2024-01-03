using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using RMS.Domain.Rms;
using System;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    public partial class ReloadRecipeBody : CommonHandler
    {
        public override ResponseMessage Handle(string jsonContent)
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

            (bool result, string message, byte[] body) = UploadRecipeToServer(eqp.ID, req.RecipeName);
            if (result) 
            {
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
                catch(Exception ex)
                {
                    db.RollbackTran();
                    Log.Error(ex.Message, ex);
                    res.Message = $"RMS Service Error:{ex.Message}";
                    return res;
                }
                db.CommitTran();
                res.Result = true;
            }
            else
            {
                res.Message = message;
                return res;
            }
            return res;
        }

    }
}

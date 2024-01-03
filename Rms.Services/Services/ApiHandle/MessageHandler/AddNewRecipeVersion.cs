using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using RMS.Domain.Rms;
using System;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    public partial class AddNewRecipeVersion:CommonHandler
    {
        public override ResponseMessage Handle(string jsonContent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new AddNewRecipeVersionResponse();
            var req = JsonConvert.DeserializeObject<AddNewRecipeVersionRequest>(jsonContent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            if (eqp == null)
            {
                res.Message = $"Equipment '{req.EquipmentId}' does not exist!";
                return res;
            }
            var eqType = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            if (eqp.RECIPE_TYPE == "onlyName")
            {
                res.Message = "Recipe Type 'onlyName' cannot add a new version!";
                return res;
            }

            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.ID == req.RecipeId && it.EQUIPMENT_ID == req.EquipmentId).First();
            if (recipe.VERSION_EFFECTIVE_ID != recipe.VERSION_LATEST_ID)
            {
                res.Message = "Last version is not finished, cannot add a new version!";
                return res;
            }

            db.BeginTran();
            try
            {
                var lastVersion = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_LATEST_ID).First();
                RMS_RECIPE_VERSION newVersion;

                // 添加 Version
                newVersion = new RMS_RECIPE_VERSION
                {
                    RECIPE_ID = req.RecipeId,
                    VERSION = lastVersion.VERSION + 1,
                    _FLOW_ROLES = eqType.FLOWROLEIDS,
                    CURRENT_FLOW_INDEX = -1,
                    CREATOR = req.TrueName,
                };
                db.Insertable<RMS_RECIPE_VERSION>(newVersion).ExecuteCommand();

                // 更新外键
                recipe.VERSION_LATEST_ID = newVersion.ID;
                recipe.VERSION_EFFECTIVE_ID = newVersion.ID;
                db.Updateable<RMS_RECIPE>(recipe).UpdateColumns(it => new { it.VERSION_LATEST_ID }).ExecuteCommand();
                res.VERSION_LATEST_ID = newVersion.ID;
                db.CommitTran();
                res.Result = true;
                return res;
            }
            catch(Exception ex)
            {
                db.RollbackTran();
                Log.Error(ex.Message, ex);
                res.Message = $"RMS Service Error:{ex.Message}";
                return res;
            }          
        }
    }
}

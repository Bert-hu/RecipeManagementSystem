using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using System;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    public partial class AddNewRecipe : CommonHandler
    {
        /// <summary>
        /// 新建Recipe
        /// </summary>
        /// <param name="jsoncontent"></param>
        /// <returns></returns>
        public override ResponseMessage Handle(string jsoncontent)
        {
            
            var res = new AddNewRecipeResponse();
            var req = JsonConvert.DeserializeObject<AddNewRecipeRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
            if (eqp == null)
            {
                res.Message = $"Euipment '{req.EquipmentId}' not exists!";
                return res;
            }
            //检查同名recipe
            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == req.EquipmentId && it.NAME == req.RecipeName)?.First();
            if (recipe != null)//检查是否存在同名
            {
                res.Message = $"Recipe '{req.RecipeName}' already exists!";
                return res;
            }

            (bool result,string message,byte[] body) = UploadRecipeToServer(eqp.ID, req.RecipeName);

            if (result)
            {
                db.BeginTran();
                try
                {
                    recipe = new RMS_RECIPE
                    {
                        EQUIPMENT_ID = req.EquipmentId,
                        NAME = req.RecipeName
                    };
                    db.Insertable<RMS_RECIPE>(recipe).ExecuteCommand();

                    var eqtype = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();

                    var version = new RMS_RECIPE_VERSION
                    {
                        RECIPE_ID = recipe.ID,
                        _FLOW_ROLES = eqtype.FLOWROLEIDS,
                        CURRENT_FLOW_INDEX = 100,
                        REMARK = "First Version",
                        CREATOR = req.TrueName
                    };
                    db.Insertable<RMS_RECIPE_VERSION>(version).ExecuteCommand();
                    if(body != null) //Recipe 为OnlyName返回result为true，body为null
                    {
                        var data = new RMS_RECIPE_DATA
                        {
                            NAME = req.RecipeName,
                            CONTENT = body,
                            CREATOR = req.TrueName,
                        };
                        db.Insertable<RMS_RECIPE_DATA>(data).ExecuteCommand();

                        version.RECIPE_DATA_ID = data.ID;
                        db.Updateable<RMS_RECIPE_VERSION>(version).UpdateColumns(it => new { it.RECIPE_DATA_ID }).ExecuteCommand();
                    }                  

                    recipe.VERSION_LATEST_ID = version.ID;
                    recipe.VERSION_EFFECTIVE_ID = version.ID;
                    db.Updateable<RMS_RECIPE>(recipe).UpdateColumns(it => new { it.VERSION_LATEST_ID, it.VERSION_EFFECTIVE_ID }).ExecuteCommand();

                    res.RECIPE_ID = recipe.ID;
                    res.VERSION_LATEST_ID = recipe.VERSION_LATEST_ID;

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
            else
            {
                res.Message = message;
                return res;
            }

        }

    }
}

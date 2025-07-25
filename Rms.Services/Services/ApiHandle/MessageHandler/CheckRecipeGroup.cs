﻿using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using System;
using System.Linq;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    public partial class CheckRecipeGroup : CommonHandler
    {
        public override ResponseMessage Handle(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new CheckRecipeGroupResponse();
            var req = JsonConvert.DeserializeObject<CheckRecipeGroupRequest>(jsoncontent);
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId).First();
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
            if (req.CheckLastRecipe)
            {
                if (recipe.ID != eqp.LASTRUN_RECIPE_ID)
                {
                    res.RecipeName = recipe.NAME;
                    res.Message = $"The last run of the recipe is inconsistent, please download it again";
                }
                else if (eqp.LASTRUN_RECIPE_TIME < DateTime.Now.AddHours(-2))
                {
                    res.RecipeName = recipe.NAME;
                    res.Message = "The recipe did not run for 2 hours, please download it again";
                }
                else
                {
                    res.Result = true;
                    res.RecipeName = recipe.NAME;
                    eqp.LASTRUN_RECIPE_TIME = DateTime.Now;
                    db.Updateable<RMS_EQUIPMENT>(eqp).ExecuteCommand();
                }
            }
            else//不检查，只记录
            {
                res.Result = true;
                res.RecipeName = recipe.NAME;
                eqp.LASTRUN_RECIPE_TIME = DateTime.Now;
                db.Updateable<RMS_EQUIPMENT>(eqp).ExecuteCommand();
            }

            return res;
        }
    }
}

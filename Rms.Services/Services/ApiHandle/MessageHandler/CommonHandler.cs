using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Services.ApiHandle.RecipeTypeFunction;
using Rms.Utils;
using RMS.Domain.Rms;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    public class CommonHandler : IMessageHandler
    {
        internal static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        internal bool IsDebugMode = ConfigurationManager.AppSettings["IsDebugMode"].ToUpper() == "TRUE";
        internal SqlSugarClient db = DbFactory.GetSqlSugarClient();
        public virtual ResponseMessage Handle(string jsoncontent)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Get Equipment Process Program Dictionary
        /// </summary>
        /// <param name="EquipmentId"></param>
        /// <returns></returns>
        internal (bool result, string message, List<string> eppd) GetEppd(string EquipmentId)
        {
            var typeinstance = GetInstanceWithEuipmentId(EquipmentId);
            if (typeinstance == null) return (false, "Recipe type do not support!", null);
            else return typeinstance.GetEppd(EquipmentId);
        }
        /// <summary>
        /// Delete Machine Recipe
        /// </summary>
        /// <param name="EquipmentId"></param>
        /// <returns></returns>
        internal (bool result, string message) DeleteAllMachineRecipes(string EquipmentId)
        {
            var typeinstance = GetInstanceWithEuipmentId(EquipmentId);
            if (typeinstance == null) return (false, "Recipe type do not support!");
            else return typeinstance.DeleteAllMachineRecipes(EquipmentId);
        }
        /// <summary>
        /// Download Recipe Body From Server to Machine
        /// </summary>
        /// <param name="RecipeVersionId"></param>
        /// <returns></returns>
        internal (bool result, string message) DownloadRecipeToMachine(string EquipmentId,string RecipeVersionId)
        {
            var typeinstance = GetInstanceWithEuipmentId(EquipmentId);
            if (typeinstance == null) return (false, "Recipe type do not support!");
            else return typeinstance.DownloadRecipeToMachine(EquipmentId, RecipeVersionId);
        }
        /// <summary>
        /// Upload Recipe From Machine To Server
        /// </summary>
        /// <param name="EquipmentId"></param>
        /// <param name="RecipeName"></param>
        /// <returns></returns>
        internal (bool result, string message, byte[]body) UploadRecipeToServer(string EquipmentId, string RecipeName)
        {
            var typeinstance = GetInstanceWithEuipmentId(EquipmentId);
            if (typeinstance == null) return (false, "Recipe type do not support!", null);
            else return typeinstance.UploadRecipeToServer(EquipmentId, RecipeName);
        }

        private IRecipeType GetInstanceWithEuipmentId(string EquipmentId)
        {
            var eqp = db.Queryable<RMS_EQUIPMENT>().InSingle(EquipmentId);
            var assembly = Assembly.GetExecutingAssembly();
            var handlers = assembly.GetTypes()
                .Where(t => typeof(IRecipeType).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToDictionary(t => t.Name.ToLower(), t => Activator.CreateInstance(t) as IRecipeType);

            if (handlers.TryGetValue(eqp.RECIPE_TYPE.ToLower(), out var handler))
            {
                return handler;
            }
            else
            {
                return null;
            }

        }

    }
}

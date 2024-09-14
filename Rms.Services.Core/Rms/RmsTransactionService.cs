using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Core.RabbitMq;
using Rms.Services.Core.Rms.RecipeTypeFunction;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Rms.Services.Core.Rms
{
    public class RmsTransactionService
    {
        internal static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        ISqlSugarClient sqlSugarClient;
        RabbitMqService rabbitMq;
        public RmsTransactionService(ISqlSugarClient _sqlSugarClient, RabbitMqService _rabbitMq)
        {
            this.sqlSugarClient = _sqlSugarClient;
            rabbitMq = _rabbitMq;
        }
   

        internal (bool result, string message, List<string>? eppd) GetEppd(RMS_EQUIPMENT eqp)
        {
            var typeinstance = GetInstanceWithEuipmentId(eqp);
            if (typeinstance == null) return (false, "Recipe type do not support!", null);
            else return typeinstance.GetEppd(eqp.ID);
        }

        internal (bool result, string message) DeleteAllMachineRecipes(RMS_EQUIPMENT eqp)
        {
            var typeinstance = GetInstanceWithEuipmentId(eqp);
            if (typeinstance == null) return (false, "Recipe type do not support!");
            else return typeinstance.DeleteAllMachineRecipes(eqp.ID);
        }

        internal (bool result, string message) DownloadRecipeToMachine(RMS_EQUIPMENT eqp, string RecipeVersionId)
        {
            var typeinstance = GetInstanceWithEuipmentId(eqp);
            if (typeinstance == null) return (false, "Recipe type do not support!");
            else return typeinstance.DownloadRecipeToMachine(eqp.ID, RecipeVersionId);
        }

        internal (bool result, string message, byte[]? body) UploadRecipeToServer(RMS_EQUIPMENT eqp, string RecipeName)
        {
            var typeinstance = GetInstanceWithEuipmentId(eqp);
            if (typeinstance == null) return (false, "Recipe type do not support!", null);
            else return typeinstance.UploadRecipeToServer(eqp.ID, RecipeName);
        }

        internal (bool result, string message) CompareRecipe(RMS_EQUIPMENT eqp, string RecipeVersionId)
        {
            var typeinstance = GetInstanceWithEuipmentId(eqp);
            if (typeinstance == null) return (false, "Recipe type do not support!");
            else return typeinstance.CompareRecipe(eqp.ID, RecipeVersionId);
        }

        private IRecipeType? GetInstanceWithEuipmentId(RMS_EQUIPMENT eqp)
        {
            // 获取当前程序集
            // 获取当前程序集
            var assembly = Assembly.GetExecutingAssembly();

            // 获取所有符合条件的类型并实例化为字典
            var handlers = assembly.GetTypes()
                .Where(t => typeof(IRecipeType).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToDictionary(t => t.Name.ToLower(), t => t);

            // 根据类型名称获取类型
            if (handlers.TryGetValue(eqp.RECIPE_TYPE.ToLower(), out var handlerType))
            {
                // 构造函数参数
                object[] constructorArgs;
                Type[] argTypes;

                constructorArgs = new object[] { sqlSugarClient, rabbitMq };
                argTypes = new Type[] { typeof(ISqlSugarClient), typeof(RabbitMqService) };

                // 可以根据需要添加更多的类型和对应的构造函数参数

                // 获取构造函数
                var constructor = handlerType.GetConstructor(argTypes);
                if (constructor != null)
                {
                    // 实例化对象
                    var instance = constructor.Invoke(constructorArgs) as IRecipeType;
                    return instance;
                }
                else
                {
                    // 没有找到匹配的构造函数
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}

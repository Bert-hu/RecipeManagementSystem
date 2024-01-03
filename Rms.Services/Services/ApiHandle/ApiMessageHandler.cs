using Rms.Models.WebApi;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Rms.Services.Services.ApiHandle
{


    public partial class ApiMessageHandler
    {
        public ApiMessageHandler() { }

        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        bool IsDebugMode = ConfigurationManager.AppSettings["IsDebugMode"].ToUpper() == "TRUE";


        public ResponseMessage HandleMessage(string route, string jsoncontent)
        {
            var res = new ResponseMessage();


                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var handlers = assembly.GetTypes()
                        .Where(t => typeof(IMessageHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                        .ToDictionary(t => t.Name.ToLower(), t => Activator.CreateInstance(t) as IMessageHandler);

                    if (handlers.TryGetValue(route.Replace("/api/", "").ToLower(), out var handler))
                    {
                        res = handler.Handle(jsoncontent);
                    }
                    else
                    {
                        res.Result = false;
                        res.Message = "Unrecognized route";
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message,ex);
                    res.Result = false;
                    res.Message = ex.ToString();
                }


                //try
                //{
                //    switch (route.ToLower())
                //    {
                //        case "/api/getequipmentstatus":
                //            res = GetEquipmentStatus(jsoncontent);
                //            break;
                //        case "/api/geteppd":
                //            res = GetEPPD(jsoncontent);
                //            break;
                //        case "/api/addnewrecipe":
                //            res = AddNewRecipe(jsoncontent);
                //            break;
                //        case "/api/addnewrecipeversion":
                //            res = AddNewRecipeVersion(jsoncontent);
                //            break;
                //        case "/api/reloadrecipebody":
                //            res = ReloadRecipeBody(jsoncontent);
                //            break;
                //        case "/api/comparerecipebody":
                //            res = CompareRecipeBody(jsoncontent);
                //            break;
                //        case "/api/downloadeffectiverecipetomachine":
                //            res = DownloadEffectiveRecipeToMachine(jsoncontent);
                //            break;
                //        case "/api/downloadeffectiverecipebyrecipegroup":
                //            res = DownloadEffectiveRecipeByRecipeGroup(jsoncontent);
                //            break;
                //        case "/api/getmarkingtexts":
                //            res = GetMarkingTexts(jsoncontent);
                //            break;
                //        case "/api/checkrecipegroup":
                //            res = CheckRecipeGroup(jsoncontent);
                //            break;
                //        case "/api/deleteallrecipes":
                //            res = DeleteAllRecipes(jsoncontent);
                //            break;
                //        case "/api/editrecipebody":
                //            res = EditRecipeBody(jsoncontent);
                //            break;
                //        default:
                //            res.Result = false;
                //            res.Message = "Unrecognized route";
                //            break;
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Log.Error(ex);
                //    res.Result = false;
                //    res.Message = ex.ToString();
                //}

            return res;
        }


    }
}

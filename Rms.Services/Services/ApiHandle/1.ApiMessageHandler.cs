using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Services.ApiHandle
{


    public partial  class ApiMessageHandler
    {
        public ApiMessageHandler() { }

        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        bool isdebugmode = ConfigurationManager.AppSettings["IsDebugMode"].ToUpper() == "TRUE";


        public ResponseMessage HandleMessage(string route, string jsoncontent)
        {
            var res = new ResponseMessage();
            try
            {
                switch (route.ToLower())
                {
                    case "/api/getequipmentstatus":
                        res = GetEquipmentStatus(jsoncontent);
                        break;
                    case "/api/geteppd":
                        res = GetEPPD(jsoncontent);
                        break;
                    case "/api/addnewrecipe":
                        res = AddNewRecipe(jsoncontent);
                        break;
                    case "/api/addnewrecipeversion":
                        res = AddNewRecipeVersion(jsoncontent);
                        break;
                    case "/api/reloadrecipebody":
                        res = ReloadRecipeBody(jsoncontent);
                        break;
                    case "/api/comparerecipebody":
                        res = CompareRecipeBody(jsoncontent);
                        break;
                    case "/api/downloadeffectiverecipetomachine":
                        res = DownloadEffectiveRecipeToMachine(jsoncontent);
                        break;
                    case "/api/downloadeffectiverecipebyrecipegroup":
                        res = DownloadEffectiveRecipeByRecipeGroup(jsoncontent);
                        break;
                    case "/api/getmarkingtexts":
                        res = GetMarkingTexts(jsoncontent);
                        break;
                    default:
                        res.Result = false;
                        res.Message = "Unrecognized route";
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                res.Result = false;
                res.Message = ex.ToString();
            }
            return res;
        }

       
    }
}

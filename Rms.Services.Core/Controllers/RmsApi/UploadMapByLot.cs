using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Core.Services;
using Rms.Services.Core.Utils;
using Rms.Services.Utils;
using SqlSugar;
using System.Net;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        private class UploadMapReply
        {
            public bool Result { get; set; }
            public string? Message { get; set; }
        }
        [HttpPost]
        public JsonResult UploadMapByLot(UploadMapByLotRequest request)
        {
            var response = new UploadMapByLotResponse();
            try
            {
                var equipment = db.Queryable<RMS_EQUIPMENT>().In(request.EquipmentId).First();
                var equipmentType = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipment.TYPE).First();

                string sfisStep7Request = $"{request.EquipmentId},{request.LotId},7,M068397,JORDAN,,OK,MODEL_NAME=???  WAFER_IDS=???";
                string sfisStep7Response = string.Empty;
                string errorMessage = string.Empty;

                if (SendMessageToSfis(sfisStep7Request, ref sfisStep7Response, ref errorMessage))
                {
                    if (sfisStep7Response.ToUpper().StartsWith("OK"))
                    {
                        var sfisParameters = sfisStep7Response.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                            .Where(keyValueArray => keyValueArray.Length == 2)
                            .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);

                        string modelName = sfisParameters["MODEL_NAME"];
                        string[] waferIds = sfisParameters["WAFER_IDS"].Split(';');

                        string sftpIp = commonConfiguration.Configs["SftpIp"];
                        string sftpAoiUsername = commonConfiguration.Configs["SftpAoiUsername"];
                        string sftpAoiPassword = commonConfiguration.Configs["SftpAoiPassword"];
                        string productionMapPath = commonConfiguration.Configs["ProductionMapPath"];
                        string productionMapUsername = commonConfiguration.Configs["ProductionMapUsername"];
                        string productionMapPassword = commonConfiguration.Configs["ProductionMapPassword"];
                        string targetPath = $"Production{equipmentType.NAME}";
                        var targetDirectory = $"{productionMapPath.TrimEnd('\\').TrimEnd('/')}";
                        List<string> uploadedwaferids = new List<string>();
                        using (new NetworkConnection(targetDirectory, new NetworkCredential(productionMapUsername, productionMapPassword)))
                        {
                            targetDirectory = Path.Combine(targetDirectory, targetPath.TrimEnd('/'), request.EquipmentId);

                            //检查map是否齐全
                            foreach (var waferId in waferIds)
                            {
                                if (!System.IO.File.Exists(Path.Combine(targetDirectory, "Output_Temp", waferId.ToUpper())))
                                {
                                    response.Message = $"Lot:{request.LotId},Error: WaferID {waferId} can not find output map file.";
                                    return Json(response);
                                }
                            }

                            string waferMapUrl = commonConfiguration.Configs["WaferMapUrl"];
                            string uploadMapUrl = $"{waferMapUrl}/WaferMap/UploadWaferMap";

                            foreach (var waferid in waferIds)
                            {
                                string mapfilecontent = System.IO.File.ReadAllText(Path.Combine(targetDirectory, "Output_Temp", waferid.ToUpper()));
                                string uploadMapBody = JsonConvert.SerializeObject(new
                                {
                                    WaferId = waferid,
                                    Source = equipmentType.ID,
                                    Content = mapfilecontent
                                });
                                var uploadMapResult = HttpClientHelper.HttpPostRequestAsync4Json(uploadMapUrl, uploadMapBody);
                                if (uploadMapResult != null)
                                {
                                    var uploadMapReply = JsonConvert.DeserializeObject<UploadMapReply>(uploadMapResult);
                                    if (!uploadMapReply.Result)
                                    {
                                        response.Message = $"Lot:{request.LotId},WaferID:{waferid},upload wafer map FAIL:{uploadMapReply.Message}. Success waferids: {string.Join(",", uploadedwaferids)}";
                                        return Json(response)   ;
                                    }
                                    uploadedwaferids.Add(waferid);
                                }
                                else
                                {
                                    response.Message = $"Lot:{request.LotId},WaferID:{waferid},Call WaferMap Service Api FAIL. Success waferids: {string.Join(",", uploadedwaferids)}";
                                    return Json(response);
                                }
                            }
                        }

                        response.Result = true;
                        response.Message = $"Success waferids: {string.Join(",", uploadedwaferids)}";
                        return Json(response);
                    }
                    else
                    {
                        response.Message = $"Lot:{request.LotId},Sfis error:{sfisStep7Response}";
                        return Json(response);
                    }
                }
                else
                {
                    response.Message = $"Lot:{request.LotId},error:{errorMessage}";
                    return Json(response);
                }
            }
            catch (Exception ex)
            {
                // 记录异常日志
                response.Message = $"未处理的异常: {ex.Message}";
                return Json(response);
            }
        }

    }
}

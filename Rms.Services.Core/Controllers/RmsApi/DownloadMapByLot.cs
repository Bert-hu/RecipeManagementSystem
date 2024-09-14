using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Core.Services;
using Rms.Services.Core.Utils;
using Rms.Services.Utils;
using SqlSugar;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        private class WaferMapVM
        {
            //public string ID { get; set; }
            public string WaferMapId { get; set; }
            public string Content { get; set; }
            public bool IsEffectiveVersion { get; set; }
            public long VersionNumber { get; set; }
            public string Source { get; set; }
        }

        private class DownloadMapReply
        {
            public bool Result { get; set; }
            public string Message { get; set; }
            public WaferMapVM WaferMap { get; set; }
        }
        [HttpPost]
        public JsonResult DownloadMapByLot(DownloadMapByLotRequest request)
        {

            var response = new DownloadMapByLotResponse();
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

                        //string modelName = sfisParameters["MODEL_NAME"];
                        string[] waferIds = sfisParameters["WAFER_IDS"].Split(';');
                        string productionMapPath = commonConfiguration.Configs["ProductionMapPath"];
                        string productionMapUsername = commonConfiguration.Configs["ProductionMapUsername"];
                        string productionMapPassword = commonConfiguration.Configs["ProductionMapPassword"];
                        var targetDirectory = $"{productionMapPath.TrimEnd('\\').TrimEnd('/')}";
                        string targetPath = $"Production{equipmentType.NAME}";


                        #region New Method: WaferMap Service
                        using (new NetworkConnection(targetDirectory, new NetworkCredential(productionMapUsername, productionMapPassword)))
                        {
                            targetDirectory = Path.Combine(targetDirectory, targetPath.TrimEnd('/'), request.EquipmentId);
                            if (!Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory);
                            }
                            List<WaferMapVM> wafermaps = new List<WaferMapVM>();
                            string waferMapUrl = commonConfiguration.Configs["WaferMapUrl"];
                            string downloadMapUrl = $"{waferMapUrl}/WaferMap/GetEffectiveWaferMap";
                            foreach (var waferid in waferIds)
                            {
                                string downloadMapBody = JsonConvert.SerializeObject(new { WaferID = waferid, Role = equipmentType.ID });
                                //Http Post Request
                                var downloadMapResult = HttpClientHelper.HttpPostRequestAsync4Json(downloadMapUrl, downloadMapBody);
                                if (downloadMapResult != null)
                                {
                                    var downloadMapReply = JsonConvert.DeserializeObject<DownloadMapReply>(downloadMapResult);
                                    if (downloadMapReply.Result)
                                    {
                                        var waferMap = downloadMapReply.WaferMap;
                                        if (waferMap != null)
                                        {
                                            wafermaps.Add(waferMap);
                                        }
                                    }
                                    else
                                    {
                                        response.Message = $"Lot:{request.LotId},WaferID:{waferid},download wafer map FAIL:{downloadMapReply.Message}";
                                        return Json(response);
                                    }
                                }
                                else
                                {
                                    response.Message = $"Lot:{request.LotId},WaferID:{waferid},Call WaferMap Service Api FAIL";
                                    return Json(response);
                                }
                            }

                            foreach (var waferMap in wafermaps)
                            {
                                string mapContent = waferMap.Content;
                                string mapFileName = waferMap.WaferMapId;
                                string mapFilePath = Path.Combine(targetDirectory, mapFileName);
                                System.IO.File.WriteAllText(mapFilePath, mapContent);
                            }
                        }
                        #endregion


                        response.Result = true;
                        response.Message = string.Join(",", waferIds);
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
                Log.Error(ex.ToString());
                Log.Error(ex.Message, ex);
                response.Message = $"未处理的异常: {ex.Message}";
                return Json(response);
            }
        }


        private bool SendMessageToSfis(string message, ref string receiveMsg, ref string errMsg)
        {
            try
            {
                string sfisip = commonConfiguration.Configs["SfisIp"];
                int sfisport = int.Parse(commonConfiguration.Configs["SfisPort"]);
                IPAddress serverip = IPAddress.Parse(sfisip);
                IPEndPoint point = new IPEndPoint(serverip, sfisport);

                using (TcpClient tcpSync = new TcpClient())
                {
                    tcpSync.Connect(point);

                    if (tcpSync.Connected)
                    {
                        using (NetworkStream nStream = tcpSync.GetStream())
                        {
                            byte[] data = Encoding.UTF8.GetBytes(message);
                            nStream.Write(data, 0, data.Length);
                            nStream.Flush();

                            int len = 0;
                            int trytimes = 3;

                            while (len == 0 && trytimes > 0)
                            {
                                data = new byte[1024];
                                len = nStream.Read(data, 0, data.Length);
                                trytimes--;
                            }

                            if (len > 0)
                            {
                                receiveMsg = Encoding.UTF8.GetString(data, 0, len);
                                return true;
                            }
                            else
                            {
                                errMsg = "SFIS Timeout";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        errMsg = "Cannot connect to SFIS";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = $"EAP send message to SFIS failed. {ex.Message}";
                return false;
            }
        }
    }
}

using Newtonsoft.Json;
using Renci.SshNet;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Utils;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    internal class UploadMapByLot : CommonHandler
    {
        public override ResponseMessage Handle(string jsonContent)
        {
            var response = new UploadMapByLotResponse();
            try
            {
                var db = DbFactory.GetSqlSugarClient();
                var request = JsonConvert.DeserializeObject<UploadMapByLotRequest>(jsonContent);


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

                        string sftpIp = CommonConfiguration.Configs["SftpIp"];
                        string sftpAoiUsername = CommonConfiguration.Configs["SftpAoiUsername"];
                        string sftpAoiPassword = CommonConfiguration.Configs["SftpAoiPassword"];
                        string productionMapPath = CommonConfiguration.Configs["ProductionMapPath"];
                        string productionMapUsername = CommonConfiguration.Configs["ProductionMapUsername"];
                        string productionMapPassword = CommonConfiguration.Configs["ProductionMapPassword"];
                        string targetPath = $"Production{equipmentType.NAME}";
                        var targetDirectory = $"{productionMapPath.TrimEnd('\\').TrimEnd('/')}";
                        List<string> uploadedwaferids = new List<string>();
                        using (new NetworkConnection(targetDirectory, new NetworkCredential(productionMapUsername, productionMapPassword)))
                        {
                            targetDirectory = Path.Combine(targetDirectory, targetPath.TrimEnd('/'), request.EquipmentId);

                            //检查map是否齐全
                            foreach (var waferId in waferIds)
                            {
                                if (!File.Exists(Path.Combine(targetDirectory, "Output_Temp", waferId.ToUpper())))
                                {
                                    response.Message = $"Lot:{request.LotId},Error: WaferID {waferId} can not find output map file.";
                                    return response;
                                }
                            }

                            string waferMapUrl = CommonConfiguration.Configs["WaferMapUrl"];
                            string uploadMapUrl = $"{waferMapUrl}/WaferMap/UploadWaferMap";
                         
                            foreach (var waferid in waferIds)
                            {
                                string mapfilecontent = File.ReadAllText(Path.Combine(targetDirectory, "Output_Temp", waferid.ToUpper()));
                                string uploadMapBody = JsonConvert.SerializeObject(new
                                {
                                    WaferId = waferid,
                                    Source = equipmentType.ID,
                                    Content = mapfilecontent
                                });
                                var uploadMapResult = HTTPClientHelper.HttpPostRequestAsync4Json(uploadMapUrl, uploadMapBody);
                                if (uploadMapResult != null)
                                {
                                    var uploadMapReply = JsonConvert.DeserializeObject<UploadMapReply>(uploadMapResult);
                                    if (!uploadMapReply.Result)
                                    {
                                        response.Message = $"Lot:{request.LotId},WaferID:{waferid},upload wafer map FAIL:{uploadMapReply.Message}. Success waferids: {string.Join(",", uploadedwaferids)}";
                                        return response;
                                    }
                                    uploadedwaferids.Add(waferid);
                                }
                                else
                                {
                                    response.Message = $"Lot:{request.LotId},WaferID:{waferid},Call WaferMap Service Api FAIL. Success waferids: {string.Join(",", uploadedwaferids)}";
                                    return response;
                                }
                            }
                        }

                        response.Result = true;
                        response.Message = $"Success waferids: {string.Join(",", uploadedwaferids)}";
                        return response;
                    }
                    else
                    {
                        response.Message = $"Lot:{request.LotId},Sfis error:{sfisStep7Response}";
                        return response;
                    }
                }
                else
                {
                    response.Message = $"Lot:{request.LotId},error:{errorMessage}";
                    return response;
                }
            }
            catch (Exception ex)
            {
                // 记录异常日志
                response.Message = $"未处理的异常: {ex.Message}";
                return response;
            }
        }


        private bool SendMessageToSfis(string message, ref string receiveMsg, ref string errMsg)
        {
            try
            {
                string sfisip = CommonConfiguration.Configs["SfisIp"];
                int sfisport = int.Parse(CommonConfiguration.Configs["SfisPort"]);
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

        private bool PlaceMapFileToLocalPath(string equipmentid, string waferid, string sftpIp, string sourceUsername, string sourcePassword, string targetPath, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {

                string sfis_step3_req = $"{equipmentid},{waferid},3,M001603,JORDAN,,OK,";
                string sfis_step3_res = string.Empty;
                var targetDirectory = targetPath.TrimEnd('/') + "/" + equipmentid + "/";


                if (SendMessageToSfis(sfis_step3_req, ref sfis_step3_res, ref errMsg))
                {
                    if (sfis_step3_res.ToUpper().StartsWith("OK"))
                    {
                        Dictionary<string, string> sfispara = sfis_step3_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                            .Where(keyValueArray => keyValueArray.Length == 2)
                       .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                        var foldname = sfispara["FOLDER_NAME"].TrimEnd('/') + "/";
                        var filename = sfispara["FILE_NAME"];
                        var sourceFilePath = foldname + filename;
                        var targetFilePath = targetDirectory + waferid.ToUpper() + ".SINF";

                        using (var sourceClient = new SftpClient(sftpIp, sourceUsername, sourcePassword))
                        {
                            sourceClient.Connect();

                            using (var fileStream = System.IO.File.OpenWrite(targetFilePath))
                            {
                                sourceClient.DownloadFile(sourceFilePath, fileStream);
                            }
                            sourceClient.Disconnect();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errMsg = $"Copy Map File Fail.{ex.Message}";
                return false;
            }

        }


        private class UploadMapReply
        {
            public bool Result { get; set; }
            public string Message { get; set; }
        }
    }
}

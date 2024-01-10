using Newtonsoft.Json;
using Renci.SshNet;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
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
    internal class DownloadMapByLot : CommonHandler
    {
        public override ResponseMessage Handle(string jsonContent)
        {
            var response = new DownloadMapByLotResponse();
            try
            {
                var db = DbFactory.GetSqlSugarClient();
                var request = JsonConvert.DeserializeObject<DownloadMapByLotRequest>(jsonContent);
                

                var equipment = db.Queryable<RMS_EQUIPMENT>().In(request.EquipmentId).First();
                var equipmentType = db.Queryable<RMS_EQUIPMENT_TYPE>().In(equipment.TYPE).First();

                string sfisStep7Request = $"{request.EquipmentId},{request.LottId},7,M068397,JORDAN,,OK,MODEL_NAME=???  WAFER_IDS=???";
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
                        string sourceUsername = CommonConfiguration.Configs["SftpSourceUsername"];
                        string sourcePassword = CommonConfiguration.Configs["SftpSourcePassword"];
                        string productionMapPath = CommonConfiguration.Configs["ProductionMapPath"];
                        string productionMapUsername = CommonConfiguration.Configs["ProductionMapUsername"];
                        string productionMapPassword = CommonConfiguration.Configs["ProductionMapPassword"];
                        string targetPath = $"\\Production{equipmentType.NAME}";

                        var targetDirectory = $"{productionMapPath.TrimEnd('\\').TrimEnd('/')}";

                        using (new NetworkConnection(targetDirectory, new NetworkCredential(productionMapUsername, productionMapPassword)))
                        {
                            targetDirectory = Path.Combine(targetDirectory, targetPath.TrimEnd('/'), request.EquipmentId);

                            if (!Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory);
                            }

                            string[] files = Directory.GetFiles(targetDirectory);
                            files.ToList().ForEach(file => System.IO.File.Delete(file));

                            using (var sourceClient = new SftpClient(sftpIp, sourceUsername, sourcePassword))
                            {
                                sourceClient.Connect();

                                foreach (var waferId in waferIds)
                                {
                                    string sfisStep3Request = $"{request.EquipmentId},{waferId},3,M001603,JORDAN,,OK,";
                                    string sfisStep3Response = string.Empty;

                                    if (SendMessageToSfis(sfisStep3Request, ref sfisStep3Response, ref errorMessage))
                                    {
                                        if (sfisStep3Response.ToUpper().StartsWith("OK"))
                                        {
                                            var sfisParameters1 = sfisStep3Response.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                                                .Where(keyValueArray => keyValueArray.Length == 2)
                                                .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);

                                            var folderName = sfisParameters1["FOLDER_NAME"].TrimEnd('/') + "/";
                                            var fileName = sfisParameters1["FILE_NAME"];
                                            var sourceFilePath = Path.Combine(folderName, fileName);
                                            var targetFilePath = Path.Combine(targetDirectory, waferId.ToUpper() + ".SINF");

                                            using (var fileStream = System.IO.File.OpenWrite(targetFilePath))
                                            {
                                                sourceClient.DownloadFile(sourceFilePath, fileStream);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        response.Message = $"Lot:{request.LottId},Sfis error:{sfisStep7Response}";
                                        return response;
                                    }
                                }

                                sourceClient.Disconnect();
                            }
                        }

                        response.Result = true;
                        return response;
                    }
                    else
                    {
                        response.Message = $"Lot:{request.LottId},Sfis error:{sfisStep7Response}";
                        return response;
                    }
                }
                else
                {
                    response.Message = $"Lot:{request.LottId},error:{errorMessage}";
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
    }
}

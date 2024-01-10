using Microsoft.Ajax.Utilities;
using Renci.SshNet;
using Rms.Models.DataBase.Rms;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Rms.Web.Controllers.Production
{
    public class WaferAOIController : CommonProductionController
    {

        // GET: WaferGrinding
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetEquipments()
        {
            var db = DbFactory.GetSqlSugarClient();
            var data = db.Queryable<RMS_EQUIPMENT>().Where(it => it.TYPE == "WaferAOI").ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DownloadMapByLot(string eqid, string lotid)
        {
            try
            {
                string sfis_step7_req = $"{eqid},{lotid},7,M068397,JORDAN,,OK,MODEL_NAME=???  WAFER_IDS=???";
                string sfis_step7_res = string.Empty;
                string errmsg = string.Empty;
                if (SendMessageToSfis(sfis_step7_req, ref sfis_step7_res, ref errmsg))
                {
                    if (sfis_step7_res.ToUpper().StartsWith("OK"))
                    {

                        Dictionary<string, string> sfispara = sfis_step7_res.Split(',')[1].Split(' ').Select(keyValueString => keyValueString.Split('='))
                        .Where(keyValueArray => keyValueArray.Length == 2)
                   .ToDictionary(keyValueArray => keyValueArray[0], keyValueArray => keyValueArray[1]);
                        string rcpgroupname = sfispara["MODEL_NAME"];
                        string[] waferids = sfispara["WAFER_IDS"].Split(';');

                        string sftpIp = ConfigurationManager.AppSettings["SftpIp"].ToString();
                        string sourceUsername = ConfigurationManager.AppSettings["SftpSourceUsername"].ToString();
                        string sourcePassword = ConfigurationManager.AppSettings["SftpSourcePassword"].ToString();
                        string targetUsername = ConfigurationManager.AppSettings["SftpTargetUsername"].ToString();
                        string targetPassword = ConfigurationManager.AppSettings["SftpTargetPassword"].ToString();
                        string targetPath = "/ProductionDPSAOI";
                        //Sftp清空文件夹
                        //ClearFilePath(eqid, sftpIp, targetUsername, targetPassword, targetpath);
                        //本地路径清空文件夹
                        var targetDirectory = "/ProductionMap" + targetPath.TrimEnd('/') + "/" + eqid + "/";
                        if (!Directory.Exists(targetDirectory))
                        {
                            Directory.CreateDirectory(targetDirectory);
                        }
                        string[] files = Directory.GetFiles(targetDirectory);
                        files.ForEach(it => System.IO.File.Delete(it));

                        foreach (var waferid in waferids)
                        {
                            //PlaceMapFileToSftpPath(eqid, waferid, sftpIp, sourceUsername, sourcePassword, targetUsername, targetPassword, targetPath, out string errMsg);
                            if (!PlaceMapFileToLocalPath(eqid, waferid, sftpIp, sourceUsername, sourcePassword, "/ProductionMap" + targetPath, out string errMsg))
                            {
                                return Json(new { Result = false, Message = errMsg });
                            }
                        }


                        AddProductionLog(eqid, "DownloadMap", $"{true}", $"Lot:{lotid}");

                        return Json(new { Result = true });
                    }
                    else
                    {
                        AddProductionLog(eqid, "DownloadMap", "False", $"Lot:{lotid},Sfis error:{sfis_step7_res}");
                        return Json(new { Result = false, Message = $"Sfis error:{sfis_step7_res}" });
                    }
                }
                else
                {
                    AddProductionLog(eqid, "DownloadMap", "False", $"Lot:{lotid},error:{errmsg}");
                    return Json(new { Result = false, Message = $"{errmsg}" });
                }
            }
            catch (Exception ex)
            {
                AddProductionLog(eqid, "DownloadMap", "False", $"Lot:{lotid},error:{ex.Message}");
                return Json(new { Result = false, Message = $"{ex.Message}" });
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
                errMsg = $"EAP Copy Map File Fail.{ex.Message}";
                return false;
            }

        }

    }
}
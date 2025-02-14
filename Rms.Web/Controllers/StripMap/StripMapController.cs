using Antlr.Runtime.Misc;
using Microsoft.Ajax.Utilities;
using Rms.Models.DataBase.Ams;
using Rms.Models.DataBase.Map;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Utils;
using Rms.Web.Extensions;
using Rms.Web.ViewModels;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Xml.Linq;

namespace Rms.Web.Controllers.StripMap
{
    public class StripMapController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult UploadMap()
        {
            try
            {


                //Code First 生成表
                //Assembly wrsmodels = Assembly.Load("Rms.Models");
                //var typesInNamespace = wrsmodels.GetTypes()
                //   .Where(t => t.Namespace != null && t.Namespace.StartsWith("Rms.Models.DataBase.Map"))
                //   .ToList();

                //foreach (var type in typesInNamespace)
                //{
                //    sqlSugarClient.CodeFirst.InitTables(type);
                //}


                var file = Request.Files[0];
                //获取文件内容字符串
                string fileContent = "";
                using (var stream = file.InputStream)
                {
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }

                // 加载XML文件
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(fileContent);

                // 获取PCB_DESCRIPTION元素
                XmlNode pcbDescription = doc.SelectSingleNode("/PCB_DATA/PCB_DESCRIPTION");

                // 获取行数、列数和单电路数（虽然这里我们实际上不需要单电路数）
                int noRows = int.Parse(pcbDescription.Attributes["NO_ROWS"].Value);
                int noColumns = int.Parse(pcbDescription.Attributes["NO_COLUMNS"].Value);

                var data = new int[noRows * noColumns][];

                var items = doc.SelectNodes("/PCB_DATA/PCB_DESCRIPTION/SC");
                int index = 0;
                foreach (XmlNode item in items)
                {
                    int row = int.Parse(item.Attributes["R"].Value);
                    int column = int.Parse(item.Attributes["C"].Value);
                    int state = int.Parse(item.Attributes["S"].Value);
                    data[index] = new int[] { column, row, state };
                    index++;
                }

                var map = new MAP_EDITMAP()
                {
                    NAME = file.FileName,
                    ORIGINAL_CONTENT = data,
                    CONTENT = data,
                    CREATOR = User.TRUENAME,
                    CREATE_TIME = DateTime.Now,
                    LASTEDITOR = User.TRUENAME,
                    LASTEDIT_TIME = DateTime.Now
                };
                db.Insertable<MAP_EDITMAP>(map).ExecuteCommand();



                return Json(new { result = true, message = "", map = map });
            }
            catch (Exception ex)
            {

            }
            return Json(new { result = false, message = "" });
        }

        public JsonResult GetMaps(int page, int limit)
        {
            int totalNum = 0;
            var data = db.Queryable<MAP_EDITMAP>().OrderByDescending(it => it.CREATE_TIME).ToPageList(page, limit, ref totalNum);
            return Json(new { data = data, code = 0, count = totalNum }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMapData(string ID)
        {
            var map = db.Queryable<MAP_EDITMAP>().Where(it => it.ID == ID).First();
            return Json(new { map = map, code = 0 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadUpdatedMap(int[][] data,string name)
        {
            int[][] intData = new int[data.Length][];
            for (int i = 0; i < data.Length; i++)
            {
                intData[i] = new int[3];
                intData[i][0] = data[i][0];
                intData[i][1] = data[i][1];
                intData[i][2] = data[i][2] % 100;
            }


            var rowNum = intData.Select(it => it[1]).Max(it => it);
            var colNum = intData.Select(it => it[0]).Max(it => it);
            StringBuilder mapFileString = new StringBuilder();
            mapFileString.AppendLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
            mapFileString.AppendLine("<PCB_DATA>");
            mapFileString.AppendLine($"<PCB_DESCRIPTION NO_SINGLE_CIRCUITS=\"{intData.Count()}\" NO_ROWS=\"{rowNum}\" NO_COLUMNS=\"{colNum}\">");
            intData.ForEach(item => mapFileString.AppendLine($"<SC R=\"{item[1]}\" C=\"{item[0]}\" S=\"{item[2]}\"/>"));
            mapFileString.AppendLine("</PCB_DESCRIPTION>");
            mapFileString.AppendLine("</PCB_DATA>");

            byte[] fileBytes = Encoding.UTF8.GetBytes(mapFileString.ToString());
            return File(fileBytes, "text/plain", name);

        }

        public JsonResult SaveMapData(string ID, int[][] data)
        {
            var map = db.Queryable<MAP_EDITMAP>().Where(it => it.ID == ID).First();
            map.CONTENT = data;
            map.LASTEDITOR = User.TRUENAME;
            map.LASTEDIT_TIME = DateTime.Now;
            db.Updateable<MAP_EDITMAP>(map).UpdateColumns(it => new { it.CONTENT, it.LASTEDITOR, it.LASTEDIT_TIME }).ExecuteCommand();
            return Json(new { result = true, message = "保存成功" });
        }
    }
}
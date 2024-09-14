using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using SqlSugar;
using System.Globalization;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        [HttpPost]
        public JsonResult GetMarkingTexts(GetMarkingTextsRequest req)
        {
            var res = new GetMarkingTextsResponse();

            #region 初始化所有的codevalue
            var codevalues = req.SfisParameter;
            //DATECODE
            codevalues.Add("DATECODE", DateTime.Now.ToString("yyyyMMdd"));
            //WEEKCODE
            DateTime currentDate = DateTime.Now;
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Calendar calendar = cultureInfo.Calendar;
            int weekNumber = calendar.GetWeekOfYear(currentDate, cultureInfo.DateTimeFormat.CalendarWeekRule, cultureInfo.DateTimeFormat.FirstDayOfWeek);
            codevalues.Add("WEEKCODE", weekNumber.ToString("D2"));
            #endregion

            try
            {
                var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == req.EquipmentId && it.NAME == req.RecipeName).First();
                if (recipe == null)
                {
                    res.Result = false;
                    res.Message = $"{req.EquipmentId} can not find recipe '{req.RecipeName}' in RMS";
                    return Json(res);
                }
                var data = db.Queryable<RMS_MARKING_CONFIG>().Where(it => it.MARKING_VERSION_ID == recipe.MARKING_EFFECTIVE_ID).OrderBy(it => new { it.TEXTINDEX, it.TEXTORDER }).ToList();
                var markingindexs = data.Select(it => it.TEXTINDEX).Distinct().OrderBy(it => it).ToList();
                string[] markingtexts = new string[10];
                markingindexs.ForEach(it =>//遍历每一行内容
                {
                    var rowconfigs = data.Where(o => o.TEXTINDEX == it).OrderBy(o => o.TEXTORDER).ToList();
                    string rowtext = string.Empty;
                    rowconfigs.ForEach(o =>//遍历每一行的配置内容并拼接
                    {
                        if (o.TYPE == "Fixed")
                        {
                            rowtext += o.CONTENT;
                        }
                        else if (o.TYPE == "Code")
                        {
                            string codevalue;
                            if (codevalues.TryGetValue(o.CONTENT, out codevalue))
                            {
                                if (o.START_INDEX + o.LENGTH > codevalue.Length)
                                {
                                    throw new Exception($"Config error: Field:{o.CONTENT}, Value '{codevalue}', StartIndex: {o.START_INDEX}, Length: {o.LENGTH}.");
                                }
                                else
                                {
                                    rowtext += codevalue.Substring(o.START_INDEX, o.LENGTH);
                                }
                            }
                            else
                            {
                                throw new Exception($"Can not get the value of code '{o.CONTENT}'");
                            }

                        }

                    });
                    markingtexts[it - 1] = rowtext;
                });

                res.MarkingTexts = markingtexts;
                res.Result = true;
                return Json(res);
            }
            catch (Exception ex)
            {
                res.Result = false;
                res.Message = ex.Message;
                return Json(res);
            }
        }

    }
}

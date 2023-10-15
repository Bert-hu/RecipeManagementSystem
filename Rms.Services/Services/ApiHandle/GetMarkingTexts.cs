using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;
using RMS.Domain.Rms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rms.Services.Services.ApiHandle
{
    public partial class ApiMessageHandler
    {
        /// <summary>
        /// 新建Recipe
        /// </summary>
        /// <param name="jsoncontent"></param>
        /// <returns></returns>
        public ResponseMessage GetMarkingTexts(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new GetMarkingTextsResponse();
            var req = JsonConvert.DeserializeObject<GetMarkingTextsRequest>(jsoncontent);

            #region 初始化所有的codevalue
            var codevalues = req.SfisParameter;
            codevalues.Add("DATECODE", DateTime.Now.ToString("yyyyMMdd"));
            #endregion

            try
            {
                var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == req.EquipmentId && it.NAME == req.RecipeName).First();
                if (recipe == null)
                {
                    res.Result = false;
                    res.Message = $"{req.EquipmentId} can not find recipe '{req.RecipeName}' in RMS";
                    return res;
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
                    markingtexts[it-1]= rowtext;
                });

                res.MarkingTexts = markingtexts;
                res.Result = true;
                return res;
            }
            catch (Exception ex)
            {
                res.Result = false;
                res.Message = ex.Message;
                return res;
            }





        }

    }
}

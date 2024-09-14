using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using Secs4Net.Sml;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rms.Services.Services.ApiHandle
{
    public partial class ApiMessageHandler
    {
        public ResponseMessage EditRecipeBody(string jsonContent)
        {
            var res = new AddNewRecipeVersionWithBodyResponse();
            var req = JsonConvert.DeserializeObject<AddNewRecipeVersionWithBodyRequest>(jsonContent);
            if (!ValidateSmlFormat(req.RecipeBody))
            {
                res.Message = "Recipe body format error";
                return res;
            }

            var db = DbFactory.GetSqlSugarClient();
            var recipeversion = db.Queryable<RMS_RECIPE_VERSION>().In(req.RecipeVersionId).First();
            var recipe = db.Queryable<RMS_RECIPE>().In(recipeversion.RECIPE_ID).First();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();
            var eqType = db.Queryable<RMS_EQUIPMENT_TYPE>().In(eqp.TYPE).First();
            if (eqp.RECIPE_TYPE != "secsSml")
            {
                res.Message = "Only Recipe Type is 'secsSml' can add a new version with body!";
                return res;
            }
            if (recipe.VERSION_LATEST_ID != req.RecipeVersionId || recipeversion.CURRENT_FLOW_INDEX != -1)
            {
                res.Message = "Only the latest version that has not been submitted can be edited.";
                return res;
            }

            db.BeginTran();
            try
            {
                var body = System.Text.Encoding.Unicode.GetBytes(req.RecipeBody);
                if (!string.IsNullOrEmpty(recipeversion.RECIPE_DATA_ID))
                {
                    db.Deleteable<RMS_RECIPE_DATA>().In(recipeversion.RECIPE_DATA_ID).ExecuteCommand();
                }
                var data = new RMS_RECIPE_DATA
                {
                    NAME = recipe.NAME,
                    CONTENT = body,
                    CREATOR = req.TrueName,
                };
                db.Insertable<RMS_RECIPE_DATA>(data).ExecuteCommand();
                recipeversion.RECIPE_DATA_ID = data.ID;
                db.Updateable<RMS_RECIPE_VERSION>(recipeversion).UpdateColumns(it => new { it.RECIPE_DATA_ID }).ExecuteCommand();
            }
            catch
            {
                db.RollbackTran();
                throw;
            }

            db.CommitTran();
            res.Result = true;
            return res;
        }

        private bool ValidateSmlFormat(string smlbody)
        {
            try
            {
                var sr = new StringReader(smlbody);
                using (TextReader reader1 = RemoveEmptyLines(sr))
                {
                    using (TextReader reader2 = AddSingleQuotesToSxFy(reader1))
                    {
                        var msg = reader2.ToSecsMessage();
                        if (msg.SecsItem != null) return true;
                        Log.Warn($"Invalid sml message: {smlbody}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"Invalid sml message: {smlbody}");
                Log.Error(ex.Message, ex);
                return false;
            }
        }

        private TextReader RemoveEmptyLines(TextReader reader)
        {
            return new StringReader(
                string.Join(Environment.NewLine,
                    reader.ReadToEnd()
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                )
            );
        }

        private TextReader AddSingleQuotesToSxFy(TextReader inputReader)
        {
            StringBuilder outputText = new StringBuilder();
            string line;
            while ((line = inputReader.ReadLine()) != null)
            {
                int colonIndex = line.IndexOf(':');
                if (colonIndex != -1)
                {
                    string beforeColon = line.Substring(0, colonIndex + 1);  // 包括冒号
                    string afterColon = line.Substring(colonIndex + 1).Trim();  // 去掉冒号后的空格

                    if (afterColon.StartsWith("S"))
                    {
                        // 如果冒号后面以S开头，则在SxFy后添加单引号
                        int spaceIndex = afterColon.IndexOf(' ');
                        if (spaceIndex != -1)
                        {
                            string sxFyPart = afterColon.Substring(0, spaceIndex);
                            string restPart = afterColon.Substring(spaceIndex);
                            line = beforeColon + "'" + sxFyPart + "'" + restPart;
                        }
                        else
                        {
                            line = beforeColon + "'" + afterColon + "'";
                        }
                    }
                }
                else
                {
                    string pattern = @"S\d+F\d+"; // 使用正则表达式匹配SxFy格式，其中x和y是任意数字
                    line = Regex.Replace(line, pattern, "'$0'"); // 将匹配到的内容加上单引号
                }
                outputText.AppendLine(line);
            }

            return new StringReader(outputText.ToString());
        }
    }
}

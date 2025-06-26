using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    public class RMS_PARAMETER_SCOPE
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; }
        public string SOURCE { get; set; }
        public double LCL { get; set; }
        public double UCL { get; set; }
        [SugarColumn(ColumnName = "ENUMVALUE")]
        public string EnumValue { get; set; }

        [SugarColumn(ColumnName = "RECIPE_ID")]
        public string RecipeID { get; set; }
        [SugarColumn(ColumnName = "PARAMETER_KEY")]
        public string ParamKey { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string PARAMETER_NAME { get; set; }

        [SugarColumn(ColumnName = "TYPE")]
        public string Type { get; set; }
        [SugarColumn(ColumnName = "LAST_EDITOR")]
        public string LastEditor { get; set; }
        [SugarColumn(ColumnName = "LAST_EDIT_TIME")]
        public DateTime LastEditTime { get; set; }


        public string CompareValue(object machineValue, object serverValue)
        {
            string result = string.Empty;

            if (machineValue == null)
                return $"{PARAMETER_NAME} 机器值为空";

            try
            {
                switch (Type?.ToLowerInvariant())
                {
                    case "value":
                        double machineVal = Convert.ToDouble(machineValue);
                        if (machineVal > UCL)
                            result = $"{PARAMETER_NAME} {machineVal} > {UCL}";
                        else if (machineVal < LCL)
                            result = $"{PARAMETER_NAME} {machineVal} < {LCL}";
                        break;

                    case "floatvalue":
                        if (serverValue == null)
                            return $"{PARAMETER_NAME} 服务器值为空";

                        double machineFloat = Convert.ToDouble(machineValue);
                        double serverFloat = Convert.ToDouble(serverValue);
                        double difference = machineFloat - serverFloat;

                        if (difference > UCL)
                            result = $"{PARAMETER_NAME} {machineFloat} - {serverFloat} = {difference:F2} > {UCL}";
                        else if (difference < LCL)
                            result = $"{PARAMETER_NAME} {machineFloat} - {serverFloat} = {difference:F2} < {LCL}";
                        break;

                    case "percent":
                        if (serverValue == null)
                            return $"{PARAMETER_NAME} 服务器值为空";

                        double machinePercent = Convert.ToDouble(machineValue);
                        double serverPercent = Convert.ToDouble(serverValue);
                        double percentDiff = ((machinePercent / serverPercent) - 1) * 100;

                        if (percentDiff > UCL)
                            result = $"{PARAMETER_NAME} 百分比差异: {percentDiff:F2}% > {UCL}%";
                        else if (percentDiff < LCL)
                            result = $"{PARAMETER_NAME} 百分比差异: {percentDiff:F2}% < {LCL}%";
                        break;

                    case "enum":
                        string machineEnum = machineValue.ToString();
                        string[] values = EnumValue.Split('/');
                        if (!values.Any(it => it.Equals(machineEnum)))
                            result = $"{PARAMETER_NAME} {machineEnum} 不在允许值范围内: {EnumValue}";
                        break;

                    default:
                        result = $"不支持的参数类型: {Type}";
                        break;
                }
            }
            catch (FormatException ex)
            {
                result = $"{PARAMETER_NAME} 值格式错误: {ex.Message}";
            }
            catch (Exception ex)
            {
                result = $"{PARAMETER_NAME} 比较时发生错误: {ex.Message}";
            }

            return result;
        }

    }
}

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
    }
}

using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Map
{
    public class MAP_EDITMAP
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");

        public string NAME { get; set; }

        [SugarColumn(IsJson = true, ColumnDataType = "CLOB")]
        public int[][] ORIGINAL_CONTENT { get; set; }

        [SugarColumn(IsJson = true, ColumnDataType = "CLOB")]
        public int[][] CONTENT { get; set; }

        [SugarColumn(IsNullable = true)]
        public string CREATOR { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? CREATE_TIME { get; set; }

        [SugarColumn(IsNullable = true)]
        public string LASTEDITOR { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? LASTEDIT_TIME { get; set; }
    }
}
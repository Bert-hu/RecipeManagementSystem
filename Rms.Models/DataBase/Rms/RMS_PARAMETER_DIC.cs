using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    public class RMS_PARAMETER_DIC
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        [SugarColumn(ColumnName = "KEY")]
        public string Key { get; set; }
        [SugarColumn(ColumnName = "KEY_NAME")]
        public string Name { get; set; }
        [SugarColumn(ColumnName = "EQ_TYPE_ID")]
        public string EQ_TYPE_ID { get; set; }
        public string SOURCE { get; set; }
    }
}

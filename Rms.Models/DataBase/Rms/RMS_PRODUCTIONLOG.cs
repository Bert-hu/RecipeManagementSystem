using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    [SugarTable("RMS_PRODUCTIONLOG")]
    public class RMS_PRODUCTIONLOG
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime? CREATE_TIME { get; set; }
        public string EQUIPMENT_ID { get; set; }
        public string ACTION { get; set; }
        public string RESULT { get; set; }
        public string MESSAGE { get; set; }
    }
}

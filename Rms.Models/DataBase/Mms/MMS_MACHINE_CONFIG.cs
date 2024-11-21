using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Mms
{
    public class MMS_MACHINE_CONFIG
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string MMDID { get; set; }
        public string EQUIPMENT_ID { get; set; }
        public string VALUE { get; set; }
        public string LASTEDITOR { get; set; } = "Default";
        public DateTime? LASTEDITTIME { get; set; } = DateTime.Now;


    }
}

using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Mms
{
    public class MMS_MACHINE_CONFIG_HIST
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string MMCID { get; set; }
        public string EQUIPMENT_ID { get; set; }
        public string SHOWNAME { get; set; }
        public string OLDVALUE { get; set; }
        public string NEWVALUE { get; set; }
        public string EDITOR { get; set; } = "Default";
        public DateTime? TIME { get; set; } = DateTime.Now;


    }
}

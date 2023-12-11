using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{

    public class RMS_EQUIPMENT_TYPE
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; }
        public string NAME { get; set; } 
        public string PROCESS { get; set; }
        public int ORDERSORT { get; set; }
        public string VENDOR { get; set; }
        public string TYPE { get; set; }
        [SugarColumn(IsJson = true)]
        public List<string> ROLEIDS { get; set; }
        [SugarColumn(IsJson = true)]
        public List<string> FLOWROLEIDS { get; set; }
    }
}

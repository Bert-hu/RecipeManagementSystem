using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Ams
{
    public class AMS_ACTION_RECORD
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string CONFIG_ID { get; set; }
        public string EQID { get; set; }
        public string ETID { get; set; }
        public string ALID { get; set; }
        public int TRIGGER_INTERVAL { get; set; }
        public int TRIGGER_COUNT { get; set; }

        public DateTime DATETIME { get; set; }
        public bool ISHANDLED { get; set; } = false;
        public DateTime? HANDLETIME { get; set; }
        public string USERNAME { get; set; }
        public string REMARK { get; set; }

        //20240911 EQ需要多人确认后才能把报警消除
        public int CONFIRMTIMES { get; set; } = 0;

        [SugarColumn(IsJson = true)]

        public List<string> CONFIRM_USERS { get; set; } = new List<string>();
    }
}

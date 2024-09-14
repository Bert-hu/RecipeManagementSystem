using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Ams
{
    public class AMS_ALARM_RECORD
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string ALID { get; set; }
        public string ALTX { get; set; }
        public DateTime ALTIME { get; set; }
        public bool ALARM_TRIGGERED { get; set; } = false;
        public string  EQID { get; set; }
        //public string ETID { get; set; }
    }
}

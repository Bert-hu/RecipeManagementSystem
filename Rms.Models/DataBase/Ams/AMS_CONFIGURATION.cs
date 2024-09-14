using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Ams
{
    public class AMS_CONFIGURATION
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string NAME { get; set; }
        public string EQUIPMENT_TYPE_ID { get; set; }
        public string ALID { get; set; }
        public int TRIGGER_INTERVAL { get; set; }
        public int TRIGGER_COUNT { get; set; }
        public string ACTIONID { get; set; }
        public bool ISVALID { get; set; }
        public string ACTION_PARAMETER { get; set; }
    }
}

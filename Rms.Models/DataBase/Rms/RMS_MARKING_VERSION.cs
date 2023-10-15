using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    public class RMS_MARKING_VERSION
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");

        public string RECIPE_ID { get; set; }
        public string FLOW_ID { get; set; }
        public decimal? VERSION { get; set; } = 1;
        [SugarColumn(IsJson = true)]
        public List<string> FLOW_ROLES { get; set; } 
        public int CURRENT_FLOW_INDEX { get; set; }
        //public string REMARK { get; set; }
        public string CREATOR { get; set; } = "Default";
        public DateTime? CREATE_TIME { get; set; } = DateTime.Now;
    }
}

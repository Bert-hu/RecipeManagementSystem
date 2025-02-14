using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    public enum FlowAction
    {
        Submit = 0,
        Approve = 1,
        Reject = 2
    }
    [SugarTable("RMS_FLOW_HIST")]
    public class RMS_FLOW_HIST
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string RECIPE_VERSION_ID { get; set; }
        //public string FLOW_ID { get; set; }
        public int FLOW_INDEX { get; set; }

        public string CREATOR { get; set; }
        [SugarColumn(IsNullable = true)]
        public DateTime CREATE_TIME { get; set; }
        [SugarColumn(IsNullable = true)]
        public string REMARK { get; set; }

        public FlowAction ACTION { get; set; }
    }
}

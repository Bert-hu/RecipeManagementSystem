using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    [SugarTable("RMS_FLOW")]
    public class RMS_FLOW
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string FLOW_ROLES { get; set; } = "[]";
        public string NAME { get; set; }



        [SugarColumn(IsIgnore = true)]
        public List<string> _FLOW_ROLES
        {
            get { return JsonConvert.DeserializeObject<List<string>>(FLOW_ROLES); }
            set { FLOW_ROLES = JsonConvert.SerializeObject(value); }
        }
    }
}

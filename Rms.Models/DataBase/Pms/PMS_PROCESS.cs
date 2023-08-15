using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Pms
{
    [SugarTable("PMS_PROCESS")]
    public class PMS_PROCESS
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int ID { get; set; }

        public string NAME { get; set; }
        public string DESC { get; set; }

        public string ROLES_SUBMIT { get; set; }

        public string ROLES_NOTIFY { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<string> _ROLES_SUBMIT
        {
            get { return JsonConvert.DeserializeObject<List<string>>(ROLES_SUBMIT); }
            set { ROLES_SUBMIT = JsonConvert.SerializeObject(value); }
        }

        [SugarColumn(IsIgnore = true)]
        public List<string> _ROLES_NOTIFY
        {
            get { return JsonConvert.DeserializeObject<List<string>>(ROLES_NOTIFY); }
            set { ROLES_NOTIFY = JsonConvert.SerializeObject(value); }
        }

    }
}

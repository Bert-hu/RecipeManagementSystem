using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Mms
{
    public class MMS_MATERIAL_DIC
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string EQUIPMENT_TYPE_ID { get; set; }
        public string TYPE { get; set; }//Material或者Tooling
        public string MATERIAL_TYPE { get; set; }
        public int ORDER_SORT { get; set; }
        public string SHOWNAME { get; set; }

    }
}

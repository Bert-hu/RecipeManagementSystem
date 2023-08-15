using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Pms
{
    [SugarTable("PMS_ROLE")]
    public class PMS_ROLE
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; }

        public string NAME { get; set; }
        public string DESCRIPTION { get; set; }

        public int INUSED { get; set; }

        public int ORDERSORT { get; set; }

    }
}

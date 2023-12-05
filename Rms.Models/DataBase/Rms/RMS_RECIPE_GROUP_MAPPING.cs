using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    [SugarTable("RMS_RECIPE_GROUP_MAPPING")]
    public class RMS_RECIPE_GROUP_MAPPING
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string RECIPE_ID { get; set; }
        public string RECIPE_GROUP_ID { get; set; }
    }
}

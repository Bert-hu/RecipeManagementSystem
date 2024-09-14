using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    [SugarTable("RMS_RECIPE_NAME_ALIAS")]
    public class RMS_RECIPE_NAME_ALIAS
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string EQUIPMENT_TYPE_ID { get; set; }
        [SugarColumn(IsJson = true)]
        public List<string> RECIPE_ALIAS { get; set; }//一个recipe name可以对应多个recipe alias
        public string RECIPE_NAME { get; set; }
    }
}

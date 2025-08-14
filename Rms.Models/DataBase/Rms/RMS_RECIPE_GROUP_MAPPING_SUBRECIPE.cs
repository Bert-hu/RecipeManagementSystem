using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    [SugarTable("RMS_RG_MAPPING_SUB")]//太长了Oracle不支持
    public class RMS_RECIPE_GROUP_MAPPING_SUBRECIPE
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        [SugarColumn(IsJson = true)]
        public List<string> RECIPE_ID_LIST { get; set; }
        public string RECIPE_GROUP_ID { get; set; }
    }
}

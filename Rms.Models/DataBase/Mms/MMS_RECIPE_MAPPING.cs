using Rms.Models.DataBase.Rms;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Mms
{
    public class MMS_GROUP_MAPPING
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");

        public string MATERIAL_DIC_ID { get; set; }
        public string RECIPE_GROUP_ID { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(MATERIAL_DIC_ID))]
        public MMS_MATERIAL_DIC MATERIAL_DIC { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(RECIPE_GROUP_ID))]
        public RMS_RECIPE_GROUP RECIPE_GROUP { get; set; } 
    }
}

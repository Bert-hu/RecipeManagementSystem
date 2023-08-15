using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    public enum RecipeType
    {
        Secs = 0,
        File = 1
    }
    public class RMS_EQUIPMENT
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } 
        public string Name { get; set; }
        public string CREATOR { get; set; } = "Default";
        public DateTime? CREATETIME { get; set; } = DateTime.Now;
        public string LASTEDITOR { get; set; } = "Default";
        public DateTime? LASTEDITTIME { get; set; } = DateTime.Now;
        public String RECIPE_TYPE { get; set; }  //secsString,secsByte,file
        public string FLOW_ID { get; set; }
    }
}

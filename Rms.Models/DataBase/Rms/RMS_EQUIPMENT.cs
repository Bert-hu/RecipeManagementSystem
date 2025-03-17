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
        public string NAME { get; set; }
        public string CREATOR { get; set; } = "Default";
        public DateTime? CREATETIME { get; set; } = DateTime.Now;
        public string LASTEDITOR { get; set; } = "Default";
        public DateTime? LASTEDITTIME { get; set; } = DateTime.Now;
        public String RECIPE_TYPE { get; set; }  //secsString,secsByte,file
        public int ORDERSORT { get; set; }
        public string TYPE { get; set; }
        public string LINE { get; set; }
        [SugarColumn(IsNullable = true)]
        public string LASTRUN_RECIPE_ID { get; set; }
        [SugarColumn(IsNullable = true)]
        public DateTime LASTRUN_RECIPE_TIME { get; set; }
        [SugarColumn(IsNullable = true)]
        public string CURRENT_PRODUCT { get; set; }
        [SugarColumn(IsNullable = true)]
        public string RECIPE_PATH { get; set; }
        [SugarColumn(IsNullable = true)]
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }
        public bool ISLOCKED { get; set; } = false;
        [SugarColumn(IsNullable = true)]
        public string LOCKED_MESSAGE { get; set; } = string.Empty;
        [SugarColumn(IsNullable = true)]

        public string FATHER_EQID { get; set; }//只适用于Golden Machine， Golden Recipe的RecipeTypeFunction
    }
}

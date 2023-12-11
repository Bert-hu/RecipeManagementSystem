using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Rms.Models.DataBase.Rms
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("RMS_RECIPE")]
    public partial class RMS_RECIPE
    {
        public RMS_RECIPE()
        {
        }

        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string EQUIPMENT_ID { get; set; }
        public string NAME { get; set; }
        public string VERSION_LATEST_ID { get; set; }
        public string VERSION_EFFECTIVE_ID { get; set; }
        //public string FLOW_ID { get; set; }//
        public string MARKING_LATEST_ID { get; set; }
        public string MARKING_EFFECTIVE_ID { get; set; }
        // public string CREATOR { get; set; } = "Default";
        // public DateTime? CREATETIME { get; set; } = DateTime.Now;
        // public string LASTEDITOR { get; set; } = "Default";
        // public DateTime? LASTEDITTIME { get; set; } = DateTime.Now;

        //public string RECIPE_GROUP_ID { get; set; }


    }


    public class MarkingRecipe
    {
        public string EQID { get; set; }
        public string EQNAME { get; set; }
        public string FLOW_ID { get; set; }
        public string RECIPEID { get; set; }
        public string RECIPENAME { get; set; }
        public string MARKING_LATEST_ID { get; set; }
        public string MARKING_EFFECTIVE_ID { get; set; }
        public int LATEST_VERSION { get; set; }
        public int EFFECTIVE_VERSION { get; set; }

    }
}

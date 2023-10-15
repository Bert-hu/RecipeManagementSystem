using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Rms.Models.DataBase.Rms
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("RMS_CHANGE_RECORD")]
    public partial class RMS_CHANGE_RECORD
    {
        public RMS_CHANGE_RECORD()
        {
        }



        public string EQID { get; set; }
        public string FROM_RECIPE_NAME { get; set; }
        public string FROM_RECIPE_VERSION { get; set; }
        public string TO_RECIPE_NAME { get; set; }
        public string TO_RECIPE_VERSION { get; set; }
        public string CREATOR { get; set; } = "Default";
        public DateTime? CREATETIME { get; set; } = DateTime.Now;



    }
}

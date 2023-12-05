using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rms.Web.ViewModels
{
    public class EquipmentViewModel
    {
        public string ID { get; set; }
        public string NAME { get; set; }
        public string CREATOR { get; set; } 
        public DateTime? CREATETIME { get; set; } 
        public string LASTEDITOR { get; set; }
        public DateTime? LASTEDITTIME { get; set; }
        public String RECIPE_TYPE { get; set; }  
        public string FLOW_ID { get; set; }
        public int ORDERSORT { get; set; }
        public string TYPE { get; set; }
        public string LINE { get; set; }
        public string LASTRUN_RECIPE_ID { get; set; }
        public DateTime LASTRUN_RECIPE_TIME { get; set; }
        public string TYPEID { get; set; }
        public string TYPENAME { get; set; }
        public string TYPEPROCESS { get; set; }
        public int TYPEORDERSORT { get; set; }
        public string TYPEVENDOR { get; set; }
        public string TYPETYPE { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
}
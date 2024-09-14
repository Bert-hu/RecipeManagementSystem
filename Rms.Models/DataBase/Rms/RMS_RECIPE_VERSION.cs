using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using SqlSugar;

namespace Rms.Models.DataBase.Rms
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("RMS_RECIPE_VERSION")]
    public partial class RMS_RECIPE_VERSION
    {
        public RMS_RECIPE_VERSION()
        {

        }
         
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string RECIPE_ID { get; set; }
        public decimal? VERSION { get; set; } = 1;
        public string FLOW_ROLES { get; set; } = "[]";
        public int CURRENT_FLOW_INDEX { get; set; }
        public string RECIPE_DATA_ID { get; set; }
        public string REMARK { get; set; }
        public string CREATOR { get; set; } = "Default";
        public DateTime? CREATE_TIME { get; set; } = DateTime.Now;

        [SugarColumn(IsIgnore = true)]
        public List<string> _FLOW_ROLES
        {
            get { return JsonConvert.DeserializeObject<List<string>>(FLOW_ROLES); }
            set { FLOW_ROLES = JsonConvert.SerializeObject(value); }
        }
    }

    public class RecipeVersion
    {
        public string RECIPE_ID { get; set; }

        public string RECIPE_NAME { get; set; }

        public decimal RECIPE_LATEST_VERSION { get; set; }

        public decimal RECIPE_EFFECTIVE_VERSION { get; set; }
        public string RECIPE_LATEST_VERSION_ID { get; set; }
        public string RECIPE_EFFECTIVE_VERSION_ID { get; set; }
    }
}

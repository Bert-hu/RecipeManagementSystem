using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    public class RMS_RECIPE_DATA
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string NAME { get; set; }
        public byte[] CONTENT { get; set; }
        public string CREATOR { get; set; } = "Default";
        public DateTime CREATE_TIME { get; set; } = DateTime.Now;
    }
}

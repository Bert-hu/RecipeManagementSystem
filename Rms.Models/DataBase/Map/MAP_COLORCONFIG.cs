using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Map
{
    public class MAP_COLORCONFIG
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int CODE { get; set; }
        public string COLOR { get; set; }
    }
}

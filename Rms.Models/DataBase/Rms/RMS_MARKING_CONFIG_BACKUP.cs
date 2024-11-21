using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.DataBase.Rms
{
    /// <summary>
    /// 用于第二人的模版创建
    /// </summary>
    public class RMS_MARKING_CONFIG_BACKUP: RMS_MARKING_CONFIG
    {
        //[SugarColumn(IsPrimaryKey = true)]
        //public string ID { get; set; } = Guid.NewGuid().ToString("N");
        //public string MARKING_VERSION_ID { get; set; }
        //public string TYPE { get; set; } = "Fixed"; //"Fixed" or "Code"
        //public string CONTENT { get; set; }
        //public int START_INDEX{ get; set; }//TYPE为Code时开始截取位置
        //public int LENGTH { get; set; }//TYPE为Code时截取的长度
        //public int TEXTINDEX { get; set; }//打印的行数
        //public int TEXTORDER { get; set; }//打印行数的顺序，用于同一行的CONTENT拼接
        //public string CREATOR { get; set; } = "Default";
        //public DateTime CREATE_TIME { get; set; } = DateTime.Now;

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rms.Web.ViewModels
{
    public class MaterialToolingViewModel
    {
    }

    public class MachineConfigVM
    {
        public string EQID { get; set; }           // 对应 RE.ID
        public string ETID { get; set; }           // 对应 RET.ID
        public string MID { get; set; }            // 对应 MMD.ID
        public string SHOWNAME { get; set; }       // 对应 MMD.SHOWNAME
        public string MTYPE { get; set; }          // 对应 MMD.TYPE
        public string TYPE_CODE { get; set; }
        public string MMCID { get; set; }          // 对应 MMC.ID
        public string VALUE { get; set; }          // 对应 MMC.VALUE
        public string LASTEDITOR { get; set; }     // 对应 MMC.LASTEDITOR
        public DateTime? LASTEDITTIME { get; set; } // 对应 MMC.LASTEDITTIME
        public int ORDER_SORT { get; set; }     // 对应 MMC.LASTEDITOR
    }
}
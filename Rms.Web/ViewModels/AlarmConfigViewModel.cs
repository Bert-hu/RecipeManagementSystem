using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rms.Web.ViewModels
{
    public class AlarmConfigVm
    {
        public string NAME { get; set; }
        public string ALID { get; set; }
        public int TRIGGER_INTERVAL { get; set; }
        public int TRIGGER_COUNT { get; set; }
        public bool ISVALID { get; set; }
    }
}
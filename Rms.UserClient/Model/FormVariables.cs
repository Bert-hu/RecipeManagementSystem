using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rms.UserClient.Model
{
    public class FormVariables : BindableBase
    {
        static FormVariables()
        {
        }
        private string station = string.Empty;
        private string emp = string.Empty;
        private string line = string.Empty;
        private string group = string.Empty;
        private string modelname = string.Empty;
        private string reelid = string.Empty;

        public string Station { get => station; set { SetProperty(ref station, value); } }
        public string EMP { get => emp; set { SetProperty(ref emp, value); } }
        public string Line { get => line; set { SetProperty(ref line, value); } }
        public string Group { get => group; set { SetProperty(ref group, value); } }
        public string ModelName { get => modelname; set { SetProperty(ref modelname, value); } }
        public string ReelID { get => reelid; set { SetProperty(ref reelid, value); } }
        public string SfisIp { get; set; } = "10.5.1.128";
        public int SfisPort { get; set; } = 21347;
        public int PinNum { get; set; } = 5;
    }
}

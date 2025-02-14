using System;
using System.Linq;
using System.Text;
using SqlSugar;
using System.Collections.Generic;

namespace Rms.Model
{
    ///<summary>
    ///
    ///</summary>
    ///
    [SugarTable(IsDisabledUpdateAll = true)]
    public class RMS_DIFFERENT_PARAMETER
    {
        public string EQID { get; set; }
        public string RecipeName { get; set; }

        public List<Parameter> Parameter { get; set; }
    }
    [SugarTable("Custom", "客户", IsDisabledUpdateAll = true)]
    public class Parameter
    {
        public string ErrorCode { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string SPEC { get; set; }
    }



}

using Rms.Models.DataBase;
using SqlSugar;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Rms.Services.Core.Services
{
    public class CommonConfiguration
    {
        public  Dictionary<string, string> Configs;
        private readonly ISqlSugarClient db;

        public CommonConfiguration(ISqlSugarClient _sqlSugarClient)
        {
            db = _sqlSugarClient;
            Configs = db.Queryable<COMMON_CONFIG>().ToList().ToDictionary(item => item.KEY, item => item.VALUE);
        }

        public void UpdateConfig(bool isdebugmode)
        {

            if (isdebugmode)
            {
                Configs = db.Queryable<COMMON_CONFIG>().ToList().ToDictionary(item => item.KEY, item => item.DEBUGVALUE);
            }
            else
            {
                Configs = db.Queryable<COMMON_CONFIG>().ToList().ToDictionary(item => item.KEY, item => item.VALUE);
            }
        }
        public string GetConfig(string KEY)
        {
            return Configs[KEY];
        }
    }
}

using Rms.Models.DataBase;
using SqlSugar;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Rms.Utils
{
    public class CommonConfiguration
    {
        public static Dictionary<string, string> Configs;
        static CommonConfiguration()
        {
            var db = DbFactory.GetSqlSugarClient();
            Configs = db.Queryable<COMMON_CONFIG>().ToList().ToDictionary(item => item.KEY, item => item.VALUE);
        }

        public static void UpdateConfig(bool isdebugmode)
        {
            var db = DbFactory.GetSqlSugarClient();

            if (isdebugmode)
            {
                Configs = db.Queryable<COMMON_CONFIG>().ToList().ToDictionary(item => item.KEY, item => item.DEBUGVALUE);
            }
            else
            {
                Configs = db.Queryable<COMMON_CONFIG>().ToList().ToDictionary(item => item.KEY, item => item.VALUE);
            }
        }
        public static string GetConfig(string KEY)
        {
            return Configs[KEY];
        }
    }
}

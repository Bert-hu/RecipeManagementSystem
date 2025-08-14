using Rms.Models.DataBase.Rms;
using SqlSugar;
using System.Configuration;

namespace Rms.Utils
{
    public class DbFactory
    {
        /// <summary>
        /// SqlSugarClient属性
        /// </summary>
        /// <returns></returns>
        public static SqlSugarClient GetSqlSugarClient(string dbConnName ="USI_DPSRMS")
        {
            var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConfigurationManager.ConnectionStrings[dbConnName].ToString(), //必填
                DbType = DbType.Oracle, //必填
                IsAutoCloseConnection = true, //默认false
                InitKeyType = InitKeyType.Attribute,

            }); //默认SystemTable

            db.CodeFirst.InitTables(typeof(RMS_RECIPE_GROUP_MAPPING_SUBRECIPE));

            return db;
        }
    }
}

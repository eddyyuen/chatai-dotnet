using Microsoft.IdentityModel.Protocols;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI.Repository
{
    public class DBInstance
    {
        public static SqlSugarClient GetInstance(string? connectionString, DbType dbType, bool isAutoCloseConnection = true)
        {
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = connectionString,
                DbType = dbType,
                IsAutoCloseConnection = isAutoCloseConnection,
                InitKeyType = InitKeyType.Attribute
            });
            return db;
        }
        public static SqlSugarClient GetInstance()
        {
            return GetInstance(_connectionstring, DbType.Sqlite,  true);
        }

        private static string? _connectionstring;
        public static void Init(string? connectionstring)
        {
            _connectionstring = connectionstring;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data.SqlServerCe
{
    public class Main
    {
        private const string ProviderName = "sqlce";

        /// <summary>
        /// Register database connection provider
        /// </summary>
        public static void RegisterConnectionProvider()
        {
            ConnectionProvider.Register(ProviderName, CreateSqlCeProvider);
        }

        private static ConnectionProvider CreateSqlCeProvider(string serverName, string connectionString)
        {
            var connectionBuilder = new SimpleDbConnectionStringBuilder(connectionString);
            connectionBuilder.Remove("provider");

            return new SqlCeConnectionProvider(serverName, connectionBuilder.ConnectionString);
        }
    }
}

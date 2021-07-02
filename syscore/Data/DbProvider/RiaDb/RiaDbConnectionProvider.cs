using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Sys.Networking;

namespace Sys.Data
{
    class RiaDbConnectionProvider : ConnectionProvider
    {

        public RiaDbConnectionProvider(string name, string connectionString)
            : base(name, ConnectionProviderType.SqlServerRia, new SimpleDbConnectionStringBuilder(connectionString))
        {
        }


        public override bool CheckConnection()
        {
            return HttpRequest.GetHttpStatus(new Uri(DataSource)) == System.Net.HttpStatusCode.OK;
        }



        protected override DbSchemaProvider GetSchema()
        {
            return new SqlDbSchemaProvider(this);
        }


        public override DbProviderType DpType
        {
            get
            {
                return DbProviderType.RiaDb;
            }
        }

        public override DbConnection NewDbConnection
        {
            get
            {
                return new RiaDbConnection(this);
            }
        }

        public override string CurrentDatabaseName()
        {

            return InitialCatalog;
        }

        public override DbProvider CreateDbProvider(string script)
        {
            return new RiaDbProvider(script, this);
        }

    }
}

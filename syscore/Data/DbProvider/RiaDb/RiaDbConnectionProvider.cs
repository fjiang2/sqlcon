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
            : base(name, ConnectionProviderType.SqlServerRia, connectionString)
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


        internal override DbProviderType DpType
        {
            get
            {
                return DbProviderType.RiaDb;
            }
        }

        internal override DbConnection NewDbConnection
        {
            get
            {
                return new RiaDbConnection(this);
            }
        }

        internal override string CurrentDatabaseName()
        {

            return InitialCatalog;
        }

        internal override DbProvider CreateDbProvider(string script)
        {
            return new RiaDbProvider(script, this);
        }

    }
}

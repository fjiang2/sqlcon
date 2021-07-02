using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlServerCe;
using System.Data;
using System.Data.Common;

namespace Sys.Data
{
    class SqlCeConnectionProvider : ConnectionProvider
    {

        public SqlCeConnectionProvider(string name, string connectionString)
            : base(name, ConnectionProviderType.SqlServerCe, new SqlCeConnectionStringBuilder(connectionString))
        {
        }

        public override int Version => 4;

        public override bool CheckConnection()
        {
            return true;
        }

        public override string InitialCatalog
        {
            get
            {
                return SqlCeSchemaProvider.SQLCE_DATABASE_NAME;
            }
            set
            {
            }
        }

        protected override DbSchemaProvider GetSchema()
        {
            return new SqlCeSchemaProvider(this);
        }

        public override SchemaName DefaultTableSchemaName => SchemaName.Empty;

        public override DbProviderType DpType => DbProviderType.SqlCe;

        public override DbConnection NewDbConnection
        {
            get
            {
                return new SqlCeConnection(ConnectionString);
            }
        }

        public override string CurrentDatabaseName()
        {
            return SqlCeSchemaProvider.SQLCE_DATABASE_NAME;
        }

        public override DbProvider CreateDbProvider(string script)
        {
            return new SqlCeProvider(script, this);
        }

    }
}

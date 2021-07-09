using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;

namespace Sys.Data
{
    class SqliteConnectionProvider : ConnectionProvider
    {

        public SqliteConnectionProvider(string name, string connectionString)
            : base(name, ConnectionProviderType.Sqlite, new SQLiteConnectionStringBuilder(connectionString))
        {
        }

        public override int Version => 3;

        public override bool CheckConnection()
        {
            return true;
        }

        public override string InitialCatalog
        {
            get
            {
                return SqliteSchemaProvider.SQLITE_DATABASE_NAME;
            }
            set
            {
            }
        }

        protected override DbSchemaProvider GetSchema()
        {
            return new SqliteSchemaProvider(this);
        }

        public override SchemaName DefaultTableSchemaName => SchemaName.Empty;

        public override DbProviderType DpType => DbProviderType.Sqlite;

        public override DbConnection NewDbConnection
        {
            get
            {
                return new SQLiteConnection(ConnectionString);
            }
        }

        public override string CurrentDatabaseName()
        {
            return SqliteSchemaProvider.SQLITE_DATABASE_NAME;
        }

        public override DbProvider CreateDbProvider(string script)
        {
            return new SqliteProvider(script, this);
        }

    }
}

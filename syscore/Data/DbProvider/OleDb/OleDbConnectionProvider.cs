using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.Data.Common;

namespace Sys.Data
{
    class OleDbConnectionProvider : ConnectionProvider
    {

        public OleDbConnectionProvider(string name, ConnectionProviderType type,  string connectionString)
            : base(name, type, new OleDbConnectionStringBuilder(connectionString))
        {
        }

        public OleDbConnectionProvider(string name, string connectionString)
            : base(name, ConnectionProviderType.OleDbServer, new OleDbConnectionStringBuilder(connectionString))
        {
        }


        public override bool CheckConnection()
        {
            return !InvalidSqlClause("EXEC sp_databases");
        }


        private bool InvalidSqlClause(string sql)
        {
            var conn = new OleDbConnection(ConnectionString);
            try
            {
                conn.Open();
                var cmd = new OleDbCommand(sql, conn);
                cmd.ExecuteScalar();
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                conn.Close();
            }

            return false;
        }


        protected override DbSchemaProvider GetSchema()
        {
           return new OleDbSchemaProvider(this);
        }

        public override SchemaName DefaultTableSchemaName => SchemaName.Dbo;

        public override DbProviderType DpType
        {
            get
            {
                return DbProviderType.OleDb;
            }
        }

        public override DbConnection NewDbConnection
        {
            get
            {
                return new OleDbConnection(ConnectionString);
            }
        }

        public override string CurrentDatabaseName()
        {
            return (string)this.ExecuteScalar("SELECT DB_NAME()");
        }

        public override DbProvider CreateDbProvider(string script)
        {
            return new OleDbProvider(script, this);
        }

    }
}

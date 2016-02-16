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

        public OleDbConnectionProvider(int handle, string name, ConnectionProviderType type,  string connectionString)
            : base(handle, name, type, connectionString)
        {
        }

        public OleDbConnectionProvider(int handle, string name, string connectionString)
            : base(handle, name, ConnectionProviderType.OleDbServer, connectionString)
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
            throw new NotImplementedException($"schema provider not implemented on {Type}");
        }


        internal override DbProviderType DpType
        {
            get
            {
                return DbProviderType.OleDb;
            }
        }

        internal override DbConnection NewDbConnection
        {
            get
            {
                return new OleDbConnection(ConnectionString);
            }
        }

        internal override string CurrentDatabaseName()
        {
            return (string)DataExtension.ExecuteScalar(this, "SELECT DB_NAME()");
        }

        internal override DbProvider CreateDbProvider(string script)
        {
            return new OleDbProvider(script, this);
        }

    }
}

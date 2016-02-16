using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Sys.Data
{
    class SqlDbConnectionProvider : ConnectionProvider
    {

        public SqlDbConnectionProvider(int handle, string name, string connectionString)
            : base(handle, name, ConnectionProviderType.SqlServer, connectionString)
        {
        }

        private int version = -1;
        public override int Version
        {
            get
            {
                if (version != -1)
                    return version;

                if (this.Type == ConnectionProviderType.SqlServer)
                {
                    SqlConnection conn = new SqlConnection(ConnectionString);
                    try
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("SELECT @@version", conn);
                        string text = (string)cmd.ExecuteScalar();
                        string[] items = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        version = int.Parse(items[3]);
                    }
                    catch (Exception)
                    {
                        version = 0;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                return version;
            }
        }

        public override bool CheckConnection()
        {
            return !InvalidSqlClause("EXEC sp_databases");
        }


        private bool InvalidSqlClause(string sql)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
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
            return new SqlDbSchemaProvider(this);
        }


        internal override DbProviderType DpType
        {
            get
            {
                return DbProviderType.SqlDb;
            }
        }

        internal override DbConnection NewDbConnection
        {
            get
            {
                return new SqlConnection(ConnectionString);
            }
        }

        internal override string CurrentDatabaseName()
        {
            return (string)DataExtension.ExecuteScalar(this, "SELECT DB_NAME()");
            //var connection = new SqlCmd(provider, string.Empty).DbProvider.DbConnection;
            //return connection.Database.ToString();
        }

        internal override DbProvider CreateDbProvider(string script)
        {
            return new SqlDbProvider(script, this);
        }

    }
}

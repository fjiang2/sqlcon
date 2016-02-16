using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Sys.IO;

namespace Sys.Data
{
    class XmlDbConnectionProvider : ConnectionProvider
    {

        public XmlDbConnectionProvider(string name, string connectionString)
            : base(name, ConnectionProviderType.XmlFile, connectionString)
        {
        }
        public override bool CheckConnection()
        {
            return FileLink.Factory(DataSource, this.UserId, this.Password).Exists;
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
            return new XmlDbSchemaProvider(this);
        }


        internal override DbProviderType DpType
        {
            get
            {
                return DbProviderType.XmlDb;
            }
        }

        internal override DbConnection NewDbConnection
        {
            get
            {
                return new XmlDbConnection(this);
            }
        }

        internal override string CurrentDatabaseName()
        {
            return InitialCatalog;
        }

        internal override DbProvider CreateDbProvider(string script)
        {
            return new XmlDbProvider(script, this);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Sys.Data.IO;

namespace Sys.Data
{
    class FileDbConnectionProvider : ConnectionProvider
    {
        public DbFileType DbFileType { get; }
        public IDbFile DataFile { get; }

        public FileLink FileLink { get; }

        public FileDbConnectionProvider(string name, string connectionString, DbFileType fileType)
            : base(name, ConnectionProviderType.DbFile, new SimpleDbConnectionStringBuilder(connectionString))
        {
            this.DbFileType = fileType;
            this.FileLink = FileLink.CreateLink(DataSource, this.UserId, this.Password);
            this.FileLink.Options = ConnectionBuilder;

            this.DataFile = DbFile.Create(DbFileType, FileLink);
        }

        public override bool CheckConnection()
        {
            return FileLink.Exists;
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
            return new FileDbSchemaProvider(this);
        }


        public override DbProviderType DpType => DbProviderType.FileDb;

        public override DbConnection NewDbConnection
        {
            get
            {
                return new FileDbConnection(this);
            }
        }

        public override string CurrentDatabaseName()
        {
            return InitialCatalog;
        }

        public override DbProvider CreateDbProvider(string script)
        {
            return new FileDbProvider(script, this);
        }

    }
}

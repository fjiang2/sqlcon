using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data.IO;

namespace Sys.Data
{
    class FileDbSchemaProvider : DbSchemaProvider, IDisposable
    {
        private DataSet dbSchema = new DataSet();
        private IDbFile dbFile;

        public FileDbSchemaProvider(FileDbConnectionProvider provider)
                    : base(provider)
        {
            this.dbFile = provider.DataFile;

            var link = FileLink.CreateLink(provider.DataSource, provider.UserId, provider.Password);
            dbFile.ReadSchema(link, dbSchema);
        }


        public void Dispose()
        {
            if (dbSchema != null)
            {
                dbSchema.Dispose();
                dbSchema = null;
            }
        }


        public override DatabaseName[] GetDatabaseNames()
        {
            List<DatabaseName> dnames = new List<DatabaseName>();
            foreach (DataTable table in dbSchema.Tables)
            {
                DatabaseName dname = new DatabaseName(provider, table.TableName);
                dnames.Add(dname);
            }

            return dnames.ToArray();
        }

        public override TableName[] GetTableNames(DatabaseName dname)
        {
            return InformationSchema.XmlTableNames(dname, dbSchema.Tables[dname.Name]);
        }

        public override TableName[] GetViewNames(DatabaseName dname)
        {
            return new TableName[] { };
        }

        public override DataTable GetTableSchema(TableName tname)
        {
            return InformationSchema.XmlTableSchema(tname, dbSchema.Tables[tname.DatabaseName.Name]);
        }

        public override DataTable GetDatabaseSchema(DatabaseName dname)
        {
            return dbSchema.Tables[dname.Name];
        }

        public override DataSet GetServerSchema(ServerName sname)
        {
            return dbSchema;
        }

        public override DependencyInfo[] GetDependencySchema(DatabaseName dname)
        {
            var dt = this.dbSchema.Tables[0];
            var L = dt.AsEnumerable().Where(row =>
                row[nameof(ColumnSchema.PK_Schema)] != DBNull.Value &&
                row[nameof(ColumnSchema.PK_Table)] != DBNull.Value &&
                row[nameof(ColumnSchema.PK_Column)] != DBNull.Value);

            DependencyInfo[] rows = L.Select(
               row => new DependencyInfo
               {
                   FkTable = new TableName(dname, (string)row["SchemaName"], (string)row["TableName"]),
                   PkTable = new TableName(dname, (string)row[nameof(ColumnSchema.PK_Schema)], (string)row[nameof(ColumnSchema.PK_Table)]),
                   PkColumn = (string)row[nameof(ColumnSchema.PK_Column)],
                   FkColumn = (string)row[nameof(ColumnSchema.ColumnName)]
               })
               .ToArray();

            return rows;
        }
    }
}

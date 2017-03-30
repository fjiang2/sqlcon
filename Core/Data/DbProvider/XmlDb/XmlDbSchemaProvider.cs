using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys.Data.IO;

namespace Sys.Data
{
    class XmlDbSchemaProvider : DbSchemaProvider, IDisposable
    {
        DataSet dbSchema;

        public void Dispose()
        {
            if (dbSchema != null)
            {
                dbSchema.Dispose();
                dbSchema = null;
            }
        }
        public XmlDbSchemaProvider(ConnectionProvider provider)
                    : base(provider)
        {
            var link = FileLink.CreateLink(provider.DataSource, provider.UserId, provider.Password);
            dbSchema = new DataSet();

            try
            {
                link.ReadXml(dbSchema);

                if (dbSchema.Tables.Count == 0)
                    throw new Exception(string.Format("error in xml schema file: {0}", provider));
            }
            catch (Exception)
            {
                throw new Exception($"bad data source defined {provider.DataSource}");
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

        public override DataTable GetDependencySchema(DatabaseName dname)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("FK_SCHEMA", typeof(string)));
            dt.Columns.Add(new DataColumn("FK_Table", typeof(string)));
            dt.Columns.Add(new DataColumn("PK_SCHEMA", typeof(string)));
            dt.Columns.Add(new DataColumn("PK_Table", typeof(string)));

            dt.AcceptChanges();
            return dt;
        }
    }
}

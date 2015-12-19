using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data
{
    class XmlDbSchemaProvider : SchemaProvider
    {
        DataSet dbSchema;

        public XmlDbSchemaProvider(ConnectionProvider provider)
            : base(provider)
        {
            dbSchema = new DataSet();
            using (var reader = new System.IO.StreamReader(provider.DataSource))
            {
                dbSchema = new DataSet();
                dbSchema.ReadXml(reader);
                if (dbSchema.Tables.Count == 0)
                    throw new Exception(string.Format("error in xml schema file: {0}", provider));
            }

        }

      
        public override DatabaseName[] GetDatabaseNames()
        {
            List<DatabaseName> dnames = new List<DatabaseName>();
            foreach(DataTable table in dbSchema.Tables)
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

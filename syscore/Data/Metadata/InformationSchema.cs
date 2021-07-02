using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;


namespace Sys.Data
{
    public static class InformationSchema
    {

        public static DataSet LoadDatabaseSchema(ServerName sname, IEnumerable<DatabaseName> dnames, Func<DatabaseName, string> sqlOfDatabaseSchema)
        {
            StringBuilder builder = new StringBuilder();
            foreach (DatabaseName dname in dnames)
            {
                builder.AppendLine(sqlOfDatabaseSchema(dname));
            }

            DataSet ds = new SqlCmd(sname.Provider, builder.ToString()).FillDataSet();
            ds.DataSetName = sname.Path;

            int i = 0;
            foreach (DatabaseName dname in dnames)
            {
                ds.Tables[i++].TableName = dname.Name;
            }

            return ds;
        }

        internal static DataTable XmlTableSchema(TableName tableName, DataTable schema)
        {
            DataTable dt = new DataTable();
            foreach (DataColumn column in schema.Columns)
            {
                if (column.ColumnName != "SchemaName" && column.ColumnName != "TableName")
                    dt.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
            }

            var rows = schema.AsEnumerable().Where(row => row.Field<string>("SchemaName") == tableName.SchemaName && row.Field<string>("TableName").ToLower() == tableName.Name.ToLower());
            foreach (var row in rows)
            {
                DataRow newRow = dt.NewRow();
                foreach (DataColumn column in dt.Columns)
                {
                    newRow[column] = row[column.ColumnName];
                }

                dt.Rows.Add(newRow);
            }

            dt.AcceptChanges();
            return dt;
        }


        internal static TableName[] XmlTableNames(this DatabaseName databaseName, DataTable schema)
        {
            var rows = schema.AsEnumerable()
                .Select(row => new { schema = row.Field<string>("SchemaName"), name = row.Field<string>("TableName") })
                .Distinct();

            List<TableName> tnames = new List<TableName>();
            foreach (var row in rows)
            {
                TableName tname = new TableName(databaseName, row.schema, row.name);
                tnames.Add(tname);
            }

            return tnames.ToArray();
        }

    }
}

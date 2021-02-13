using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace Sys.Data
{
    public static class DataTableExtension
    {
        public static DataColumn[] IdentityKeys(this DataTable dt, IdentityKeys keys)
        {
            return GetDataColumns(dt, keys.ColumnNames);
        }

        public static DataColumn[] ForeignKeys(this DataTable dt, IForeignKeys keys)
        {
            return GetDataColumns(dt, keys.Keys.Select(x => x.FK_Column));
        }

        public static DataColumn[] PrimaryKeys(this DataTable dt, IPrimaryKeys keys)
        {
            return PrimaryKeys(dt, keys.Keys);
        }

        public static DataColumn[] PrimaryKeys(this DataTable dt, string[] keys)
        {
            DataColumn[] primaryKey = GetDataColumns(dt, keys);

            dt.PrimaryKey = primaryKey;
            return primaryKey;
        }

        public static DataColumn[] GetDataColumns(this DataTable dt, IEnumerable<string> columnNames)
        {
            var L = columnNames.Select(key => key.ToUpper());

            DataColumn[] _columns = dt.Columns
                .Cast<DataColumn>()
                .Where(column => L.Contains(column.ColumnName.ToUpper()))
                .ToArray();

            return _columns;
        }

        public static void SetSchemaAndTableName(this DataTable dt, TableName tname)
        {
            var sname = new DataTableSchemaName(dt);
            sname.SetSchemaAndTableName(tname);
        }

        public static bool IsDbo(this DataTable dt)
        {
            var sname = new DataTableSchemaName(dt);
            return sname.IsDbo;
        }

        public static string GetSchemaName(this DataTable dt)
        {
            var sname = new DataTableSchemaName(dt);
            return sname.SchemaName;
        }

        public static int WriteSql(this DataTable dt, TextWriter writer, TableName tname)
        {
            tname.SetTableSchema(dt);
            string SQL = tname.GenerateCreateTableClause(appendGO: true);
            writer.WriteLine(SQL);

            TableSchema schema = new TableSchema(tname);
            SqlScriptGeneration gen = new SqlScriptGeneration(SqlScriptType.INSERT, schema);
            return gen.GenerateByDbTable(dt, writer);
        }

        public static int WriteSql(this DataSet ds, TextWriter writer, DatabaseName dname)
        {
            int count = 0;
            foreach (DataTable dt in ds.Tables)
            {
                TableName tname = new TableName(dname, SchemaName.dbo, dt.TableName);
                count += WriteSql(dt, writer, tname);
            }

            return count;
        }
    }
}

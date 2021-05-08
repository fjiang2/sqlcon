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
        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            var properties = typeof(T).GetProperties();

            DataTable dt = new DataTable();
            foreach (var propertyInfo in properties)
            {
                dt.Columns.Add(new DataColumn(propertyInfo.Name, propertyInfo.PropertyType));
            }

            Func<T, object[]> selector = row =>
            {
                var values = new object[properties.Length];
                int i = 0;

                foreach (var propertyInfo in properties)
                {
                    values[i++] = propertyInfo.GetValue(row);
                }

                return values;
            };

            foreach (T row in source)
            {
                object[] values = selector(row);
                var newRow = dt.NewRow();
                int k = 0;
                foreach (var item in values)
                {
                    newRow[k++] = item;
                }

                dt.Rows.Add(newRow);
            }
            
            dt.AcceptChanges();
            return dt;
        }

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
            TableSchemaManager.SetTableSchema(tname, dt);
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

        public static List<T> ToList<T>(this DataTable dt) where T : new()
        {
            if (dt == null)
                return null;

            List<T> list = new List<T>();
            if (dt.Rows.Count == 0)
                return list;

            var properties = typeof(T).GetProperties();
            Dictionary<DataColumn, System.Reflection.PropertyInfo> d = new Dictionary<DataColumn, System.Reflection.PropertyInfo>();
            foreach (DataColumn column in dt.Columns)
            {
                var property = properties.FirstOrDefault(p => p.Name.ToUpper() == column.ColumnName.ToUpper());
                if (property != null)
                {
                    Type ct = column.DataType;
                    Type pt = property.PropertyType;

                    if (pt == ct || (pt.GetGenericTypeDefinition() == typeof(Nullable<>) && pt.GetGenericArguments()[0] == ct))
                        d.Add(column, property);
                }
            }

            foreach (DataRow row in dt.Rows)
            {
                T item = new T();
                foreach (DataColumn column in dt.Columns)
                {
                    if (d.ContainsKey(column))
                    {
                        var propertyInfo = d[column];
                        object obj = row[column];

                        if (obj != null && obj != DBNull.Value)
                            propertyInfo.SetValue(item, obj);
                    }
                }

                list.Add(item);
            }

            return list;
        }
    }
}

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

        public static T[] ToArray<T>(this DataTable dataTable, string columnName)
        {
            return ToArray<T>(dataTable, row => (T)row[columnName]);
        }

        public static T[] ToArray<T>(this DataTable dataTable, int columnIndex = 0)
        {
            return ToArray<T>(dataTable, row => (T)row[columnIndex]);
        }

        public static T[] ToArray<T>(this DataTable dataTable, Func<DataRow, T> func)
        {
            T[] values = new T[dataTable.Rows.Count];

            int i = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                values[i++] = func(row);
            }

            return values;
        }


        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this DataTable dataTable, Func<DataRow, TKey> keySelector, Func<DataRow, TValue> valueSelector)
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

            foreach (DataRow row in dataTable.Rows)
            {
                TKey key = keySelector(row);
                TValue value = valueSelector(row);
                if (dict.ContainsKey(key))
                    dict[key] = value;
                else
                    dict.Add(key, value);
            }

            return dict;
        }

        public static List<T> ToList<T>(this DataTable dt, Func<int, DataRow, bool> where, Func<int, DataRow, T> select)
        {
            List<T> list = new List<T>();
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (where(i, row))
                    list.Add(select(i, row));
                i++;
            }

            return list;
        }

        public static List<T> ToList<T>(this DataTable dt, Func<DataRow, bool> where, Func<DataRow, T> select)
        {
            return dt.ToList((_, row) => where(row), (_, row) => select(row));
        }

        public static List<T> ToList<T>(this DataTable dt, Func<int, DataRow, T> select)
        {
            return dt.ToList((rowId, row) => true, (rowId, row) => select(rowId, row));
        }

        public static List<T> ToList<T>(this DataTable dt, Func<DataRow, T> select)
        {
            return dt.ToList((rowId, row) => true, (rowId, row) => select(row));
        }

        public static IEnumerable<DataLine> Where(this DataTable dt, Func<DataLine, bool> where)
        {
            return dt.ToLines().Where(where);
        }

        public static IEnumerable<DataLine> ToLines(this DataTable dt)
        {
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                yield return new DataLine { Line = i, Row = row };
                i++;
            }
        }

        public static void Insert(this DataTable dt, Action<DataRow> insert)
        {
            DataRow row = dt.NewRow();
            insert(row);
            dt.Rows.Add(row);

            dt.AcceptChanges();
        }

        public static int Update(this DataTable dt, Action<DataRow> update)
        {
            return Update(dt, (_, row) => true, (_, row) => update(row));
        }

        public static int Update(this DataTable dt, Func<DataRow, bool> where, Action<DataRow> update)
        {
            return Update(dt, (_, row) => where(row), (_, row) => update(row));
        }

        public static int Update(this DataTable dt, Action<int, DataRow> update)
        {
            return Update(dt, (_, row) => true, update);
        }

        public static int Update(this DataTable dt, Func<int, DataRow, bool> where, Action<int, DataRow> update)
        {
            int count = 0;
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (where(i, row))
                {
                    update(i, row);
                    count++;
                }
                i++;
            }

            return count;
        }

        public static int Delete(this DataTable dt, Func<DataRow, bool> where)
        {
            return Delete(dt, (_, row) => where(row));
        }

        public static int Delete(this DataTable dt, Func<int, DataRow, bool> where)
        {
            int count = 0;
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (where(i, row))
                {
                    row.Delete();
                    count++;
                }
                i++;
            }

            dt.AcceptChanges();
            return count;
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

    }
}

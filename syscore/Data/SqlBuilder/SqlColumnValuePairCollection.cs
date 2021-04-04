using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sys.Data
{
    public class SqlColumnValuePairCollection
    {
        protected List<SqlColumnValuePair> columns = new List<SqlColumnValuePair>();

        public SqlColumnValuePairCollection()
        {
        }

        public void Clear()
        {
            columns.Clear();
        }

        /// <summary>
        /// Add all properties of data contract class
        /// </summary>
        /// <param name="data"></param>
        public void AddRange(object data)
        {
            foreach (var propertyInfo in data.GetType().GetProperties())
            {
                object value = propertyInfo.GetValue(data) ?? DBNull.Value;
                Add(propertyInfo.Name, value);
            }
        }

        public void AddRange(IDictionary<string, object> map)
        {
            foreach (var kvp in map)
            {
                Add(kvp.Key, kvp.Value);
            }
        }

        public void AddRange(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                Add(column.ColumnName, row[column]);
            }
        }

        public void AddRange<T>(string columnPrefix, T[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                string column = $"{columnPrefix}{i + 1}";
                Add(column, values[i]);
            }
        }

        public void AddRange(string columnPrefix, object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                string column = $"{columnPrefix}{i + 1}";
                Add(column, values[i]);
            }
        }

        public void AddRange(string[] columns, object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                Add(columns[i], values[i]);
            }
        }

        public virtual SqlColumnValuePair Add(string name, object value)
        {
            SqlColumnValuePair found = columns.Find(c => c.ColumnName == name);
            if (found != null)
            {
                found.Value = new SqlValue(value);
                return found;
            }
            else
            {
                var pair = new SqlColumnValuePair(name, value);
                columns.Add(pair);
                return pair;
            }
        }

        public int RemoveRange(IEnumerable<string> columns)
        {
            int count = 0;
            foreach (var column in columns)
            {
                if (Remove(column))
                    count++;
            }

            return count;
        }

        public bool Remove(string column)
        {
            SqlColumnValuePair found = columns.Find(c => c.ColumnName == column);
            if (found != null)
                return columns.Remove(found);

            return false;
        }

        public IDictionary<string, object> ToDictionary()
        {
            return columns.ToDictionary(c => c.ColumnName, c => c.Value.Value);
        }


        /// <summary>
        /// To SQL column/value list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SqlColumnAndValue> ToList()
        {
            return columns.Select(c => new SqlColumnAndValue { Name = c.ColumnFormalName, Value = c.Value.ToString("N") }).ToList();
        }

        public string Join(Func<SqlColumnValuePair, string> expr, string separator)
        {
            var L = columns.Select(pair => expr(pair));
            return string.Join(separator, L);
        }

        public string Join(string separator)
        {
            return Join(pair => $"{pair.ColumnFormalName} = {pair.Value}", separator);
        }


        public override string ToString()
        {
            return columns.ToString();
        }
    }

}

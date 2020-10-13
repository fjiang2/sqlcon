using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Sys.Data.Linq;

namespace Sys.Data
{
    public class SqlMaker
    {
        private List<ColumnValuePair> Columns { get; } = new List<ColumnValuePair>();

        public string TableName { get; }
        public string[] PrimaryKeys { get; set; }
        public string[] IdentityKeys { get; set; }

        private SqlTemplate template;

        public SqlMaker(string formalName)
        {
            this.TableName = formalName;
            this.template = new SqlTemplate(formalName);
        }

        public void Clear()
        {
            Columns.Clear();
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

        public void Add(string name, object value)
        {
            ColumnValuePair found = Columns.Find(c => c.ColumnName == name);
            if (found != null)
            {
                found.Value = new SqlValue(value);
            }
            else
            {
                bool identity = IdentityKeys != null && IdentityKeys.Contains(name);
                var pair = new ColumnValuePair(name, value);
                pair.Field.Identity = identity;

                Columns.Add(pair);
            }
        }

        public IDictionary<string, object> ToDictionary()
        {
            return Columns.ToDictionary(c => c.ColumnName, c => c.Value.Value);
        }



        private string[] notUpdateColumns => Columns.Where(p => !p.Field.Saved).Select(p => p.Field.Name).ToArray();


        public string Select()
        {
            if (PrimaryKeys.Length > 0)
            {
                var C1 = Columns.Where(c => PrimaryKeys.Contains(c.ColumnName));
                var L1 = string.Join(" AND ", C1.Select(c => c.ToString()));
                return template.Select("*", L1);
            }
            else
                return template.Select("*");
        }

        public string SelectRows() => SelectRows("*");

        public string SelectRows(IEnumerable<string> columns)
        {
            var L1 = string.Join(",", columns.Select(c => FormalName(c)));
            if (L1 == string.Empty)
                L1 = "*";

            return SelectRows(L1);
        }

        private string SelectRows(string columns) => template.Select(columns);

        public string InsertOrUpdate()
        {
            var C1 = Columns.Where(c => PrimaryKeys.Contains(c.ColumnName));
            var L1 = string.Join(" AND ", C1.Select(c => c.ToString()));

            if (PrimaryKeys.Length + notUpdateColumns.Length == Columns.Count)
            {
                return template.IfNotExistsInsert(L1, Insert());
            }
            else
            {
                return template.IfExistsUpdateElseInsert(L1, Update(), Insert());
            }
        }

        public string Insert()
        {
            var C = Columns.Where(c => !c.Field.Identity && !c.Value.IsNull);
            var L1 = string.Join(",", C.Select(c => c.ColumnFormalName));
            var L2 = string.Join(",", C.Select(c => c.Value.ToString()));

            return template.Insert(L1, L2);
        }

        public string Update()
        {
            var C1 = Columns.Where(c => PrimaryKeys.Contains(c.ColumnName));
            var C2 = Columns.Where(c => !PrimaryKeys.Contains(c.ColumnName) && !notUpdateColumns.Contains(c.ColumnName));

            var L1 = string.Join(" AND ", C1.Select(c => c.ToString()));
            var L2 = string.Join(",", C2.Select(c => c.ToString()));

            return template.Update(L2, L1);
        }

        public string Delete()
        {
            var C1 = Columns.Where(c => PrimaryKeys.Contains(c.ColumnName));
            var L1 = string.Join(" AND ", C1.Select(c => c.ToString()));
            return template.Delete(L1);
        }


        public string DeleteAll()
        {
            return template.Delete();
        }

        private static string FormalName(string name)
        {
            if (name.StartsWith("[") && name.EndsWith("]"))
                return name;

            return $"[{name}]";
        }

        internal IDictionary<string, object> Row => Columns.ToDictionary(x => x.ColumnName, x => x.Value.Value);

        public class ColumnValuePair
        {
            public DataField Field { get; }
            public SqlValue Value { get; set; }


            public ColumnValuePair(string columnName, object value)
            {
                this.Field = new DataField(columnName, value?.GetType());
                this.Value = new SqlValue(value);
            }

            public string ColumnName => Field.Name;

            public string ColumnFormalName => FormalName(ColumnName);

            public override string ToString()
            {
                return string.Format("[{0}] = {1}", ColumnName, Value);
            }

        }

    }


}

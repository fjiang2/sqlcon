using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;

namespace Sys.Data
{
    class SqlMaker
    {
        public TableName TableName { get; set; }
        public List<ColumnValuePair> Columns { get; } = new List<ColumnValuePair>();
        public IPrimaryKeys PK { get; set; }

        public SqlMaker()
        {
            Columns = new List<ColumnValuePair>();
        }

        public SqlMaker(IDictionary<string, object> map)
        {
            foreach (var kvp in map)
            {
                Columns.Add(new ColumnValuePair(kvp.Key, kvp.Value));
            }
        }

        public SqlMaker(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                Columns.Add(new ColumnValuePair(column.ColumnName, row[column]));
            }
        }

        private string[] primaryKeys => PK.Keys;

        private string[] notUpdateColumns => Columns.Where(p => !p.Field.Saved).Select(p => p.Field.Name).ToArray();


        public string Select()
        {
            if (primaryKeys.Length > 0)
            {
                var C1 = Columns.Where(c => primaryKeys.Contains(c.ColumnName));
                var L1 = string.Join(" AND ", C1.Select(c => c.ToString()));
                return $"SELECT * FROM {TableName} WHERE {L1}";
            }
            else
                return $"SELECT * FROM {TableName}";
        }

        public string InsertOrUpdate()
        {
            var C1 = Columns.Where(c => primaryKeys.Contains(c.ColumnName));
            var L1 = string.Join(" AND ", C1.Select(c => c.ToString()));

            if (primaryKeys.Length + notUpdateColumns.Length == Columns.Count)
            {
                return string.Format(updateOrInsertCommandTemplate1, L1, Insert());
            }
            else
            {
                return string.Format(updateOrInsertCommandTemplate2, L1, Update(), Insert());
            }
        }

        public string Insert()
        {
            var L1 = string.Join(",", Columns.Select(c => c.ColumnFormalName));
            var L2 = string.Join(",", Columns.Select(c => c.Value.ToString()));

            return string.Format(insertCommandTemplate, L1, L2);
        }

        public string Update()
        {
            var C1 = Columns.Where(c => primaryKeys.Contains(c.ColumnName));
            var C2 = Columns.Where(c => !primaryKeys.Contains(c.ColumnName) && !notUpdateColumns.Contains(c.ColumnName));

            var L1 = string.Join(" AND ", C1.Select(c => c.ToString()));
            var L2 = string.Join(",", C2.Select(c => c.ToString()));

            return string.Format(updateCommandTemplate, L2, L1);
        }

        public string Delete()
        {
            var C1 = Columns.Where(c => primaryKeys.Contains(c.ColumnName));
            var L1 = string.Join(" AND ", C1.Select(c => c.ToString()));
            return string.Format(deleteCommandTemplate, L1);
        }


        public string DeleteAll()
        {
            return $"DELETE FROM [{TableName}]";
        }

        private string selectCommandTemplate => $"SELECT {{0}} FROM {TableName} WHERE {{1}}";
        private string updateOrInsertCommandTemplate1 => $"IF NOT EXISTS(SELECT * FROM {TableName} WHERE {{0}}) {{1}}";
        private string updateOrInsertCommandTemplate2 => $"IF EXISTS(SELECT * FROM {TableName} WHERE {{0}}) {{1}} ELSE {{2}}";
        private string updateCommandTemplate => $"UPDATE {TableName} SET {{0}} WHERE {{1}}";
        private string insertCommandTemplate => $"INSERT INTO {TableName}({{0}}) VALUES({{1}})";
        private string deleteCommandTemplate => $"DELETE FROM {TableName} WHERE {{0}}";


        public class ColumnValuePair
        {
            public DataField Field { get; set; }
            public SqlValue Value;

            public ColumnValuePair(string columnName, object value)
            {
                this.Field = new DataField(columnName, value?.GetType());
                this.Value = new SqlValue(value);
            }

            public string ColumnName => Field.Name;

            public string ColumnFormalName => $"[{ColumnName}]";

            public override string ToString()
            {
                return string.Format("[{0}] = {1}", ColumnName, Value);
            }

        }

    }


}

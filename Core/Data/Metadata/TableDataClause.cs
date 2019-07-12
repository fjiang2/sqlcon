using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Sys.Data.Comparison;

namespace Sys.Data
{
    class TableDataClause
    {
        private ITableSchema schema;
        private TableName tableName;

        private string[] pk;
        private string[] ik;
        private string[] ck;


        public TableDataClause(ITableSchema schema)
        {
            this.schema = schema;
            this.tableName = schema.TableName;

            this.pk = schema.PrimaryKeys.Keys;
            this.ik = schema.Identity.ColumnNames;
            this.ck = schema.Columns.Where(column => column.IsComputed).Select(column => column.ColumnName).ToArray();
        }


        private string WHERE(IEnumerable<ColumnPair> pairs)
        {
            var L1 = pairs.Where(p => pk.Contains(p.ColumnName)).ToArray();
            return string.Join<ColumnPair>(" AND ", L1);
        }

        public string IF_NOT_EXISTS_INSERT(IEnumerable<ColumnPair> pairs)
        {
            return string.Format(ifNotExistsInsertTemplate, WHERE(pairs), INSERT(pairs));
        }

        public string INSERT(IEnumerable<ColumnPair> pairs)
        {
            var L1 = pairs
              .Where(column => !ik.Contains(column.ColumnName))
              .Where(column => !ck.Contains(column.ColumnName));

            var x1 = L1.Select(p => p.ColumnName.ColumnName());
            var x2 = L1.Select(p => p.Value.ToScript());

            return string.Format(insertCommandTemplate,
                string.Join(",", x1),
                string.Join(",", x2)
                );
        }


        public string IF_NOT_EXISTS_INSERT_ELSE_UPDATE(IEnumerable<ColumnPair> pairs)
        {
            return string.Format(ifNotExistsInsertElseUpdateTemplate, WHERE(pairs), INSERT(pairs), UPDATE(pairs));
        }

        public string UPDATE(IEnumerable<ColumnPair> pairs)
        {
            var L1 = pairs
                .Where(column => !ik.Contains(column.ColumnName))
                .Where(column => !pk.Contains(column.ColumnName))
                .Where(column => !ck.Contains(column.ColumnName))
                .Select(p => $"{p.ColumnName.ColumnName()} = {p.Value.ToScript()}");

            string update = string.Join(",", L1);
            return string.Format(updateCommandTemplate, update, WHERE(pairs));
        }


        public string UPDATE(RowCompare compare)
        {
            return string.Format(updateCommandTemplate, compare.Set, compare.Where);
        }

        public string DELETE(DataRow row, IPrimaryKeys primaryKey)
        {
            var L1 = new List<ColumnPair>();
            foreach (var column in primaryKey.Keys)
            {
                L1.Add(new ColumnPair(column, row[column]));
            }

            return string.Format(deleteCommandTemplate, string.Join<ColumnPair>(" AND ", L1));
        }

        
        #region Insert/Update/Delete template

        private string ifNotExistsInsertTemplate => $@"
IF NOT EXISTS(SELECT * FROM {tableName.FormalName} WHERE {{0}})
  {{1}}";

        private string ifNotExistsInsertElseUpdateTemplate => $@"
IF NOT EXISTS(SELECT * FROM {tableName.FormalName} WHERE {{0}})
  {{1}}
ELSE 
  {{2}}";

        private string selectCommandTemplate => $"SELECT {{0}} FROM {tableName.FormalName} WHERE {{1}}";
        private string updateCommandTemplate => $"UPDATE {tableName.FormalName} SET {{0}} WHERE {{1}}";
        private string insertCommandTemplate => $"INSERT INTO {tableName.FormalName}({{0}}) VALUES({{1}})";
        private string deleteCommandTemplate => $"DELETE FROM {tableName.FormalName} WHERE {{0}}";

        #endregion





    }
}

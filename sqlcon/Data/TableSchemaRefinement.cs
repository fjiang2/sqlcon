using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using Sys;
using Sys.Data;

namespace sqlcon
{
    class SchemaRefineOption
    {
        public bool ChangeNotNull { get; set; }
        public bool ConvertInteger { get; set; }
        public bool ConvertBoolean { get; set; }
        public bool ShrinkString { get; set; }
    }

    class TableSchemaRefinement
    {
        private TableName tname;
        private TableSchema schema;

        public TableSchemaRefinement(TableName tname)
        {
            this.tname = tname;
            this.schema = new TableSchema(tname);
        }

        public string Refine(SchemaRefineOption option)
        {
            string SQL = $"SELECT * FROM [{tname.Name}]";
            var dt = FillDataTable(SQL);

            StringBuilder builder = new StringBuilder();
            foreach (IColumn column in schema.Columns)
            {
                ColumnSchema cs = (ColumnSchema)column;
                bool isDirty = false;
                if (option.ChangeNotNull && column.Nullable && !HasNull(dt, column))
                {
                    cs.Nullable = false;
                    isDirty = true;
                }

                if (option.ConvertInteger && CanConvertToInt32(dt, column))
                {
                    cs.DataType = "int";
                    isDirty = true;
                }

                if (option.ConvertBoolean && CanConvertBoolean(dt, column))
                {
                    cs.DataType = "bit";
                    isDirty = true;
                }

                if (option.ShrinkString && ShrinkString(dt, column, out short length))
                {
                    cs.Length = length;
                    isDirty = true;
                }

                if (isDirty)
                {
                    string field = cs.GetSQLField();
                    SQL = $"ALTER TABLE [{tname.Name}] ALTER COLUMN {field}";

                    builder.AppendLine(SQL);
                }
            }

            return builder.ToString();
        }

        private bool ShrinkString(DataTable dt, IColumn column, out short max)
        {
            max = 0;

            if (column.CType != CType.NChar && column.CType != CType.NVarChar && column.CType != CType.Char && column.CType != CType.VarChar)
                return false;

            string columnName = column.ColumnName;
            foreach (DataRow row in dt.Rows)
            {
                object obj = row[columnName];
                if (obj == DBNull.Value)
                    continue;

                string s = obj.ToString();
                if (s.Length > max)
                {
                    if (s.Length < short.MaxValue)
                        max = (short)s.Length;
                    else
                        return false;
                }
            }

            if (column.CType == CType.NChar || column.CType == CType.NVarChar)
                max += max;

            return max < column.Length;
        }

        private bool CanConvertToInt32(DataTable dt, IColumn column)
        {
            //ignore column with int
            if (column.CType == CType.Int)
                return false;

            string columnName = column.ColumnName;
            foreach (DataRow row in dt.Rows)
            {
                object obj = row[columnName];
                if (obj == DBNull.Value)
                    continue;

                string s = obj.ToString();
                if (!int.TryParse(s, out var a))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CanConvertBoolean(DataTable dt, IColumn column)
        {
            if (column.CType != CType.Int && column.CType != CType.BigInt && column.CType != CType.TinyInt)
                return false;

            string columnName = column.ColumnName;
            foreach (DataRow row in dt.Rows)
            {
                object obj = row[columnName];
                if (obj == DBNull.Value)
                    continue;

                long x = Convert.ToInt64(obj);

                if (x != 1 && x != 0)
                    return false;
            }

            return true;
        }


        private bool HasNull(DataTable dt, IColumn column)
        {
            string columnName = column.ColumnName;
            foreach (DataRow row in dt.Rows)
            {
                object obj = row[columnName];
                if (obj == DBNull.Value)
                    return true;
            }

            return false;
        }

        private DataTable FillDataTable(string sql)
        {
            ConnectionProvider provider = tname.Provider;
            return new SqlCmd(provider, sql).FillDataTable();
        }
    }
}

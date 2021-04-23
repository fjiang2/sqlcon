using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data
{
    public class UniqueTable
    {
        public readonly TableName TableName;
        public static string ROWID = "$RowId";

        private DataTable table;
        private List<byte[]> LOC = new List<byte[]>();
        private bool hasPhysloc = false;

        private DataColumn colLoc = null;
        private DataColumn colRowID = null;

        public UniqueTable(TableName tname, DataTable table)
        {
            this.TableName = tname;
            this.table = table;

            int i = 0;
            int I1 = -1;
            int I2 = -1;

            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName == SqlExpr.PHYSLOC)
                {
                    this.hasPhysloc = true;
                    colLoc = column;
                    I1 = i;
                }

                if (column.ColumnName == SqlExpr.ROWID)
                {
                    this.hasPhysloc = true;
                    colRowID = column;
                    I2 = i;
                }

                i++;
            }

            if (!hasPhysloc)
                return;

            i = 0;
            foreach (DataRow row in table.Rows)
            {
                LOC.Add((byte[])row[I1]);
                row[I2] = i++;
            }

            colRowID.ColumnName = ROWID;

            table.Columns.Remove(colLoc);
            table.AcceptChanges();
        }

        public bool HasPhysloc
        {
            get { return hasPhysloc; }
        }

        public DataTable Table { get { return this.table; } }

        public byte[] PhysLoc(int rowId)
        {
            if (rowId < 0 || rowId > LOC.Count - 1)
                throw new IndexOutOfRangeException("RowId is out of range");

            return LOC[rowId];
        }

        private int RowId(DataRow row)
        {
            return (int)row[colRowID];
        }

        public object this[DataColumn column, DataRow row]
        {
            get
            {
                return this[column.ColumnName, RowId(row)];
            }
            set
            {
                this[column.ColumnName, RowId(row)] = value;
            }
        }

        public object this[string column, int rowId]
        {
            get
            {
                return table.Rows[rowId][column];
            }
            set
            {
                var builder = WriteValue(column, rowId, value);
                new SqlCmd(builder).ExecuteNonQuery();
                table.AcceptChanges();
            }
        }

        public SqlBuilder WriteValue(string column, int rowId, object value)
        {
            table.Rows[rowId][column] = value;
            return UpdateClause(column, rowId, value);
        }

        private SqlBuilder UpdateClause(string column, int rowId, object value)
        {
            return new SqlBuilder().UPDATE(TableName).SET(column.Assign(value)).WHERE(PhysLoc(rowId));
        }

        public void UpdateCell(DataRow row, DataColumn column, object value)
        {
            if (column == colRowID)
                return;

            string col = column.ColumnName;
            int rowId = RowId(row);
            var builder = UpdateClause(col, rowId, value);
            new SqlCmd(builder).ExecuteNonQuery();
            row.AcceptChanges();
        }

        public void InsertRow(DataRow row)
        {
            List<string> columns = new List<string>();
            List<object> values = new List<object>();
            List<SqlExpr> where = new List<SqlExpr>();

            var _columns = TableName.GetTableSchema().Columns;

            foreach (DataColumn column in table.Columns)
            {
                object value = row[column];
                string name = column.ColumnName;
                IColumn _column = _columns[column.ColumnName];

                if (column == colRowID)
                    continue;

                if (value != DBNull.Value)
                {
                    columns.Add(name);
                    values.Add(value);

                    where.Add(name.Equal(value));
                }
                else if (!_column.Nullable)  //add default value to COLUMN NOT NULL
                {
                    Type type = _column.CType.ToType();
                    value = GetDefault(type);
                    columns.Add(name);
                    values.Add(value);

                    where.Add(name.Equal(value));
                }
            }

            var builder = new SqlBuilder().INSERT(TableName, columns.ToArray()).VALUES(values.ToArray());
            new SqlCmd(builder).ExecuteNonQuery();

            builder = new SqlBuilder().SELECT().COLUMNS(SqlExpr.PHYSLOC).FROM(TableName).WHERE(where.AND());
            var loc = new SqlCmd(builder).FillObject<byte[]>();
            LOC.Add(loc);

            row[colRowID] = table.Rows.Count - 1; //this will trigger events ColumnChanged or RowChanged

            row.AcceptChanges();
        }

        private static object GetDefault(Type type)
        {
            if (type == typeof(string))
                return string.Empty;
            else if (type == typeof(DateTime))
                return new DateTime();
            else if (type == typeof(bool))
                return false;
            else if (type == typeof(int))
                return 0;
            else if (type == typeof(double))
                return 0.0;

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }


        private DataColumnCollection columns = null;
        public DataColumnCollection Columns
        {
            get
            {
                if (columns == null)
                    columns = new DataColumnCollection(this.table);

                return columns;
            }
        }
    }

    public class DataColumnCollection
    {
        private DataTable table;
        private List<Column> columns = new List<Column>();

        internal DataColumnCollection(DataTable table)
        {
            this.table = table;

            foreach (DataColumn column in table.Columns)
            {
                columns.Add(new Column(this.table, column));
            }
        }

        public Column this[DataColumn column]
        {
            get
            {
                return columns.Find(c => c.DataColumn == column);
            }
        }


        public Column this[string column]
        {
            get
            {
                return columns.Find(c => c.DataColumn.ColumnName == column);
            }
        }
    }

    public class Column
    {
        private DataColumn column;
        private DataTable table;

        internal Column(DataTable table, DataColumn name)
        {
            this.table = table;
            this.column = name;
        }

        public DataColumn DataColumn
        {
            get { return this.column; }
        }

        public object this[int rowId]
        {
            get
            {
                return table.Rows[rowId][column];
            }
            set
            {
                table.Rows[rowId][column] = value;
            }
        }

        public object[] ItemArray
        {
            get
            {
                object[] objs = new object[table.Rows.Count];
                for (int i = 0; i < objs.Length; i++)
                    objs[i] = table.Rows[i][column];

                return objs;
            }
            set
            {
                object[] objs = value;
                for (int i = 0; i < objs.Length; i++)
                    table.Rows[i][column] = objs[i];
            }
        }

        public override string ToString()
        {
            return string.Format("Table Column Array: {0} size={1}", column.ColumnName, table.Rows.Count);
        }
    }
}

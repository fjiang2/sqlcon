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
        private const string PHYSLOC = "%%physloc%%";
        private const string ROWID = "%%RowId%%";

        public readonly TableName TableName;
        public const string ROWID_HEADER = "$RowId";

        private DataTable table;
        private List<byte[]> LOC = new List<byte[]>();
        private bool hasPhysloc = false;

        private DataColumn colLoc = null;
        private DataColumn colRowID = null;
        private string[] parimaryKeys;

        public UniqueTable(TableName tname, DataTable table)
        {
            this.TableName = tname;
            this.table = table;

            int i = 0;
            int I1 = -1;
            int I2 = -1;

            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName == PHYSLOC)
                {
                    this.hasPhysloc = true;
                    colLoc = column;
                    I1 = i;
                }

                if (column.ColumnName == ROWID)
                {
                    colRowID = column;
                    I2 = i;
                }

                i++;
            }

            this.parimaryKeys = tname.GetTableSchema().PrimaryKeys.Keys;

            if (I2 == -1)
                return;

            i = 0;
            foreach (DataRow row in table.Rows)
            {
                if (I1 != -1)
                    LOC.Add((byte[])row[I1]);

                if (I2 != -1)
                    row[I2] = i++;
            }

            colRowID.ColumnName = ROWID_HEADER;

            if (I1 != -1)
                table.Columns.Remove(colLoc);

            table.AcceptChanges();
        }

        public static string ROWID_COLUMN(TableName tname)
        {
            if (tname.Provider.DpType == DbProviderType.SqlDb)
                return $"{PHYSLOC} AS [{PHYSLOC}],0 AS [{ROWID}],";

            return $"0 AS [{ROWID}],";
        }


        public bool HasPhysloc => hasPhysloc;
        public DataTable Table => this.table;


        public SqlBuilder WriteValue(string column, int rowId, object value)
        {
            if (rowId < 0 || rowId > LOC.Count - 1)
                throw new IndexOutOfRangeException("RowId is out of range");

            DataRow row = table.Rows[rowId];
            row[column] = value;
            return UpdateClause(column, row, value);
        }

        public byte[] PhysLoc(int rowId)
        {

            return LOC[rowId];
        }

        private SqlBuilder UpdateClause(string column, DataRow row, object value)
        {
            SqlMaker gen = new SqlMaker(TableName.FormalName);

            if (hasPhysloc)
            {
                gen.PrimaryKeys = new string[] { PHYSLOC };
                int rowId = (int)row[colRowID];
                byte[] loc = LOC[rowId];
                gen.Add(PHYSLOC, loc);
                gen.Add(column, value);
            }
            else
            {
                gen.PrimaryKeys = parimaryKeys;
                gen.AddRange(row);
                gen.Add(column, value);
                gen.Remove(ROWID_HEADER);
            }

            gen.Update();
            return new SqlBuilder(TableName.Provider).Append(gen.Update());
        }

        public void UpdateCell(DataRow row, DataColumn column, object value)
        {
            if (column == colRowID)
                return;

            string col = column.ColumnName;
            var builder = UpdateClause(col, row, value);
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

            if (colLoc != null)
            {
                builder = new SqlBuilder().SELECT().COLUMNS(PHYSLOC).FROM(TableName).WHERE(where.AND());
                var loc = new SqlCmd(builder).FillObject<byte[]>();
                LOC.Add(loc);

            }

            row[colRowID] = table.Rows.Count - 1; //this will trigger events ColumnChanged or RowChanged
            row.AcceptChanges();
        }

        public void DeleteRow(DataRow row)
        {
            SqlMaker gen = new SqlMaker(TableName.FormalName);

            if (hasPhysloc)
            {
                gen.PrimaryKeys = new string[] { PHYSLOC };
                int rowId = (int)row[colRowID, DataRowVersion.Original];
                byte[] loc = LOC[rowId];
                gen.Add(PHYSLOC, loc);
            }
            else
            {
                gen.PrimaryKeys = parimaryKeys;
                gen.AddRange(row, DataRowVersion.Original);
            }

            string SQL = gen.Delete();

            new SqlCmd(TableName.Provider, SQL).ExecuteNonQuery();
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

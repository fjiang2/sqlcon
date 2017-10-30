using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;

namespace sqlcon
{
    class DuplicatedTable
    {
        public bool AllColumnsSelected { get; } = false;

        private TableName tname;
        private string[] _columns;
        const string COUNT_COLUMN_NAME = "$Count";
        public DataTable group { get; }

        public DuplicatedTable(TableName tname, string[] columns)
        {
            this.tname = tname;

            if (columns.Length == 0)
            {
                _columns = new TableSchema(tname)
                    .Columns
                    .Where(column => column.CType != CType.Image && column.CType != CType.Text && column.CType != CType.NText)
                    .Select(column => column.ColumnName)
                    .ToArray();
                AllColumnsSelected = true;
            }
            else
                this._columns = columns;

            var builder = new SqlBuilder()
                .SELECT
                .AppendFormat("COUNT(*) AS [{0}], ", COUNT_COLUMN_NAME)
                .COLUMNS(_columns)
                .FROM(tname)
                .GROUP_BY(_columns).Append("HAVING COUNT(*)>1 ")
                .ORDER_BY(_columns);

            group = builder.SqlCmd.FillDataTable();
        
        }

        public void Dispaly(Action<DataTable> display)
        {
            foreach (var row in group.AsEnumerable())
            {
                var where = _columns.Select(column => column.Equal(row[column])).AND();
                if (AllColumnsSelected)
                    cout.WriteLine("idential rows");
                else
                    cout.WriteLine("{0}", where);

                var builder = new SqlBuilder().SELECT.COLUMNS().FROM(tname).WHERE(where);
                display(builder.SqlCmd.FillDataTable());
                cout.WriteLine();
            }
        }

        public int DuplicatedRowCount()
        {
            int sum = 0;
            foreach (var row in group.AsEnumerable())
            {
                int count = row.Field<int>(COUNT_COLUMN_NAME);
                sum += count - 1;
            }

            return sum;
        }


        public int Clean()
        {
            int sum = 0;
            foreach (var row in group.AsEnumerable())
            {
                int count = row.Field<int>(COUNT_COLUMN_NAME);

                var where = _columns.Select(column => column.Equal(row[column])).AND();
                var builder = new SqlBuilder()
                    .SET("ROWCOUNT", count-1)
                    .DELETE(tname)
                    .WHERE(where)
                    .SET("ROWCOUNT", 0);

                sum += count - 1;
                builder.SqlCmd.ExecuteNonQuery();
            }

            return sum;
        }
    }
}

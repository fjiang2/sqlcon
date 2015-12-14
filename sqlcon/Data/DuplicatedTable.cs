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
                _columns = new TableSchema(tname).Columns.Select(column => column.ColumnName).ToArray();
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
                var where = _columns.Select(column => column.Assign(row[column])).AND();
                if (AllColumnsSelected)
                    stdio.WriteLine("idential rows");
                else
                    stdio.WriteLine("{0}", where);

                var builder = new SqlBuilder().SELECT.COLUMNS().FROM(tname).WHERE(where);
                display(builder.SqlCmd.FillDataTable());
                stdio.WriteLine();
            }
        }

        public int Clean()
        {
            int sum = 0;
            foreach (var row in group.AsEnumerable())
            {
                int count = row.Field<int>(COUNT_COLUMN_NAME);

                var where = _columns.Select(column => column.Assign(row[column])).AND();
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

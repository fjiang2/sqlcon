using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data.Comparison
{
    class ColumnPairCollection : List<ColumnPair>
    {
        public ColumnPairCollection()
        {
        }

        public ColumnPairCollection(DataRow row)
        {
            AddRange(row);
        }
        public ColumnPairCollection(string[] columnName, object[] values)
        {
            AddRange(columnName, values);
        }

        public void AddRange(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                if (row[column] != DBNull.Value)
                    this.Add(new ColumnPair(column.ColumnName, row[column]));
            }

        }


        public void AddRange(string[] columnName, object[] values)
        {
            for (int i = 0; i < columnName.Length; i++)
            {
                if (values[i] != DBNull.Value)
                    this.Add(new ColumnPair(columnName[i], values[i]));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data.Comparison
{
    class RowCompare
    {
        private TableCompare table;
        private List<ColumnPair> L1;
        private List<ColumnPair> L2;

        public RowCompare(TableCompare table, DataRow row1, DataRow row2)
        {
            this.table = table;
            Difference(row1, row2);
        }

        private void Difference(DataRow row1, DataRow row2)
        {

            L1 = new List<ColumnPair>();
            L2 = new List<ColumnPair>();

            foreach (var column in table.CompareColumns)
            {
                var r1 = row1[column];
                var r2 = row2[column];

                if (r1 is string)   //compare string with postfix ' ' character
                    r1 = (r1 as string).Trim();

                if (r2 is string)
                    r2 = (r2 as string).Trim();

                if (!r1.Equals(r2))
                    L2.Add(new ColumnPair(column, r1));
            }

            foreach (var column in table.PkColumns.Keys)
            {
                L1.Add(new ColumnPair(column, row1[column]));
            }
        }

        public string UPDATE(TableName tableName)
        {
            string Set = string.Join<ColumnPair>(", ", L2);
            string where = string.Join<ColumnPair>(" AND ", L1);
            return new SqlTemplate(tableName).Update(Set, where);
        }

        public static bool Compare(string[] columns, DataRow row1, DataRow row2)
        {
            foreach (var column in columns)
            {
                if (row1[column] is byte[] && row2[column] is byte[])
                {
                    var B1 = (byte[])row1[column];
                    var B2 = (byte[])row2[column];
                    if (B1.Length != B2.Length)
                        return false;

                    for (int i = 0; i < B1.Length; i++)
                    {
                        if (B1[i] != B2[i])
                            return false;
                    }
                }
                else if (row1[column] is string && row2[column] is string)
                {
                    var r1 = row1[column];
                    var r2 = row2[column];

                    if (r1 is string)   //compare string with postfix ' ' character
                        r1 = (r1 as string).Trim();

                    if (r2 is string)
                        r2 = (r2 as string).Trim();

                    if (!r1.Equals(r2))
                        return false;
                }
                else if (!row1[column].Equals(row2[column]))
                    return false;
            }

            return true;
        }

    }
}

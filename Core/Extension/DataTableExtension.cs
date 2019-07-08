using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data
{
    public static class DataTableExtension
    {
        public static DataColumn[] PrimaryKeys(this DataTable dt, PrimaryKeys pk)
        {
            return PrimaryKeys(dt, pk.Keys);
        }

        public static DataColumn[] PrimaryKeys(this DataTable dt, string[] keys)
        {
            DataColumn[] primaryKey = dt.Columns
                .Cast<DataColumn>()
                .Where(column => keys.Select(key => key.ToUpper()).Contains(column.ColumnName.ToUpper()))
                .ToArray();

            dt.PrimaryKey = primaryKey;
            return primaryKey;
        }
    }
}

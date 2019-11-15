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
        public static DataColumn[] dentityKeys(this DataTable dt, IdentityKeys keys)
        {
            return GetDataColumns(dt, keys.ColumnNames);
        }
        public static DataColumn[] ForeignKeys(this DataTable dt, IForeignKeys keys)
        {
            return GetDataColumns(dt, keys.Keys.Select(x => x.FK_Column));
        }

        public static DataColumn[] PrimaryKeys(this DataTable dt, IPrimaryKeys keys)
        {
            return PrimaryKeys(dt, keys.Keys);
        }

        public static DataColumn[] PrimaryKeys(this DataTable dt, string[] keys)
        {
            DataColumn[] primaryKey = GetDataColumns(dt, keys);

            dt.PrimaryKey = primaryKey;
            return primaryKey;
        }

        public static DataColumn[] GetDataColumns(this DataTable dt, IEnumerable<string> columnNames)
        {
            var L = columnNames.Select(key => key.ToUpper());

            DataColumn[] _columns = dt.Columns
                .Cast<DataColumn>()
                .Where(column => L.Contains(column.ColumnName.ToUpper()))
                .ToArray();

            return _columns;
        }
    }
}

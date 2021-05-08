using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data
{
    public static class DataEnumerable
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            var properties = typeof(T).GetProperties();

            DataTable dt = new DataTable();
            foreach (var propertyInfo in properties)
            {
                dt.Columns.Add(new DataColumn(propertyInfo.Name, propertyInfo.PropertyType));
            }

            Func<T, object[]> selector = row =>
            {
                var values = new object[properties.Length];
                int i = 0;

                foreach (var propertyInfo in properties)
                {
                    values[i++] = propertyInfo.GetValue(row);
                }

                return values;
            };

            foreach (T row in source)
            {
                object[] values = selector(row);
                var newRow = dt.NewRow();
                int k = 0;
                foreach (var item in values)
                {
                    newRow[k++] = item;
                }

                dt.Rows.Add(newRow);
            }

            dt.AcceptChanges();
            return dt;
        }

        public static DataTable ToTable<T>(this IEnumerable<T> records) where T : class, IDPObject, new()
        {
            DPList<T> list = new DPList<T>(records);
            return list.Table;
        }

        public static DPList<T> ToDPList<T>(this IEnumerable<T> collection) where T : class, IDPObject, new()
        {
            return new DPList<T>(collection);
        }
    }
}

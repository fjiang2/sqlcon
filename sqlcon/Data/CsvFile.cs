using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace sqlcon
{
    class CsvFile
    {
        const char QUOTATION_MARK = '\"';

        public static void Write(DataTable table, TextWriter writer, bool includeHeaders)
        {
            if (includeHeaders)
            {
                IEnumerable<String> headers = table.Columns
                    .OfType<DataColumn>()
                    .Select(column => quote(string.Concat("[", column.ColumnName, "]")));

                writer.WriteLine(string.Join(",", headers));
            }

            IEnumerable<String> items = null;

            foreach (DataRow row in table.Rows)
            {
                items = row.ItemArray.Select(o => quote(o.ToString()));
                writer.WriteLine(string.Join(",", items));
            }

            writer.Flush();
        }

        private static string quote(string value)
        {
            if (value == null)
                return null;
                
            if (value.IndexOf(QUOTATION_MARK) >= 0)
                return string.Concat(QUOTATION_MARK, value.Replace("\"", "\"\""), QUOTATION_MARK);

            if (value.IndexOf(',') >= 0)
                return string.Concat(QUOTATION_MARK, value, QUOTATION_MARK);


            return value;
        }
    }
}

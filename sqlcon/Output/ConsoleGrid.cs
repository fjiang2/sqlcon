using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;


namespace sqlcon
{
    static class ConsoleGrid
    {
        public static void ToConsole<T>(this IEnumerable<T> source, bool vertical = false)
        {
            DataTable dt = ToDataTable(source);
            new OutputDataTable(dt, cout.TrimWriteLine, vertical).Output();
        }

        private static DataTable ToDataTable<T>(IEnumerable<T> source)
        {
            var properties = typeof(T).GetProperties();
            string[] headers = properties.Select(p => p.Name).ToArray();
            DataTable dt = new DataTable();
            foreach (var header in headers)
                dt.Columns.Add(new DataColumn(header, typeof(string)));

            Func<T, object[]> selector = row =>
            {
                var values = new object[headers.Length];
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

            return dt;
        }

        public static void ToConsole(this DbDataReader reader, int maxRow = 0)
        {
            while (reader.HasRows)
            {
                DataTable schemaTable = reader.GetSchemaTable();

                var schema = schemaTable
                    .AsEnumerable()
                    .Select(row => new
                    {
                        Name = row.Field<string>("ColumnName"),
                        Size = row.Field<int>("ColumnSize"),
                        Type = row.Field<Type>("DataType")
                    });

                string[] headers = schema.Select(row => row.Name).ToArray();

                var D = new OutputDataLine(cout.TrimWriteLine, headers.Length);

                D.MeasureWidth(schema.Select(row => row.Size).ToArray());
                D.MeasureWidth(headers);
                D.MeasureWidth(schema.Select(row => row.Type).ToArray());

                D.DisplayLine();
                D.DisplayLine(headers);
                D.DisplayLine();

                if (!reader.HasRows)
                {
                    cout.WriteLine("<0 row>");
                    return;
                }

                object[] values = new object[headers.Length];
                int count = 0;
                bool limited = false;
                while (reader.Read())
                {
                    reader.GetValues(values);
                    D.DisplayLine(values);

                    if (++count == maxRow)
                    {
                        limited = true;
                        break;
                    }

                }

                D.DisplayLine();
                cout.WriteLine("<{0} row{1}> {2}",
                    count,
                    count > 1 ? "s" : "",
                    limited ? "limit reached" : ""
                    );

                reader.NextResult();
            }

        }

        public static void ToConsole(this DataTable dt, bool vertical = false, bool more = false, bool outputDbNull = true)
        {
            ShellHistory.SetLastResult(dt);
            OutputDataTable odt = new OutputDataTable(dt, cout.TrimWriteLine, vertical)
            {
                OutputDbNull = outputDbNull,
            };
            odt.Output();

            cout.WriteLine("<{0}{1} row{2}>", more ? "top " : "", dt.Rows.Count, dt.Rows.Count > 1 ? "s" : "");
        }



    }


}

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
        public static void ToConsole<T>(this IEnumerable<T> source)
        {
            OutputCollection<T> oc = new OutputCollection<T>(source, cout.TrimWriteLine, vertical: false);
            oc.WriteData();
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

        public static void ToConsole(this DataTable table, bool vertical = false, bool more = false)
        {
            ShellHistory.SetLastResult(table);
            OutputDataTable cdt = new OutputDataTable(table, cout.TrimWriteLine, vertical);
            cdt.WriteData();

            cout.WriteLine("<{0}{1} row{2}>", more ? "top " : "", table.Rows.Count, table.Rows.Count > 1 ? "s" : "");
        }



    }


}

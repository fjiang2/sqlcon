using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace sqlcon
{
    static class  ConsoleGrid
    {
	  public static void ToConsole<T>(this IEnumerable<T> source)
        {
            var properties = typeof(T).GetProperties();
            string[] headers = properties.Select(p => p.Name).ToArray();
            
            Func<T, object[]> selector = row =>
                {
                    var values = new object[headers.Length];
                    int i=0;
                    
                    foreach (var propertyInfo in properties)
                    {
                        values[i++] = propertyInfo.GetValue(row);
                    }
                    return values;
                };

            source.ToConsole(headers, selector);
        }

        public static void ToConsole<T>(this IEnumerable<T> source, string[] headers, Func<T, object[]> selector)
        {

            var D = new ConsoleTable(headers.Length);

            D.MeasureWidth(headers);
            foreach (var row in source)
            {
                D.MeasureWidth(selector(row));
            }

            D.DisplayLine();
            D.DisplayLine(headers);
            D.DisplayLine();

            if (source.Count() == 0)
                return;

            foreach (var row in source)
            {
                D.DisplayLine(selector(row));
            }

            D.DisplayLine();
        }

        public static void ToConsole(this DbDataReader reader, int maxRow = 0)
        {
            while (reader.HasRows)
            {
                DataTable schemaTable = reader.GetSchemaTable();

                var schema = schemaTable
                    .AsEnumerable()
                    .Select(row => new {
                        Name = row.Field<string>("ColumnName"),
                        Size = row.Field<int>("ColumnSize"),
                        Type = row.Field<Type>("DataType")
                    });

                string[] headers = schema.Select(row => row.Name).ToArray();

                var D = new ConsoleTable(headers.Length);

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


        public static void ToConsole(this DataTable table, bool more = false)
        {
            ShellHistory.SetLastResult(table);

            List<string> list = new List<string>();
            foreach (DataColumn column in table.Columns)
                list.Add(column.ColumnName);

            string[] headers = list.ToArray();

            var D = new ConsoleTable(headers.Length);

            D.MeasureWidth(headers);
            foreach (DataRow row in table.Rows)
            {
                D.MeasureWidth(row.ItemArray);
            }

            D.DisplayLine();
            D.DisplayLine(headers);
            D.DisplayLine();

            if (table.Rows.Count == 0)
                return;

            foreach (DataRow row in table.Rows)
            {
                D.DisplayLine(row.ItemArray);
            }

            D.DisplayLine();

            cout.WriteLine("<{0}{1} row{2}>", more ? "top " : "", table.Rows.Count,  table.Rows.Count > 1 ? "s" : "");
        }


        public static void ToVConsole<T>(this IEnumerable<T> source)
        {
            var properties = typeof(T).GetProperties();
            string[] headers = properties.Select(p => p.Name).ToArray();

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

            source.ToVConsole(headers, selector);
        }


        public static void ToVConsole<T>(this IEnumerable<T> source, string[] headers, Func<T, object[]> selector)
        {
            int m = 1;
            int n = headers.Length;

            var D = new ConsoleTable(m + 1);

            object[] L = new object[m + 1];
            T[] src = source.ToArray();

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                L[k++] = src[i];

                D.MeasureWidth(L);
            }

            D.DisplayLine();

            if (source.Count() == 0)
                return;

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                L[k++] = src[i];

                D.DisplayLine(L);
            }

            D.DisplayLine();
        }


        public static void ToVConsole(this DataTable table, bool more)
        {

            List<string> list = new List<string>();
            foreach (DataColumn column in table.Columns)
                list.Add(column.ColumnName);

            string[] headers = list.ToArray();

            int m = table.Rows.Count;
            int n = table.Columns.Count;

            var D = new ConsoleTable(m + 1);

            object[] L = new object[m + 1];

            for (int i = 0; i < n; i++)
            {
                int k=0;
                L[k++] = headers[i];
                foreach (DataRow row in table.Rows)
                    L[k++] = row[i];
                
                D.MeasureWidth(L);
            }

            D.DisplayLine();

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                foreach (DataRow row in table.Rows)
                    L[k++] = row[i];

                D.DisplayLine(L);
            }

            D.DisplayLine();
            cout.WriteLine("<{0}{1} row{2}>", more ? "top " : "", table.Rows.Count, table.Rows.Count > 1 ? "s" : "");
        }

    }
}

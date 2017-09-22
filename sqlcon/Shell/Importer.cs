using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Sys;
using Sys.Data;

namespace sqlcon
{
    class Importer
    {
        public static int ImportCsv(string path, TableName tname, string[] columns)
        {
            int count = 0;

            //create column schema list
            TableSchema schema = new TableSchema(tname);
            List<IColumn> list = new List<IColumn>();
            foreach (string column in columns)
            {
                var c = schema.Columns.FirstOrDefault(x => x.ColumnName.ToUpper() == column.ToUpper());
                if (c == null)
                {
                    stdio.Error($"cannot find the column [{column}]");
                    return count;
                }

                list.Add(c);
            }

            var _columns = list.ToArray();

            //read .csv file
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    object[] values = parseLine(_columns, line);

                    var builder = new SqlBuilder().INSERT(tname, columns).VALUES(values);
                    try
                    {
                        new SqlCmd(builder).ExecuteNonQuery();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        stdio.Error(ex.AllMessage(builder.ToString()));
                        return count;
                    }

                    count++;
                }
            }

            return count;
        }

        private static object[] parseLine(IColumn[] columns, string line)
        {
            string[] items = line.Split(',');
            object[] values = new object[columns.Length];
            for (int i = 0; i < items.Length; i++)
            {
                values[i] = (columns[i] as ColumnSchema).Parse(items[i]);
            }

            return values;
        }
    }
}

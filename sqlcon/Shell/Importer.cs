using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Sys.Data;

namespace sqlcon
{
    class Importer
    {
        public static int ImportCsv(string path, TableName tname, string[] columns)
        {
            string L = string.Empty;
            if (columns.Length > 0)
                L = columns.Select(x => $"[{x}]").Aggregate((x, y) => $"{x},{y}");

            int count = 0;
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] items = line.Split(',');
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (DateTime.TryParse(items[i], out var _))
                            items[i] = $"'{items[i]}'";
                    }

                    line = string.Join(",", items);
                    string sql;
                    if (columns.Length == 0)
                        sql = $"INSERT INTO {tname} VALUES({line})";
                    else
                    {
                        sql = $"INSERT INTO {tname} ({L}) VALUES({line})";
                    }

                    new SqlCmd(tname.Provider, sql).ExecuteNonQuery();
                    count++;
                }
            }

            return count;
        }
    }
}

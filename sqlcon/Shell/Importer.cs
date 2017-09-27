using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Sys;
using Sys.Data;
using Tie;

namespace sqlcon
{
    class Importer
    {
        private Command cmd;
        public Importer(Command cmd)
        {
            this.cmd = cmd;
        }

        public int ImportCsv(string path, TableName tname, string[] columns)
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


        public int ImportCfg(string path, TableName tname)
        {
            int count = 0;
            if (!File.Exists(path))
            {
                stdio.Error($"file {path} not found");
                return 0;
            }

            string code = File.ReadAllText(path);
            Memory DS = new Memory();
            Script.Execute(code, DS);

            //create key-value pairs
            Dictionary<string, VAL> dict = new Dictionary<string, VAL>();
            foreach (VAR var in DS.Names)
            {
                VAL val = DS[var];
                createKeyValues(dict, tname, string.Empty, (string)var, val);
            }


            //prepare default values of NOT NULL columns when record inserted
            string[] columns = cmd.Columns;
            string _not_null_keys = string.Empty;
            string _not_null_values = string.Empty;
            if (columns.Length > 0)
            {
                var pairs = columns
                    .Select(c => c.Split('='))
                    .Select(c => new { Key = c[0], Value = c[1] });
                _not_null_keys = "," + string.Join(",", pairs.Select(p => p.Key));
                _not_null_values = "," + string.Join(",", pairs.Select(p => p.Value));
            }

            //execute IF EXISTS UPDATE ELSE INSERT
            foreach (var kvp in dict)
            {
                string sql = createSQL(tname, kvp.Key, kvp.Value, _not_null_keys, _not_null_values);
                try
                {
                    new SqlCmd(tname.Provider, sql).ExecuteNonQuery();
                    count++;
                }
                catch (Exception ex)
                {
                    stdio.Error(ex.Message);
                    break;
                }
            }

            return count;
        }

        private string createSQL(TableName tname, string var, VAL val, string keys, string values)
        {
            string colKey = cmd.GetValue("key") ?? "Key";
            string colValue = cmd.GetValue("value") ?? "Value";

            string _colKey = $"[{colKey}]";
            string _colValue = $"[{colValue}]";

            string _tname = tname.ShortName;
            string _var = $"'{var}'";
            string _val = $"'{val.ToString()}'";

            string sql =
$@"IF EXISTS(SELECT * FROM {_tname} WHERE {_colKey} = {_var}) 
  UPDATE {_tname}  SET {_colValue} = {_val} WHERE {_colKey}={_var} 
ELSE 
  INSERT {_tname} ({_colKey},{_colValue}{keys}) VALUES({_var},{_val}{values})
";

            return sql;
        }

        private void createKeyValues(Dictionary<string, VAL> dict, TableName tname, string prefix, string key, VAL val)
        {
            if (val.IsAssociativeArray())
            {
                if (prefix == string.Empty)
                    prefix = key;
                else
                    prefix = $"{prefix}.{key}";

                foreach (var member in val.Members)
                {
                    createKeyValues(dict, tname, prefix, member.Name, member.Value);
                    continue;
                }

                return;
            }

            string var = $"{prefix}.{key}";
            if (prefix == string.Empty)
                var = key;

            dict.Add(var, val);
        }
    }
}

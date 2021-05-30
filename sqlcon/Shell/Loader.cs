using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Sys;
using Sys.Data;
using Sys.Stdio;
using Tie;

namespace sqlcon
{
    class Loader
    {
        private ApplicationCommand cmd;
        public Loader(ApplicationCommand cmd)
        {
            this.cmd = cmd;
        }

        public int LoadCsv(string path, TableName tname, string[] columns)
        {
            //create column schema list
            TableSchema schema = new TableSchema(tname);
            IColumn[] _columns;
            try
            {
                _columns = GetColumnSchema(schema, columns);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
                return 0;
            }

            //read .csv file
            int count = 0;
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    //Processor header
                    if (count == 0)
                    {
                        string[] __columns = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        try
                        {
                            _columns = GetColumnSchema(schema, __columns);
                            if (_columns.Length != 0)
                            {
                                count++;
                                continue;
                            }
                        }
                        catch (Exception)
                        {
                        }

                        if (_columns.Length == 0)
                            _columns = schema.Columns.ToArray();
                    }

                    object[] values = parseLine(_columns, line);
                    if (values == null)
                        return count;

                    var builder = new SqlBuilder().INSERT(tname, columns).VALUES(values);
                    try
                    {
                        new SqlCmd(builder).ExecuteNonQuery();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        cerr.WriteLine(ex.AllMessages(builder.ToString()));
                        return count;
                    }

                    count++;
                }
            }

            return count;
        }

        private static IColumn[] GetColumnSchema(TableSchema schema, string[] columns)
        {
            List<IColumn> list = new List<IColumn>();
            foreach (string column in columns)
            {
                var c = schema.Columns.FirstOrDefault(x => x.ColumnName.ToUpper() == column.ToUpper());
                if (c == null)
                {
                    throw new Exception($"cannot find the column [{column}]");
                }

                list.Add(c);
            }

            return list.ToArray();

        }

        private static object[] parseLine(IColumn[] columns, string line)
        {
            string[] items = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length > columns.Length)
            {
                cerr.WriteLine($"#columns({items.Length}) on .csv > #column({columns.Length}) on database, {line}");
                return null;
            }

            object[] values = new object[columns.Length];
            for (int i = 0; i < items.Length; i++)
            {
                try
                {
                    values[i] = (columns[i] as ColumnSchema).Parse(items[i]);
                }
                catch (Exception ex)
                {
                    cerr.WriteLine($"cannot parse {items[i]} on column {columns[i]}, {ex.Message}");
                }
            }

            return values;
        }


        public int LoadCfg(string path, TableName tname)
        {
            int count = 0;
            if (!File.Exists(path))
            {
                cerr.WriteLine($"file {path} not found");
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
                    cerr.WriteLine(ex.Message);
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

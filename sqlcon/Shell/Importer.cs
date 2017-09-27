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

            List<string> statements = new List<string>();
            foreach (VAR var in DS.Names)
            {
                VAL val = DS[var];
                createSQL(statements, tname, string.Empty, (string)var, val);
            }

            //string _sql = string.Join(Environment.NewLine, statements);

            foreach (string sql in statements)
            {
                new SqlCmd(tname.Provider, sql).ExecuteNonQuery();
                count++;
            }

            return count;
        }

        private string createSQL(TableName tname, string var, string val)
        {
            string colKey = cmd.GetValue("key") ?? "Key";
            string colValue = cmd.GetValue("value") ?? "Value";

            string _tname = tname.ShortName;
            string sql =
$@"IF EXISTS(SELECT * FROM {_tname} WHERE [{colKey}] = '{var}') 
  UPDATE {_tname}  SET [{colValue}] = '{val}' WHERE [Key]='{var}' 
ELSE 
  INSERT {_tname} ([{colKey}],[{colValue}]) VALUES('{var}','{val}')
";

            return sql;
        }

        private void createSQL(List<string> statements, TableName tname, string prefix, string key, VAL val)
        {
            if (val.IsAssociativeArray())
            {
                if (prefix == string.Empty)
                    prefix = key;
                else
                    prefix = $"{prefix}.{key}";

                foreach (var member in val.Members)
                {
                    createSQL(statements, tname, prefix, member.Name, member.Value);
                    continue;
                }

                return;
            }

            string var = $"{prefix}.{key}";
            if (prefix == string.Empty)
                var = key;

            string code = createSQL(tname, var, val.ToString());
            statements.Add(code);
        }
    }
}

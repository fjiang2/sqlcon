using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Sys.Data;
using Tie;

namespace sqlcon
{
    public static class DataExtension
    {
        public static string ToJson(this DataLake lake)
        {
            VAL val = new VAL();
            foreach (var kvp in lake)
            {
                DataSet ds = kvp.Value;
                VAL _ds = new VAL();
                foreach (DataTable dt in ds.Tables)
                {
                    var _dt = toJsonValueArray(dt);
                    _ds.AddMember(dt.TableName, _dt);
                }

                val.AddMember(kvp.Key, _ds);
            }

            return val.ToJson();
        }

        public static string ToJson(this DataSet ds)
        {
            VAL val = new VAL();
            foreach (DataTable dt in ds.Tables)
            {
                var _dt = toJsonValueArray(dt);
                val.AddMember(dt.TableName, _dt);
            }

            return val.ToJson();
        }

        public static string ToJson(this DataTable dt)
        {
            if (dt.Columns.Count == 1)
            {
                string json = VAL.Boxing(dt.ToArray(row => row[0])).ToJson();
                return json;
            }

            VAL _dt = toJsonValueArray(dt);
            VAL val = new VAL();
            val.AddMember(dt.TableName, _dt);
            return val.ToJson();
        }

        public static DataLake ToDataLake(this string json)
        {
            VAL val = Script.Evaluate(json);
            return toDataLake(val);
        }

        public static DataSet ToDataSet(this string json)
        {
            VAL val = Script.Evaluate(json);
            return toDataSet(val);
        }

        public static DataTable ToDataTable(this string json)
        {
            VAL val = Script.Evaluate(json);
            return toDataTable(val);
        }

        private static VAL toJsonValueArray(DataTable dt)
        {
            string[] columns = dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();
            VAL L = new VAL();
            foreach (DataRow row in dt.Rows)
            {
                VAL V = new VAL();
                for (int i = 0; i < columns.Length; i++)
                {
                    object obj;
                    switch (row[i])
                    {
                        case Guid x:
                            obj = "{" + x.ToString() + "}";
                            break;

                        case DBNull NULL:
                            obj = null;
                            break;

                        default:
                            obj = row[i];
                            break;
                    }

                    V.AddMember(columns[i], obj);
                }
                L.Add(V);
            }

            return L;
        }

        private static DataTable toDataTable(VAL val)
        {
            DataTable dt = new DataTable();

            Dictionary<string, Type> dict = new Dictionary<string, Type>();
            for (int i = 0; i < val.Size; i++)
            {
                VAL line = val[i];
                for (int k = 0; k < line.Size; k++)
                {
                    VAL member = line[k];
                    if (member.Size < 2)
                        throw new Exception($"invalid key-value pair: {member}");

                    string key = member[0].ToSimpleString();
                    object value = member[1].HostValue;

                    Type type = null;
                    if (value != null)
                        type = value.GetType();

                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, type);
                    }
                    else
                    {
                        Type stocked = dict[key];
                        if (stocked == null && type != null)
                        {
                            dict[key] = type;
                        }
                    }
                }
            }

            foreach (var kvp in dict)
            {
                Type type = kvp.Value;

                if (type == null)
                    type = typeof(string);  //value of entire column is NULL

                dt.Columns.Add(new DataColumn(kvp.Key, type));
            }

            for (int i = 0; i < val.Size; i++)
            {
                VAL line = val[i];
                DataRow newRow = dt.NewRow();
                for (int k = 0; k < line.Size; k++)
                {
                    VAL member = line[k];
                    string key = member[0].ToSimpleString();
                    object value = member[1].HostValue;
                    if (value == null)
                        newRow[key] = DBNull.Value;
                    else
                        newRow[key] = value;
                }
                dt.Rows.Add(newRow);
            }

            return dt;
        }

        private static DataSet toDataSet(VAL val)
        {
            DataSet ds = new DataSet();
            for (int i = 0; i < val.Size; i++)
            {
                VAL line = val[i];
                DataTable dt = toDataTable(line[1]);
                dt.TableName = line[0].ToSimpleString();
                ds.Tables.Add(dt);
            }

            return ds;
        }

        private static DataLake toDataLake(VAL val)
        {
            DataLake lake = new DataLake();
            for (int i = 0; i < val.Size; i++)
            {
                VAL line = val[i];
                DataSet ds = toDataSet(line[1]);
                ds.DataSetName = line[0].ToSimpleString();
                lake.Add(ds.DataSetName, ds);
            }

            return lake;
        }


        public static DataSet ReadDataSet(this string path)
        {
            string ext = Path.GetExtension(path);
            try
            {
                switch (ext)
                {
                    case ".json":
                        string json = File.ReadAllText(path);
                        return ToDataSet(json);

                    case ".xml":
                        var ds = new DataSet();
                        ds.ReadXml(path, XmlReadMode.ReadSchema);
                        return ds;

                    case ".cs":
                        var csc = new Sys.Compiler.CSharpCompiler();
                        string cs = File.ReadAllText(path);
                        csc.Compile("temp.dll", cs);
                        if (csc.HasError)
                        {
                            cerr.WriteLine(csc.GetError());
                            return null;
                        }
                        return new Sys.Compiler.DataSetBuilder(csc.GetAssembly()).ToDataSet();
                }
            }
            catch (Exception ex)
            {
                cerr.WriteLine($"invalid data set file: {path}, {ex.Message}");
            }

            return null;
        }

        public static void WriteDataSet(this string path, DataSet ds)
        {
            string directory = Path.GetDirectoryName(path);
            if (directory != string.Empty)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }

            if (Path.GetExtension(path) == string.Empty)
                path = Path.ChangeExtension(path, ".xml");

            if (Path.GetExtension(path) == ".json")
            {
                string json = ToJson(ds);
                File.WriteAllText(path, json);
            }
            else
            {
                ds.WriteXml(path, XmlWriteMode.WriteSchema);
            }
        }


    }
}

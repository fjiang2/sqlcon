using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using Tie;
using Sys.Data;

namespace sqlcon
{
    static class JsonExtension
    {
        public static string ToJson(this DataSet ds)
        {
            VAL val = new VAL();
            foreach (DataTable dt in ds.Tables)
            {
                var _dt = ToJsonValueArray(dt);
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

            VAL _dt = ToJsonValueArray(dt);
            VAL val = new VAL();
            val.AddMember(dt.TableName, _dt);
            return val.ToJson();
        }

        private static VAL ToJsonValueArray(DataTable dt)
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

        private static DataTable ToDataTable(VAL val)
        {
            DataTable dt = new DataTable();

            Dictionary<string, Type> dict = new Dictionary<string, Type>();
            for (int i = 0; i < val.Size; i++)
            {
                VAL line = val[i];
                for (int k = 0; k < line.Size; k++)
                {
                    VAL member = line[k];
                    string key = member[0].ToString();
                    object value = member[1].HostValue;

                    Type type = typeof(string);
                    if (value != null)
                        type = value.GetType();

                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, type);
                    }
                    else
                    {
                        Type stocked = dict[key];
                        if (type != stocked)
                            dict[key] = type;
                    }
                }
            }

            foreach (var kvp in dict)
            {
                dt.Columns.Add(new DataColumn(kvp.Key, kvp.Value));
            }

            for (int i = 0; i < val.Size; i++)
            {
                VAL line = val[i];
                DataRow newRow = dt.NewRow();
                for (int k = 0; k < line.Size; k++)
                {
                    VAL member = line[k];
                    string key = member[0].ToString();
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

        public static DataSet ToDataSet(VAL val)
        {
            DataSet ds = new DataSet();
            for (int i = 0; i < val.Size; i++)
            {
                VAL line = val[i];
                DataTable dt = ToDataTable(line[1]);
                dt.TableName = line[0].ToString();
                ds.Tables.Add(dt);
            }

            return ds;
        }

        public static DataSet ReadJson(string path)
        {
            string json = File.ReadAllText(path);
            VAL val = Script.Evaluate(json);
            return ToDataSet(val);
        }
    }
}

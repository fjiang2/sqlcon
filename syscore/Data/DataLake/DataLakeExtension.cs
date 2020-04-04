using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Sys.Data;
using Tie;

namespace Sys.Data
{

    public static class DataLakeExtension
    {
        private static string ToJson(JsonStyle style, VAL val)
        {
            switch (style)
            {
                case JsonStyle.Coded:
                    return val.ToString();

                case JsonStyle.Extended:
                    return val.ToExJson();
            }

            return val.ToJson();
        }

        public static string WriteJson(this DataLake lake, JsonStyle style)
        {
            VAL val = new VAL();
            foreach (var kvp in lake)
            {
                DataSet ds = kvp.Value;
                VAL _ds = new VAL();
                foreach (DataTable dt in ds.Tables)
                {
                    var _dt = WriteVAL(dt, style);
                    _ds.AddMember(dt.TableName, _dt);
                }

                val.AddMember(kvp.Key, _ds);
            }

            return ToJson(style, val);
        }

        public static string WriteJson(this DataSet ds, JsonStyle style)
        {
            VAL val = new VAL();
            foreach (DataTable dt in ds.Tables)
            {
                var _dt = WriteVAL(dt, style);
                val.AddMember(dt.TableName, _dt);
            }

            return ToJson(style, val);
        }

        public static string WriteJson(this DataTable dt, JsonStyle style)
        {
            if (dt.Columns.Count == 1)
            {
                string json = ToJson(style, VAL.Boxing(dt.ToArray(row => row[0])));
                return json;
            }

            VAL _dt = WriteVAL(dt, style);
            VAL val = new VAL();
            val.AddMember(dt.TableName, _dt);

            return ToJson(style, val);
        }

        private static VAL WriteVAL(DataTable dt, JsonStyle style)
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


        public static void ReadJson(this DataLake lake, string json)
        {
            VAL val = Script.Evaluate(json);
            ReadVAL(lake, val);
        }

        public static void ReadJson(this DataSet ds, string json)
        {
            VAL val = Script.Evaluate(json);
            ReadVAL(ds, val);
        }

        public static void ReadJson(this DataTable dt, string json)
        {
            VAL val = Script.Evaluate(json);
            ReadVAL(dt, val);
        }


        private static void ReadVAL(DataTable dt, VAL val)
        {
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

        }

        private static void ReadVAL(DataSet ds, VAL val)
        {
            for (int i = 0; i < val.Size; i++)
            {
                VAL line = val[i];
                DataTable dt = new DataTable();
                ReadVAL(dt, line[1]);
                dt.TableName = line[0].ToSimpleString();
                ds.Tables.Add(dt);
            }

        }

        private static void ReadVAL(DataLake lake, VAL val)
        {
            for (int i = 0; i < val.Size; i++)
            {
                VAL line = val[i];
                DataSet ds = new DataSet();
                ReadVAL(ds, line[1]);
                ds.DataSetName = line[0].ToSimpleString();
                lake.Add(ds.DataSetName, ds);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                            obj = "{" + row[i].ToString() + "}";
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

    }
}

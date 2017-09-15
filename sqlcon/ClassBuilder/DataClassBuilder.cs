﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

using Sys;
using Sys.Data;
using Sys.Data.Comparison;
using Sys.Data.Manager;
using Sys.CodeBuilder;

namespace sqlcon
{
    enum DataClassType
    {
        undefined,
        list,
        dict,
        key
    }

    class DataClassBuilder
    {
        private Command cmd;
        private TableName tname;
        public DataClassBuilder(Command cmd, TableName tname)
        {
            this.cmd = cmd;
            this.tname = tname;
        }

        private string ns
        {
            get
            {
                string _ns = cmd.GetValue("ns") ?? "Sys.DataModel.Db";
                return _ns;
            }
        }

        private string ClassName(string tableName)
        {
            string _cname = "Table";
            if (cmd.GetValue("class") != null)
            {
                _cname = cmd.GetValue("class");
            }
            else if (!string.IsNullOrEmpty(tableName))
            {
                //use table name as class name
                string name = new string(tableName.Trim().Where(ch => char.IsLetterOrDigit(ch) || ch == '_').ToArray());
                if (name.Length > 0 && char.IsDigit(name[0]))
                    name = $"_{name}";

                if (name != string.Empty)
                    _cname = name;
            }
            return _cname;
        }



        private DataClassType dataType
        {
            get
            {
                string _dataType = cmd.GetValue("type") ?? "list";
                switch (_dataType)
                {
                    case "list": return DataClassType.list;
                    case "dict": return DataClassType.dict;
                    case "key": return DataClassType.key;
                }

                return DataClassType.undefined;
            }
        }

        public void ExportCSharpData(DataTable dt)
        {
            if (dataType == DataClassType.key)
                ExportConfigKey(dt);
            else
                ExportCSData(dt);
        }


        public void ExportConfigKey(DataTable dt)
        {
            var builder = new CSharpBuilder { nameSpace = ns };
            builder.AddUsing("System.Collections.Generic");

            string cname = ClassName(dt.TableName);
            var clss = new Class(cname)
            {
                modifier = Modifier.Public
            };

            builder.AddClass(clss);

            string columnName = cmd.GetValue("col");

            Field field;
            foreach (DataRow row in dt.Rows)
            {
                TypeInfo ty = new TypeInfo(typeof(string));
                string key;
                if (columnName != null)
                    key = row[columnName].ToString();
                else
                    key = row[0].ToString();

                string fieldName = key.Replace(".", "_").ToUpper();
                field = new Field(ty, fieldName, new Value(key)) { modifier = Modifier.Public | Modifier.Const };
                clss.Add(field);
            }

            PrintCode(builder, cname);
        }


        /// <summary>
        /// create C# data from data table
        /// </summary>
        /// <param name="cmd"></param>
        public void ExportCSData(DataTable dt)
        {

            string dataclass = cmd.GetValue("dataclass") ?? "DbReadOnly";

            var builder = new CSharpBuilder { nameSpace = ns };
            builder.AddUsing("System.Collections.Generic");
            string cname = ClassName(dt.TableName);

            var clss = new Class(cname)
            {
                modifier = Modifier.Public | Modifier.Partial
            };

            builder.AddClass(clss);

            Property prop;
            foreach (DataColumn column in dt.Columns)
            {
                bool nullable = dt.AsEnumerable().Any(row => row[column] is DBNull);
                TypeInfo ty = new TypeInfo(column.DataType) { Nullable = nullable };

                prop = new Property(ty, column.ColumnName.ToFieldName()) { modifier = Modifier.Public };
                clss.Add(prop);
            }

            clss = new Class(dataclass)
            {
                modifier = Modifier.Public | Modifier.Partial
            };
            builder.AddClass(clss);


            string[] columns = dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();


            if (dataType == DataClassType.list)
            {
                Field field = CreateListField(dt, cname, columns);
                clss.Add(field);
            }
            else
            {
                if (dt.Columns.Count < 2)
                {
                    stdio.ErrorFormat("cannot generate dictionary class, column# > 2");
                    return;
                }

                Field field = CreateDictionaryField(dt, cname, columns);
                clss.Add(field);
            }

            PrintCode(builder, cname);
        }

        private void PrintCode(CSharpBuilder builder, string cname)
        {
            string code = $"{builder}";

            string path = cmd.GetValue("out");
            if (path == null)
            {
                stdio.WriteLine(code);
            }
            else
            {
                string file = Path.ChangeExtension(Path.Combine(path, cname), "cs");
                try
                {
                    code.WriteIntoFile(file);
                    stdio.WriteLine("code generated on {0}", Path.GetFullPath(file));
                }
                catch (Exception ex)
                {
                    stdio.WriteLine(ex.Message);
                }
            }
        }

        private static Field CreateDictionaryField(DataTable dt, string cname, string[] columns)
        {
            string fieldName = $"{cname}Data";

            List<KeyValuePair<object, object>> L = new List<KeyValuePair<object, object>>();
            var keyType = new TypeInfo(dt.Columns[0].DataType);
            var valueType = new TypeInfo(dt.Columns[1].DataType);
            if (dt.Columns.Count != 2)
                valueType = new TypeInfo { userType = cname };

            TypeInfo type = new TypeInfo { userType = $"{cname}" };
            foreach (DataRow row in dt.Rows)
            {
                string key = Value.ToPrimitive(row[0]);

                if (dt.Columns.Count != 2)
                {
                    var V = Value.NewPropertyObject(type);
                    for (int i = 0; i < columns.Length; i++)
                    {
                        V.AddProperty(columns[i], new Value(row[i]));
                    }
                    L.Add(new KeyValuePair<object, object>(key, V));
                }
                else
                {
                    L.Add(new KeyValuePair<object, object>(key, new Value(row[1])));
                }
            }

            var groups = L.GroupBy(x => x.Key, x => x.Value);
            Dictionary<object, object> dict = new Dictionary<object, object>();
            foreach (var group in groups)
            {
                var A = group.ToArray();
                if (A.Length > 1)
                    valueType.isArray = true;
            }

            foreach (var group in groups)
            {
                var A = group.ToArray();
                object val;
                if (valueType.isArray)
                    val = new Value(A) { type = valueType };
                else
                    val = A[0];

                dict.Add(group.Key, val);
            }


            TypeInfo typeinfo = new TypeInfo { userType = $"Dictionary<{keyType}, {valueType}>" };
            Field field = new Field(typeinfo, fieldName, new Value(dict) { type = typeinfo })
            {
                modifier = Modifier.Public | Modifier.Static | Modifier.Readonly
            };

            return field;
        }

        private static Field CreateListField(DataTable dt, string cname, string[] columns)
        {
            string fieldName = $"{cname}Data";

            List<Value> L = new List<Value>();
            TypeInfo type = new TypeInfo { userType = $"{cname}" };
            foreach (DataRow row in dt.Rows)
            {
                var V = Value.NewPropertyObject(type);
                for (int i = 0; i < columns.Length; i++)
                {
                    V.AddProperty(columns[i], new Value(row[i]));
                }
                L.Add(V);
            }

            TypeInfo typeinfo = new TypeInfo { userType = $"{cname}[]" };
            Field field = new Field(typeinfo, fieldName, new Value(L.ToArray()) { type = typeinfo })
            {
                modifier = Modifier.Public | Modifier.Static | Modifier.Readonly
            };

            return field;
        }
    }
}

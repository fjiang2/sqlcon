using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.CodeBuilder;
using Sys.Data;
using Sys.Data.Manager;
using Sys.Stdio;

namespace sqlcon
{
    enum DataClassType
    {
        Undefined,
        Array,
        List,
        Dictionary,
        Enum,
        Constant
    }

    class DataClassBuilder : ClassMaker
    {
        private DataTable dt;

        public DataClassBuilder(ApplicationCommand cmd, DataTable dt)
            : base(cmd)
        {
            this.cmd = cmd;
            this.dt = dt;
        }

        public void ExportCSharpData()
        {
            switch (dataType)
            {
                case DataClassType.Array:
                case DataClassType.List:
                case DataClassType.Dictionary:
                    ExportCSData(dt);
                    return;

                case DataClassType.Enum:
                    ExportEnum(dt);
                    return;

                case DataClassType.Constant:
                    ExportConstant(dt);
                    return;
            }
        }


        private DataClassType dataType
        {
            get
            {
                string _dataType = cmd.GetValue("type") ?? "list";
                switch (_dataType)
                {
                    case "array": return DataClassType.Array;
                    case "list": return DataClassType.List;
                    case "dict": return DataClassType.Dictionary;
                    case "enum": return DataClassType.Enum;
                    case "const": return DataClassType.Constant;
                }

                return DataClassType.Undefined;
            }
        }




        protected override string ClassName
        {
            get
            {
                string tableName = dt.TableName;
                string _cname = base.ClassName;
                if (_cname != nameof(DataTable))
                    return _cname;

                if (!string.IsNullOrEmpty(tableName))
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
        }

        /// <summary>
        /// command: export /c# /code-column:Col1=Dictionary<int,string>;Col2=Dictionary<int,string>
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, TypeInfo> CodeColumnDef()
        {
            Dictionary<string, TypeInfo> dict = new Dictionary<string, TypeInfo>();
            var columns = cmd.GetValue("code-column");
            if (columns == null)
                return dict;
            string[] _columns = columns.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var column in _columns)
            {
                string[] kvp = column.Split('=');
                TypeInfo ty = new TypeInfo { UserType = kvp[1] };
                dict.Add(kvp[0], ty);
            }

            return dict;
        }


        /// <summary>
        /// create C# data from data table
        /// </summary>
        /// <param name="cmd"></param>
        public void ExportCSData(DataTable dt)
        {

            string dataclass = cmd.GetValue("dataclass") ?? "DbReadOnly";

            var builder = new CSharpBuilder { Namespace = NamespaceName };
            builder.AddUsing("System.Collections.Generic");
            string cname = ClassName;

            Dictionary<string, TypeInfo> codeColumns = CodeColumnDef();
            var clss = new Class(cname)
            {
                Modifier = Modifier.Public | Modifier.Partial
            };

            builder.AddClass(clss);

            Property prop;
            foreach (DataColumn column in dt.Columns)
            {
                bool nullable = dt.AsEnumerable().Any(row => row[column] is DBNull);
                TypeInfo ty = new TypeInfo(column.DataType) { Nullable = nullable };
                if (codeColumns.ContainsKey(column.ColumnName))
                    ty = codeColumns[column.ColumnName];

                prop = new Property(ty, column.ColumnName.ToFieldName()) { Modifier = Modifier.Public };
                clss.Add(prop);
            }

            clss = new Class(dataclass)
            {
                Modifier = Modifier.Public | Modifier.Partial
            };
            builder.AddClass(clss);


            string[] columns = dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();


            if (dataType == DataClassType.List || dataType == DataClassType.Array)
            {
                Field field = CreateListOrArrayField(dataType, dt, cname, columns, codeColumns);
                clss.Add(field);
            }
            else
            {
                if (dt.Columns.Count < 2)
                {
                    cerr.WriteLine("cannot generate dictionary class, column# > 2");
                    return;
                }

                Field field = CreateDictionaryField(dt, cname, columns, codeColumns);
                clss.Add(field);
            }

            PrintOutput(builder, cname);
        }


        private static Field CreateDictionaryField(DataTable dt, string cname, string[] columns, IDictionary<string, TypeInfo> codeColumns)
        {
            string fieldName = $"{cname}Data";

            List<KeyValuePair<object, object>> L = new List<KeyValuePair<object, object>>();
            var keyType = new TypeInfo(dt.Columns[0].DataType);
            var valueType = new TypeInfo(dt.Columns[1].DataType);
            if (dt.Columns.Count != 2)
                valueType = new TypeInfo { UserType = cname };

            TypeInfo type = new TypeInfo { UserType = $"{cname}" };
            foreach (DataRow row in dt.Rows)
            {
                string key = Value.ToPrimitive(row[0]);

                if (dt.Columns.Count != 2)
                {
                    var V = Value.NewPropertyObject(type);
                    for (int i = 0; i < columns.Length; i++)
                    {
                        object obj = row[i];
                        if (codeColumns.ContainsKey(columns[i]))
                            obj = new CodeString(obj.ToString());

                        V.AddProperty(columns[i], new Value(obj));
                    }
                    L.Add(new KeyValuePair<object, object>(key, V));
                }
                else
                {
                    object obj = row[1];
                    if (codeColumns.ContainsKey(columns[1]))
                        obj = new CodeString(obj.ToString());

                    L.Add(new KeyValuePair<object, object>(key, new Value(obj)));
                }
            }

            var groups = L.GroupBy(x => x.Key, x => x.Value);
            Dictionary<object, object> dict = new Dictionary<object, object>();
            foreach (var group in groups)
            {
                var A = group.ToArray();
                if (A.Length > 1)
                {
                    valueType.IsArray = true;
                    break;
                }
            }

            foreach (var group in groups)
            {
                var A = group.ToArray();
                object val;
                if (valueType.IsArray)
                    val = new Value(A) { Type = valueType };
                else
                    val = A[0];

                dict.Add(group.Key, val);
            }


            TypeInfo typeinfo = new TypeInfo { UserType = $"Dictionary<{keyType}, {valueType}>" };
            Field field = new Field(typeinfo, fieldName, new Value(dict) { Type = typeinfo })
            {
                Modifier = Modifier.Public | Modifier.Static | Modifier.Readonly
            };

            return field;
        }

        private static Field CreateListOrArrayField(DataClassType dataType, DataTable dt, string cname, string[] columns, IDictionary<string, TypeInfo> codeColumns)
        {
            string fieldName = $"{cname}Data";

            List<Value> L = new List<Value>();
            TypeInfo type = new TypeInfo { UserType = $"{cname}" };
            foreach (DataRow row in dt.Rows)
            {
                var V = Value.NewPropertyObject(type);
                for (int i = 0; i < columns.Length; i++)
                {
                    object obj = row[i];
                    if (codeColumns.ContainsKey(columns[i]))
                        obj = new CodeString(obj.ToString());

                    V.AddProperty(columns[i], new Value(obj));
                }
                L.Add(V);
            }

            TypeInfo typeinfo = new TypeInfo { UserType = $"{cname}[]" };
            if (dataType == DataClassType.List)
                typeinfo = new TypeInfo { UserType = $"List<{cname}>" };

            Field field = new Field(typeinfo, fieldName, new Value(L.ToArray()) { Type = typeinfo })
            {
                Modifier = Modifier.Public | Modifier.Static | Modifier.Readonly
            };

            return field;
        }


        private void ExportEnum(DataTable dt)
        {
            int count = dt.Columns.Count;
            if (count < 2)
            {
                cerr.WriteLine("cannot generate enum class because table is < 2 columns");
                return;
            }

            CSharpBuilder builder = new CSharpBuilder()
            {
                Namespace = NamespaceName
            };

            string cname = ClassName;
            if (count > 2)
                builder.AddUsing("System.ComponentModel");

            DataColumn _feature = null;     //1st string column as property name
            DataColumn _value = null;       //1st int column as property value
            DataColumn _label = null;       //2nd string column as attribute [DataEnum("label")]
            DataColumn _category = null;    //3rd string column as category to generate multiple enum types
            foreach (DataColumn column in dt.Columns)
            {
                if (column.DataType == typeof(string))
                {
                    if (_feature == null)
                        _feature = column;
                    else if (_label == null)
                        _label = column;
                    else if (_category == null)
                        _category = column;
                }

                if (_value == null && column.DataType == typeof(int))
                    _value = column;
            }

            if (_feature == null)
            {
                cerr.WriteLine("invalid enum property name");
                return;
            }

            if (_value == null)
            {
                cerr.WriteLine("invalid enum property value");
                return;
            }

            var rows = dt
                .AsEnumerable()
                .Select(row => new
                {
                    Feature = row.Field<string>(_feature),
                    Value = row.Field<int>(_value),
                    Category = _category != null ? row.Field<string>(_category) : null,
                    Label = _label != null ? row.Field<string>(_label) : null
                });

            if (_category != null)
            {
                var groups = rows.GroupBy(row => row.Category);

                foreach (var group in groups)
                {
                    var _enum = new EnumType(group.First().Category);
                    foreach (var row in group)
                        _enum.Add(row.Feature, row.Value, $"\"{row.Label}\"");

                    builder.AddEnum(_enum);
                }
            }
            else
            {
                var _enum = new EnumType(cname);
                foreach (var row in rows)
                    _enum.Add(row.Feature, row.Value, $"\"{row.Label}\"");

                builder.AddEnum(_enum);
            }

            PrintOutput(builder, cname);

        }


        private void ExportConstant(DataTable dt)
        {
            CSharpBuilder builder = new CSharpBuilder()
            {
                Namespace = NamespaceName
            };

            string cname = ClassName;
            Class clss = new Class(cname)
            {
                Modifier = Modifier.Public | Modifier.Static
            };
            builder.AddClass(clss);
            List<string> rows = new List<string>();
            if (cmd.Columns.Length > 0)
            {
                foreach (string column in cmd.Columns)
                {
                    rows.AddRange(dt.AsEnumerable()
                        .Where(row => row[column] != DBNull.Value)
                        .Select(row => row.Field<string>(column))
                    );
                }
            }
            else
            {
                cerr.WriteLine("missing parameter /col:c1,c2");
                return;
            }

            var L = rows.Distinct().OrderBy(x => x);
            foreach (string fieldName in L)
            {
                Field field = new Field(new TypeInfo("string"), fieldName, new Value(fieldName))
                {
                    Modifier = Modifier.Public | Modifier.Const
                };

                clss.Add(field);
            }

            PrintOutput(builder, cname);
        }
    }


}

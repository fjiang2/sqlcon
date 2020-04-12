using Sys.CodeBuilder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tie;

namespace sqlcon
{
    enum CodeMemberType
    {
        Field = 0x01,
        Property = 0x02,
        Method = 0x04,
    };

    internal class ConfigScript
    {
        private Memory DS = new Memory();

        /// <summary>
        /// create hierachical property or field?
        /// </summary>
        public CodeMemberType HierarchicalMemberType { get; set; } = CodeMemberType.Property;

        /// <summary>
        /// class name of const key 
        /// </summary>
        public string ConstKeyClassName { get; set; }

        /// <summary>
        /// class name of default value
        /// </summary>
        public string DefaultValueClassName { get; set; }

        public string GetValueMethodName { get; set; } = "GetValue";

        public ConfigScript(string code)
        {
            Script.Execute(code, DS);
        }

        public Class Generate(string cname)
        {
            Class clss = new Class(cname) { Modifier = Modifier.Public | Modifier.Static | Modifier.Partial };


            foreach (VAR var in DS.Names)
            {
                VAL val = DS[var];
                createConfigKeyMap(clss, string.Empty, (string)var, val);
            }

            return clss;
        }

        public List<Field> ConstKeyFields { get; } = new List<Field>();
        public List<Field> DefaultValueFields { get; } = new List<Field>();
        public List<Field> StaticFields { get; } = new List<Field>();
        public List<Property> StaticProperties { get; } = new List<Property>();
        public List<Method> StaticMethods { get; } = new List<Method>();

        private void createConfigKeyMap(Class clss, string prefix, string key, VAL val)
        {
            if (val.IsAssociativeArray())
            {
                var clss1 = new Class(key) { Modifier = Modifier.Public | Modifier.Static };
                clss.Add(clss1);

                prefix = MakeVariableName(prefix, key);

                foreach (var member in val.Members)
                {
                    createConfigKeyMap(clss1, prefix, member.Name, member.Value);
                }

                return;
            }

            //if (val.IsList)
            //{
            //    prefix = MakeVariableName(prefix, key);

            //    int index = 0;
            //    foreach (var item in val)
            //    {
            //        createConfigKeyMap(clss, prefix, $"[{index}]", item);
            //        index++;
            //    }

            //    return;
            //}

            if (val.IsFunction)
            {
                return;
            }

            Type type = typeof(string);
            if (val.HostValue != null)
            {
                type = val.HostValue.GetType();
            }
            TypeInfo ty = new TypeInfo(type);

            string var = MakeVariableName(prefix, key);

            switch (HierarchicalMemberType)
            {
                case CodeMemberType.Property:
                    Property prop = createProperty(key, ty, var);
                    clss.Add(prop);
                    break;

                case CodeMemberType.Method:
                    Method mtd = createMethod(key, ty, var);
                    clss.Add(mtd);
                    break;

                default:
                    Field fld = createField(key, ty, var);
                    clss.Add(fld);
                    break;
            }

            Other(ty, var, val);
        }

        private string MakeVariableName(string prefix, string key)
        {
            if (prefix == string.Empty)
                return key;

            if (key.StartsWith("[") && key.EndsWith("]"))
                return $"{prefix}{key}";
            else
                return $"{prefix}.{key}";


        }

        private Property createProperty(string name, TypeInfo ty, string var)
        {
            Comment comment = new Comment(var) { Alignment = Alignment.Top };
            return new Property(ty, name)
            {
                Modifier = Modifier.Public | Modifier.Static,
                Expression = expr(ty, var),
                Comment = comment
            };
        }

        private Method createMethod(string name, TypeInfo ty, string var)
        {
            Comment comment = new Comment(var)
            {
                Alignment = Alignment.Top
            };

            Parameter parm = new Parameter(ty, "value")
            {
                Value = $"default({ty})"
            };

            Method method = new Method(ty, name)
            {
                Modifier = Modifier.Public | Modifier.Static,
                IsExpressionBodied = true,
                Params = new Parameters(new Parameter[] { parm }),
                Comment = comment
            };

            method.Statement.Append($"=> {mtd(ty, var)};");
            return method;
        }

        private Field createField(string name, TypeInfo ty, string var)
        {
            Comment comment = new Comment(var) { Alignment = Alignment.Top };
            return new Field(ty, name)
            {
                Modifier = Modifier.Public | Modifier.Readonly | Modifier.Static,
                UserValue = expr(ty, var),
                Comment = comment
            };
        }



        private string expr(TypeInfo ty, string var)
        {
            string constKey = ToConstKey(var);
            string defaultKey = ToDefaultKey(var);  //default value

            if (!string.IsNullOrEmpty(ConstKeyClassName))
                constKey = $"{ConstKeyClassName}.{constKey}";

            if (!string.IsNullOrEmpty(DefaultValueClassName))
                defaultKey = $"{DefaultValueClassName}.{defaultKey}";

            return $"{GetValueMethodName}<{ty}>({constKey}, {defaultKey})";
        }

        private string mtd(TypeInfo ty, string var)
        {
            string constKey = ToConstKey(var);

            if (!string.IsNullOrEmpty(ConstKeyClassName))
                constKey = $"{ConstKeyClassName}.{constKey}";

            return $"{GetValueMethodName}<{ty}>({constKey}, value)";
        }

        private void Other(TypeInfo ty, string var, VAL val)
        {
            string constKey = ToConstKey(var);
            string defaultKey = ToDefaultKey(var);
            Comment comment = new Comment(var) { Alignment = Alignment.Top };

            //const key field
            Field field = new Field(new TypeInfo(typeof(string)), constKey, new Value(var))
            {
                Modifier = Modifier.Public | Modifier.Const,
            };
            ConstKeyFields.Add(field);

            //default value field
            Modifier modifier = Modifier.Public | Modifier.Const;
            string value = val.ToString();
            if (val.IsList)
            {
                modifier = Modifier.Public | Modifier.Readonly | Modifier.Static;
                value = $"new {ty} " + value;
            }

            field = new Field(ty, defaultKey)
            {
                Modifier = modifier,
                UserValue = value,
                Comment = comment
            };
            DefaultValueFields.Add(field);

            StaticFields.Add(createField(TOKEY(var), ty, var));
            StaticProperties.Add(createProperty(toPascal(var), ty, var));
            StaticMethods.Add(createMethod(toPascal(var), ty, var));
        }

        private static string ToKey(string key) => key.Replace(".", "_").Replace("[", "_").Replace("]", "");
        private static string TOKEY(string key) => ToKey(key).ToUpper();
        private static string tokey(string key) => ToKey(key).ToLower();
        private static string toPascal(string key)
        {
            string _toPascal(string k)
            {
                if (k.Length == 0)
                    return k;
                else
                    return char.ToUpper(k[0]) + k.Substring(1).ToLower();
            }

            return ToKey(key)
                .Split('_')
                .Select(k => _toPascal(k))
                .Aggregate((x, y) => $"{x}_{y}");
        }

        private static string ToConstKey(string key) => TOKEY(key);
        private static string ToDefaultKey(string key) => "_" + TOKEY(key);


        public string GenerateTieScript(bool flat)
        {
            List<string> statements = new List<string>();
            if (flat)
            {
                foreach (VAR var in DS.Names)
                {
                    VAL val = DS[var];
                    createConfigFile(statements, string.Empty, (string)var, val);
                }

            }
            else
            {
                foreach (VAR var in DS.Names)
                {
                    VAL val = DS[var];
                    string text = $"{var} = {val.ToExJson()};";
                    statements.Add(text);
                    statements.Add(string.Empty);
                }
            }

            return string.Join(Environment.NewLine, statements);

        }

        private void createConfigFile(List<string> statements, string prefix, string key, VAL val)
        {
            if (val.IsAssociativeArray())
            {
                prefix = MakeVariableName(prefix, key);

                foreach (var member in val.Members)
                {
                    createConfigFile(statements, prefix, member.Name, member.Value);
                }

                statements.Add(string.Empty);

                return;
            }

            if (val.IsList)
            {
                prefix = MakeVariableName(prefix, key);

                int index = 0;
                foreach (var item in val)
                {
                    createConfigFile(statements, prefix, $"[{index}]", item);
                    index++;
                }

                return;
            }

            string var = MakeVariableName(prefix, key);
            string code = $"{var} = {val.ToString()};";

            statements.Add(code);
        }

    }
}

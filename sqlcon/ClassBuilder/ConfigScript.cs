using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Tie;
using Sys.CodeBuilder;

namespace sqlcon
{
    class ConfigScript
    {
        private Memory DS = new Memory();

        /// <summary>
        /// create hierachical property or field?
        /// </summary>
        public bool IsHierarchicalProperty { get; set; } = true;

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
            Class clss = new Class(cname) { modifier = Modifier.Public | Modifier.Static | Modifier.Partial };


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

        private void createConfigKeyMap(Class clss, string prefix, string key, VAL val)
        {
            if (val.IsAssociativeArray())
            {
                var clss1 = new Class(key) { modifier = Modifier.Public | Modifier.Static };
                clss.Add(clss1);

                if (prefix == string.Empty)
                    prefix = key;
                else
                    prefix = $"{prefix}.{key}";

                foreach (var member in val.Members)
                {
                    createConfigKeyMap(clss1, prefix, member.Name, member.Value);
                    continue;
                }

                return;
            }

            Type type = typeof(string);
            if (val.HostValue != null)
            {
                type = val.HostValue.GetType();
            }
            TypeInfo ty = new TypeInfo(type);

            string var = $"{prefix}.{key}";
            if (prefix == string.Empty)
                var = key;

            if (IsHierarchicalProperty)
            {
                Property prop = createProperty(key, ty, var);
                clss.Add(prop);
            }
            else
            {
                Field fld = createField(key, ty, var);
                clss.Add(fld);
            }

            Other(ty, var, val);
        }

        private Property createProperty(string name, TypeInfo ty, string var)
        {
            Comment comment = new Comment(var) { alignment = Alignment.Top };
            return new Property(ty, name)
            {
                modifier = Modifier.Public | Modifier.Static,
                Expression = expr(ty, var),
                comment = comment
            };
        }

        private Field createField(string name, TypeInfo ty, string var)
        {
            Comment comment = new Comment(var) { alignment = Alignment.Top };
            return new Field(ty, name)
            {
                modifier = Modifier.Public | Modifier.Readonly | Modifier.Static,
                userValue = expr(ty, var),
                comment = comment
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


        private void Other(TypeInfo ty, string var, VAL val)
        {
            string constKey = ToConstKey(var);
            string defaultKey = ToDefaultKey(var);
            Comment comment = new Comment(var) { alignment = Alignment.Top };

            //const key field
            Field field = new Field(new TypeInfo(typeof(string)), constKey, new Value(var))
            {
                modifier = Modifier.Public | Modifier.Const,
            };
            ConstKeyFields.Add(field);

            //default value field
            field = new Field(ty, defaultKey)
            {
                modifier = Modifier.Public | Modifier.Readonly | Modifier.Static,
                userValue = val.ToString(),
                comment = comment
            };
            DefaultValueFields.Add(field);

            StaticFields.Add(createField(TOKEY(var), ty, var));
            StaticProperties.Add(createProperty(toPascal(var), ty, var));
        }

        static string TOKEY(string key) => key.Replace(".", "_").ToUpper();
        static string tokey(string key) => key.Replace(".", "_").ToLower();
        static string toPascal(string key) => key.Split('.').Select(k => char.ToUpper(k[0]) + k.Substring(1).ToLower()).Aggregate((x, y) => $"{x}_{y}");
        static string ToConstKey(string key) => "_" + TOKEY(key);
        static string ToDefaultKey(string key) => "__" + TOKEY(key);


        public string GenerateTieScript(bool flat)
        {
            List<string> statements = new List<string>();
            if (flat)
            {
                foreach (VAR var in DS.Names)
                {
                    VAL val = DS[var];
                    createConfigFile(statements, string.Empty, (string)var, val);
                    statements.Add(string.Empty);
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
                if (prefix == string.Empty)
                    prefix = key;
                else
                    prefix = $"{prefix}.{key}";

                foreach (var member in val.Members)
                {
                    createConfigFile(statements, prefix, member.Name, member.Value);
                    continue;
                }

                return;
            }

            string var = $"{prefix}.{key}";
            if (prefix == string.Empty)
                var = key;

            string code = $"{var} = {val.ToString()};";
            statements.Add(code);
        }

    }
}

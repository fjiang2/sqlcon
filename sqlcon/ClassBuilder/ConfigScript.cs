using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tie;
using Sys.CodeBuilder;

namespace sqlcon
{
    class ConfigScript
    {
        private Memory DS = new Memory();
        private string cname;

        static string TOKEY(string key) => key.Replace(".", "_").ToUpper();
        static string tokey(string key) => key.Replace(".", "_").ToLower();
        static string toCamel(string key) => key.Split('.').Select(k => char.ToUpper(k[0]) + k.Substring(1).ToLower()).Aggregate((x, y) => $"{x}_{y}");
        static string ToConstKey(string key) => "_" + TOKEY(key);
        static string ToDefaultKey(string key) => "__" + TOKEY(key);

        public ConfigScript(string cname, string code)
        {
            this.cname = cname;
            Script.Execute(code, DS);
        }

        public Class Generate()
        {
            Class clss = new Class(cname) { modifier = Modifier.Public | Modifier.Static | Modifier.Partial };

            this.ConstKeyFields.Clear();
            this.DefaultValueFields.Clear();
            this.StaticProperties.Clear();
            this.StaticFields.Clear();

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
            Property prop = new Property(ty, key) { modifier = Modifier.Public | Modifier.Static };
            clss.Add(prop);

            string var = $"{prefix}.{key}";
            if (prefix == string.Empty)
                var = key;

            string constKey = ToConstKey(var);
            string defaultKey = ToDefaultKey(var);
            prop.Expression = $"GetValue<{ty}>({constKey}, {defaultKey})";

            Other(ty, var, val);
        }

        private void Other(TypeInfo ty, string var, VAL val)
        {
            string constKey = ToConstKey(var);
            string defaultKey = ToDefaultKey(var);

            //const key
            Field field = new Field(new TypeInfo(typeof(string)), constKey, new Value(var))
            {
                modifier = Modifier.Public | Modifier.Const,
            };
            ConstKeyFields.Add(field);

            //default value
            field = new Field(ty, defaultKey)
            {
                modifier = Modifier.Private | Modifier.Readonly | Modifier.Static,
                userValue = val.ToString(),
                comment = new Comment(var) { alignment = Alignment.Top }
            };
            DefaultValueFields.Add(field);

            field = new Field(ty, TOKEY(var))
            {
                modifier = Modifier.Public | Modifier.Readonly | Modifier.Static,
                userValue = $"GetValue<{ty}>({constKey}, {defaultKey})",
                comment = new Comment(var) { alignment = Alignment.Top }
            };
            StaticFields.Add(field);

            Property prop = new Property(ty, toCamel(var))
            {
                modifier = Modifier.Public | Modifier.Static,
                Expression = $"GetValue<{ty}>({constKey}, {defaultKey})",
                comment = new Comment(var) { alignment = Alignment.Top }
            };

            StaticProperties.Add(prop);
        }
    }
}

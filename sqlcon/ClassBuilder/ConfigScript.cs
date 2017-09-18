using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Tie;
using Sys.CodeBuilder;

namespace sqlcon
{
    class ConfigScript
    {
        private Memory DS = new Memory();
        private string cname;

        public static string ToKey(string key) => key.Replace(".", "_").ToUpper();
        public static string ToConstKey(string key) => "_" + ToKey(key);
        public static string ToDefaultKey(string key) => "__" + ToKey(key);

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
            this.ValueProperties.Clear();

            foreach (VAR var in DS.Names)
            {
                VAL val = DS[var];
                createConfigKeyMap(clss, string.Empty, (string)var, val);
            }

            return clss;
        }

        public CSharpBuilder CreateConstKeyClass()
        {
            CSharpBuilder builder = new CSharpBuilder();
            Class clss = new Class(cname) { modifier = Modifier.Public | Modifier.Static | Modifier.Partial };
            foreach (Field field in ConstKeyFields)
                clss.Add(field);

            builder.AddClass(clss);
            return builder;
        }
        public CSharpBuilder CreateDefaultValueClass()
        {
            CSharpBuilder builder = new CSharpBuilder();
            Class clss = new Class(cname) { modifier = Modifier.Public | Modifier.Static | Modifier.Partial };
            foreach (Field field in DefaultValueFields)
                clss.Add(field);

            builder.AddClass(clss);
            return builder;
        }

        public CSharpBuilder CreateGetValueClass()
        {
            CSharpBuilder builder = new CSharpBuilder();
            Class clss = new Class(cname) { modifier = Modifier.Public | Modifier.Static | Modifier.Partial };
            foreach (Property prop in ValueProperties)
                clss.Add(prop);

            builder.AddClass(clss);
            return builder;
        }

        public List<Field> ConstKeyFields { get; } = new List<Field>();
        public List<Field> DefaultValueFields { get; } = new List<Field>();
        public List<Property> ValueProperties { get; } = new List<Property>();

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
                comment = new Comment(var) { Orientation = Orientation.Vertical }
            };

            DefaultValueFields.Add(field);


            prop = new Property(ty, ToKey(var)) { modifier = Modifier.Public | Modifier.Static };
            prop.Expression = $"GetValue<{ty}>({constKey}, {defaultKey})";
            ValueProperties.Add(prop);

        }

    }
}

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

        public static string ToKey(string key) => key.Replace(".", "_").ToUpper();
        public static string ToConstKey(string key) => "_" + ToKey(key);
        public static string ToDefaultKey(string key) => "__" + ToKey(key);

        public ConfigScript(string code)
        {
            Script.Execute(code, DS);
        }

        public void Generate(Class clss)
        {
            List<Field> fields = new List<Field>();
            foreach (VAR var in DS.Names)
            {
                VAL val = DS[var];
                createConfigKeyMap(clss, string.Empty, (string)var, val, fields);
            }

            foreach (Field field in fields.Where(x => !x.name.StartsWith("__")))
                clss.Add(field);

            foreach (Field field in fields.Where(x => x.name.StartsWith("__")))
                clss.Add(field);
        }

        private void createConfigKeyMap(Class clss, string prefix, string key, VAL val, List<Field> fields)
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
                    createConfigKeyMap(clss1, prefix, member.Name, member.Value, fields);
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
            fields.Add(field);

            //default value
            field = new Field(ty, defaultKey)
            {
                modifier = Modifier.Private | Modifier.Readonly | Modifier.Static,
                userValue = val.ToString(),
                comment = new Comment(var) { Orientation = Orientation.Vertical }
            };

            fields.Add(field);
            clss.Add(prop);
        }

    }
}

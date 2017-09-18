using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

using Sys.Data;
using Sys.Data.Manager;
using Sys.CodeBuilder;
using Tie;

namespace sqlcon
{
    enum ConfClassType
    {
        Undefined,
        Const,
        Value,
        Prop,
        Cfg
    }

    class ConfClassBuilder : ClassMaker
    {
        private DataTable dt;

        public ConfClassBuilder(Command cmd, DataTable dt)
            : base(cmd)
        {
            this.cmd = cmd;
            this.dt = dt;
        }

        public void ExportCSharpData()
        {
            ExportConfigKey(dt, dataType);
        }


        private ConfClassType dataType
        {
            get
            {
                string _dataType = cmd.GetValue("type") ?? "const";
                switch (_dataType)
                {
                    case "const": return ConfClassType.Const;
                    case "value": return ConfClassType.Value;
                    case "prop": return ConfClassType.Prop;
                    case "cfg": return ConfClassType.Cfg;
                }

                return ConfClassType.Undefined;
            }
        }



        class KeyLine
        {
            public string Key { get; set; }
            public string DefaultValue { get; set; }

        }

        public void ExportConfigKey(DataTable dt, ConfClassType dataType)
        {
            bool aggregationKey = cmd.Has("aggregate");
            var builder = new CSharpBuilder { nameSpace = NameSpace };
            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");

            string cname = ClassName;

            string columnKey = cmd.GetValue("key");
            string defaultValue = cmd.GetValue("default");

            List<KeyLine> lines = new List<KeyLine>();
            foreach (DataRow row in dt.Rows)
            {

                KeyLine line = new KeyLine();

                if (columnKey != null)
                    line.Key = row[columnKey].ToString();
                else
                    line.Key = row[0].ToString();

                if (defaultValue != null)
                    line.DefaultValue = row[defaultValue].ToString();

                lines.Add(line);
            }

            lines = lines.OrderBy(x => x.Key).ToList();

            if (dataType == ConfClassType.Const)
            {
                var clss = new Class(cname) { modifier = Modifier.Public };
                builder.AddClass(clss);
                CreateConfigKeyClass(clss, lines, aggregationKey);
            }
            else if (columnKey != null && defaultValue != null)
            {
                if (dataType == ConfClassType.Value)
                {
                    var clss = new Class(cname) { modifier = Modifier.Public | Modifier.Static | Modifier.Partial };
                    builder.AddClass(clss);
                    CreateConfigValueClass(clss, lines);
                }
                else if (dataType == ConfClassType.Prop)
                {
                    var clss = CreateConfigSettingClass(ClassName, lines);
                    builder.AddClass(clss);
                }
            }
            else if (dataType == ConfClassType.Cfg)
            {
                string path = cmd.GetValue("in");
                if (!File.Exists(path))
                {
                    stdio.Error($"file {path} not found");
                    return;
                }

                string code = File.ReadAllText(path);
                var maker = new ConfigScript(cname, code);
                var clss = maker.Generate();
                builder.AddClass(clss);

                var _builder = maker.CreateConstKeyClass();
                PrintOutput(_builder, $"{cname}~1");

                _builder = maker.CreateDefaultValueClass();
                PrintOutput(_builder, $"{cname}~2");

                _builder = maker.CreateGetValueClass();
                PrintOutput(_builder, $"{cname}~3");
            }

            PrintOutput(builder, cname);
        }

        private static void CreateConfigKeyClass(Class clss, List<KeyLine> lines, bool aggregationKey)
        {
            List<string> keys = new List<string>();
            foreach (var line in lines)
            {
                //Aggregation key, 
                // key = "a.b.c" will create 3 keys, A, A_B, A_B_C
                if (aggregationKey)
                {
                    string[] items = line.Key.Split('.');

                    if (items.Length > 1)
                    {
                        for (int i = 0; i < items.Length - 1; i++)
                        {
                            string _key = string.Join(".", items.Take(i + 1));
                            if (keys.IndexOf(_key) < 0)
                                keys.Add(_key);
                        }

                    }
                }

                keys.Add(line.Key);
            }

            char lastCh = lines.First().Key[0];
            foreach (string key in keys)
            {
                TypeInfo ty = new TypeInfo(typeof(string));

                string fieldName = ConfigScript.ToKey(key);
                var field = new Field(ty, fieldName, new Value(key)) { modifier = Modifier.Public | Modifier.Const };
                clss.Add(field);

                if (lastCh != key[0])
                    clss.AppendLine();

                lastCh = key[0];
            }
        }

        private void CreateConfigValueClass(Class clss, List<KeyLine> lines)
        {

            //generate static variable = GetValue<T>(...)
            char lastCh = lines.First().Key[0];
            foreach (var line in lines)
            {
                VAL val = Script.Evaluate(line.DefaultValue);
                Type type = typeof(string);
                if (val.HostValue != null)
                    type = val.HostValue.GetType();

                TypeInfo ty = new TypeInfo(type);

                string expr;
                if (line.DefaultValue != null)
                    expr = $"GetValue<{ty}>({ConfigScript.ToConstKey(line.Key)}, {ConfigScript.ToDefaultKey(line.Key)})";
                else
                    expr = $"GetValue<{ty}>({ConfigScript.ToConstKey(line.Key)})";

                Field field = new Field(ty, ConfigScript.ToKey(line.Key)) { modifier = Modifier.Public | Modifier.Static | Modifier.Readonly, userValue = expr };
                clss.Add(field);

                if (lastCh != line.Key[0])
                    clss.AppendLine();

                lastCh = line.Key[0];
            }


            //generate const key
            clss.AppendLine();
            foreach (var line in lines)
            {
                TypeInfo ty = new TypeInfo(typeof(string));
                string fieldName = ConfigScript.ToConstKey(line.Key);
                var field = new Field(ty, fieldName, new Value(line.Key)) { modifier = Modifier.Public | Modifier.Const };
                clss.Add(field);
            }

            //generate default value
            clss.AppendLine();
            foreach (var line in lines)
            {
                VAL val = Script.Evaluate(line.DefaultValue);
                Type type = typeof(string);
                if (val.HostValue != null)
                    type = val.HostValue.GetType();

                TypeInfo ty = new TypeInfo(type);
                string fieldName = ConfigScript.ToDefaultKey(line.Key);
                var field = new Field(ty, fieldName)
                {
                    modifier = Modifier.Private | Modifier.Static | Modifier.Readonly,
                    userValue = line.DefaultValue,
                    comment = new Comment(line.Key) { Orientation = Orientation.Vertical }
                };
                clss.Add(field);
            }

        }

        /// <summary>
        /// create static class Setting 
        /// </summary>
        /// <param name="clss"></param>
        /// <param name="lines"></param>
        private Class CreateConfigSettingClass(string cname, List<KeyLine> lines)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var line in lines)
            {
                builder.AppendLine($"{line.Key}={line.DefaultValue};");
            }
            var maker = new ConfigScript(cname, builder.ToString());
            return maker.Generate();
        }



    }


}

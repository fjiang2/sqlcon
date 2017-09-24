using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

using Sys.CodeBuilder;

namespace sqlcon
{

    class ConfClassBuilder : ClassMaker
    {
        [Flags]
        enum ClassType
        {
            Nothing = 0x00,

            ConstKey = 0x01,                //  public const string _MATH_PI = "Math.PI";
            DefaultValue = 0x02,            //  public static double __MATH_PI = 3.14;
            StaticField = 0x04,             //  public static double MATH_PI = GetValue<double>(_MATH_PI, __MATH_PI);
            StaticPropery = 0x08,           //  public static double MATH_PI => GetValue<double>(_MATH_PI, __MATH_PI);
            HierarchicalField = 0x10,       //  public static Math { public static class Pi = GetValue<double>(_MATH_PI, __MATH_PI); }
            HierarchicalProperty = 0x20,    //  public static Math { public static class Pi => GetValue<double>(_MATH_PI, __MATH_PI); }
        }

        private DataTable dt;

        public ConfClassBuilder(Command cmd, DataTable dt)
            : base(cmd)
        {
            this.cmd = cmd;
            this.dt = dt;
        }

        public void ExportCSharpData()
        {
            string code;
            if (cmd.GetValue("in") != null)
                code = ReadCode();
            else
                code = ReadCode(dt);

            if (code == null)
                return;

            ClassType ctype = getClassType();
            string _GetValueMethodName = cmd.GetValue("method");
            string _ConstKeyClassName = cmd.GetValue("kc");
            string _DefaultValueClassName = cmd.GetValue("dc");

            var builder = new CSharpBuilder { nameSpace = NameSpace };
            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            string cname = ClassName;

            var maker = new ConfigScript(cname, code);
            maker.IsHierarchicalProperty = (ctype & ClassType.HierarchicalProperty) == ClassType.HierarchicalProperty;

            if (_GetValueMethodName != null)
                maker.GetValueMethodName = _GetValueMethodName;

            if (_ConstKeyClassName != null)
                maker.ConstKeyClassName = _ConstKeyClassName;

            if (_DefaultValueClassName != null)
                maker.DefaultValueClassName = _DefaultValueClassName;

            var clss = maker.Generate();
            builder.AddClass(clss);

            if (ctype == ClassType.ConstKey)
                builder = CreateClass(maker.ConstKeyFields);
            else if (ctype == ClassType.DefaultValue)
                builder = CreateClass(maker.DefaultValueFields);
            else if (ctype == ClassType.StaticField)
                builder = CreateClass(maker.StaticFields);
            else if (ctype == ClassType.StaticPropery)
                builder = CreateClass(maker.StaticProperties);
            else if (ctype == ClassType.HierarchicalField || ctype == ClassType.HierarchicalProperty)
            {
                //skip, because clss has created class already
            }
            else
            {
                if ((ctype & ClassType.HierarchicalField) != ClassType.HierarchicalField && (ctype & ClassType.HierarchicalProperty) != ClassType.HierarchicalProperty)
                    clss.Clear();

                if ((ctype & ClassType.StaticField) == ClassType.StaticField)
                    clss.AddRange(maker.StaticFields);

                if ((ctype & ClassType.StaticPropery) == ClassType.StaticPropery)
                    clss.AddRange(maker.StaticProperties);

                if ((ctype & ClassType.ConstKey) == ClassType.ConstKey)
                    clss.AddRange(maker.ConstKeyFields);

                if ((ctype & ClassType.DefaultValue) == ClassType.DefaultValue)
                    clss.AddRange(maker.DefaultValueFields);
            }

            builder.AddUsingRange(base.Usings);
            PrintOutput(builder, cname);
        }


        private ClassType getClassType()
        {
            string _type = cmd.GetValue("type") ?? "kdP";

            ClassType ctype = ClassType.Nothing;

            for (int i = 0; i < _type.Length; i++)
            {
                char ty = _type[i];

                switch (ty)
                {
                    case 'k':
                        ctype |= ClassType.ConstKey;
                        break;

                    case 'd':
                        ctype |= ClassType.DefaultValue;
                        break;

                    case 'F':
                        ctype |= ClassType.StaticField;
                        break;

                    case 'P':
                        ctype |= ClassType.StaticPropery;
                        break;

                    case 'f':
                        ctype |= ClassType.HierarchicalField;
                        break;

                    case 'p':
                        ctype |= ClassType.HierarchicalProperty;
                        break;
                }
            }
            return ctype;
        }

        private CSharpBuilder CreateClass(IEnumerable<Buildable> elements)
        {
            CSharpBuilder builder = new CSharpBuilder { nameSpace = NameSpace };
            Class clss = new Class(ClassName)
            {
                modifier = Modifier.Public | Modifier.Static | Modifier.Partial
            };

            builder.AddUsing("System");

            clss.AddRange(elements);

            builder.AddClass(clss);
            return builder;
        }


        private string ReadCode(DataTable dt)
        {
            string columnKey = cmd.GetValue("key");
            string columnDefaultValue = cmd.GetValue("default");

            if (columnKey != null && !dt.Columns.Contains(columnKey))
            {
                stdio.Error($"column [{columnKey}] not found in [{dt.TableName}]");
                return string.Empty;
            }

            if (columnDefaultValue != null && !dt.Columns.Contains(columnDefaultValue))
            {
                stdio.Error($"column [{columnDefaultValue}] not found in [{dt.TableName}]");
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            foreach (DataRow row in dt.Rows)
            {
                string key;
                string val;

                if (columnKey != null)
                    key = row[columnKey].ToString();
                else
                    key = row[0].ToString();

                if (columnDefaultValue != null)
                    val = row[columnDefaultValue].ToString();
                else
                    val = "0";

                builder.AppendLine($"{key}={val};");
            }

            return builder.ToString();
        }

        private string ReadCode()
        {
            string path = cmd.GetValue("in");
            if (path == null)
                return null;

            if (!File.Exists(path))
            {
                stdio.Error($"file {path} not found");
                return null;
            }

            return File.ReadAllText(path);
        }
    }
}


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

            ConstKey = 0x01,        //  public const string _MATH_PI = "Math.PI";
            DefaultValue = 0x02,    //  public static double __MATH_PI = 3.14;
            StaticField = 0x04,     //  public static double MATH_PI = GetValue<double>(_MATH_PI, __MATH_PI);
            StaticPropery = 0x08,   //  public static double MATH_PI => GetValue<double>(_MATH_PI, __MATH_PI);
            Hierarchy = 0x10,       //  public static Math { public static class Pi => GetValue<double>(_MATH_PI, __MATH_PI); }

            All = ConstKey | DefaultValue | StaticPropery
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

            ClassType classType = getClassType();
            bool hierarchy = (classType & ClassType.Hierarchy) != ClassType.Hierarchy;

            var builder = new CSharpBuilder { nameSpace = NameSpace };
            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            string cname = ClassName;

            var maker = new ConfigScript(cname, code);
            maker.IsHierarchicalProperty = hierarchy;
            var clss = maker.Generate();
            builder.AddClass(clss);

            switch (classType)
            {
                case ClassType.All:
                    clss.AddRange(maker.ConstKeyFields);
                    clss.AddRange(maker.StaticFields);
                    clss.AddRange(maker.StaticProperties);
                    clss.AddRange(maker.DefaultValueFields);
                    break;

                case ClassType.ConstKey:
                    builder = CreateClass(maker.ConstKeyFields);
                    break;

                case ClassType.DefaultValue:
                    builder = CreateClass(maker.DefaultValueFields);
                    break;

                case ClassType.StaticField:
                    builder = CreateClass(maker.StaticFields);
                    break;

                case ClassType.StaticPropery:
                    builder = CreateClass(maker.StaticProperties);
                    break;

                case ClassType.Hierarchy:
                    break;
            }

            PrintOutput(builder, cname);
        }


        private ClassType getClassType()
        {
            string _dataType = cmd.GetValue("type") ?? "all";
            switch (_dataType)
            {
                case "const":
                    return ClassType.ConstKey;

                case "default":
                    return ClassType.DefaultValue;

                case "field":
                    return ClassType.StaticField;

                case "property":
                    return ClassType.StaticPropery;

                case "hierarchy":
                    return ClassType.Hierarchy;
            }

            return ClassType.All;
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


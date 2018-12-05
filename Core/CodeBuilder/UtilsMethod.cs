using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    class UtilsMethod
    {
        private string className;
        private IEnumerable<string> variables;

        TypeInfo classType;
        public UtilsMethod(string className, IEnumerable<string> variables)
        {
            this.className = className;
            this.variables = variables;
            this.classType = new TypeInfo { userType = className };

        }
        public Method Map()
        {
            return Assign("Map");
        }
        public Method Copy()
        {
            return Assign("Copy");
        }

        private Method Assign(string methodName)
        {
            Method mtd = new Method(methodName)
            {
                modifier = Modifier.Public,
            };

            mtd.args.Add(className, "obj");

            var sent = mtd.statements;

            foreach (var variable in variables)
            {
                sent.AppendFormat("this.{0} = obj.{0};", variable);
            }

            return mtd;
        }
        public Method CopyTo()
        {
            Method mtd = new Method("CopyTo")
            {
                modifier = Modifier.Public | Modifier.Static,
                IsExtensionMethod = true
            };

            mtd.args.Add(className, "from");
            mtd.args.Add(className, "to");

            var sent = mtd.statements;

            foreach (var variable in variables)
            {
                sent.AppendFormat("to.{0} = from.{0};", variable);
            }

            return mtd;
        }


        public Method Clone()
        {
            Method mtd = new Method(classType, "Clone")
            {
                modifier = Modifier.Public,
            };

            var sent = mtd.statements;

            sent.AppendFormat("var obj = new {0}();", className);
            sent.AppendLine();

            foreach (var variable in variables)
            {
                sent.AppendFormat("obj.{0} = this.{0};", variable);
            }

            sent.AppendLine();
            sent.RETURN("obj");

            return mtd;
        }

        public Method CloneFrom()
        {
            Method mtd = new Method(classType, "Clone")
            {
                modifier = Modifier.Public | Modifier.Static,
                IsExtensionMethod = true
            };

            mtd.args.Add(className, "from");
            var sent = mtd.statements;

            sent.AppendFormat("var obj = new {0}();", className);
            sent.AppendLine();

            foreach (var variable in variables)
            {
                sent.AppendFormat("obj.{0} = from.{0};", variable);
            }

            sent.AppendLine();
            sent.RETURN("obj");

            return mtd;
        }

        public Method Equals()
        {
            Method mtd = new Method(new TypeInfo { type = typeof(bool) }, "Equals")
            {
                modifier = Modifier.Public | Modifier.Override,
            };

            mtd.args.Add<object>("obj");

            var sent = mtd.statements;
            sent.AppendFormat("var x = ({0})obj;", className);
            sent.AppendLine();

            sent.AppendLine("return ");

            variables.ForEach(
                variable => sent.Append($"this.{variable}.Equals(x.{variable})"),
                variable => sent.AppendLine("&& ")
                );

            sent.Append(";");
            return mtd;
        }

        public Method _GetHashCode()
        {
            Method mtd = new Method(new TypeInfo { type = typeof(int) }, "GetHashCode")
            {
                modifier = Modifier.Public | Modifier.Override,
            };

            var sent = mtd.statements;
            sent.AppendLine("return 0;");
            return mtd;
        }

        public Method Compare()
        {
            Method mtd = new Method(new TypeInfo { type = typeof(bool) }, "Compare")
            {
                modifier = Modifier.Public,
            };

            mtd.args.Add(className, "obj");

            var sent = mtd.statements;

            sent.AppendLine("return ");

            variables.ForEach(
                variable => sent.Append($"this.{variable}.Equals(obj.{variable})"),
                variable => sent.AppendLine("&& ")
                );

            sent.Append(";");
            return mtd;
        }


        public Method CompareTo()
        {
            Method mtd = new Method(new TypeInfo { type = typeof(bool) }, "CompareTo")
            {
                modifier = Modifier.Public | Modifier.Static,
                IsExtensionMethod = true
            };

            mtd.args.Add(className, "a");
            mtd.args.Add(className, "b");

            var sent = mtd.statements;

            sent.AppendLine("return ");

            variables.ForEach(
                variable => sent.Append($"a.{variable} == b.{variable}"),
                variable => sent.AppendLine("&& ")
                );

            sent.Append(";");
            return mtd;
        }

        public Method ToSimpleString()
        {
            Method mtd = new Method(new TypeInfo { type = typeof(string) }, "ToSimpleString")
            {
                modifier = Modifier.Public | Modifier.Static,
                IsExtensionMethod = true
            };

            mtd.args.Add(className, "obj");

            var sent = mtd.statements;


            StringBuilder builder = new StringBuilder("\"{{");
            int index = 0;
            variables.ForEach(
                variable => builder.Append($"{variable}:{{{index++}}}"),
                variable => builder.Append(", ")
                );

            builder.AppendLine("}}\", ");

            variables.ForEach(
                variable => builder.Append($"obj.{variable}"),
                variable => builder.AppendLine(", ")
                );

            sent.AppendFormat("return string.Format({0});", builder);
            return mtd;
        }

        public Method _ToString()
        {
            Method mtd = new Method(new TypeInfo { type = typeof(string) }, "ToString")
            {
                modifier = Modifier.Public | Modifier.Override,
            };

            var sent = mtd.statements;


            StringBuilder builder = new StringBuilder("\"{{");
            int index = 0;
            variables.ForEach(
                variable => builder.Append($"{variable}:{{{index++}}}"),
                variable => builder.Append(", ")
                );

            builder.AppendLine("}}\", ");

            variables.ForEach(
                variable => builder.Append($"this.{variable}"),
                variable => builder.AppendLine(", ")
                );

            sent.AppendFormat("return string.Format({0});", builder);
            return mtd;
        }

        public Method _ToString_v2()
        {
            Method mtd = new Method(new TypeInfo { type = typeof(string) }, "ToString")
            {
                modifier = Modifier.Public | Modifier.Override,
            };

            var sent = mtd.statements;


            sent.Append("return ");
            sent.Append("$\"");
            variables.ForEach(
                variable => sent.Append($"{variable}:{{{variable}}}"),
                variable => sent.Append(", ")
                );
            sent.Append("\";");

            return mtd;
        }

    }

    [Flags]
    public enum UtilsThisMethod
    {
        Undefined = 0x00,
        Copy = 0x01,
        Clone = 0x02,
        Compare = 0x04,
        ToString = 0x08,
        Equals = 0x10,
        GetHashCode = 0x20,
        Map = 0x40,
    }

    [Flags]
    public enum UtilsStaticMethod
    {
        Undefined = 0x00,
        CopyTo = 0x01,
        CloneFrom = 0x02,
        CompareTo = 0x04,
        ToSimpleString = 0x08
    }
}

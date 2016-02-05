using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    class Utils
    {
        private string className;
        private IEnumerable<string> variables;

        TypeInfo classType;
        public Utils(string className, IEnumerable<string> variables)
        {
            this.className = className;
            this.variables = variables;
            this.classType = new TypeInfo { userType = className };

        }

        public Method Copy()
        {
            Method mtd = new Method("Copy")
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

        public Method Equals()
        {
            Method mtd = new Method(new TypeInfo { type = typeof(bool)}, "Equals")
            {
                modifier = Modifier.Public | Modifier.Override,
            };

            mtd.args.Add<object>("obj");

            var sent = mtd.statements;
            sent.AppendFormat("var x = ({0})obj;", className);
            sent.AppendLine();

            sent.AppendLine("return ");

            variables.ForEach(
                variable => sent.Append($"this.{variable} == x.{variable}"),
                variable => sent.AppendLine("&& ")
                );

            sent.Append(";");
            return mtd;
        }
    }
}

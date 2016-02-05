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
            this.variables = variables;
            this.classType = new TypeInfo { userType = className };

        }

        public Method Copy()
        {
            Method mtd = new Method(classType, "Copy")
            {
                modifier = Modifier.Public,
            };

            mtd.args.Add(new Argument(classType, "obj"));

            foreach (var variable in variables)
            {
                mtd.statements.AppendFormat("this.{0} = obj.{0};", variable);
            }

            return mtd;
        }

        public Method Clone()
        {
            Method mtd = new Method(classType, "Clone")
            {
                modifier = Modifier.Public,
            };

            mtd.statements.AppendFormat("var obj = new {0}();", className);
            foreach (var variable in variables)
            {
                mtd.statements.AppendFormat("obj.{0} = this.{0};", variable);
            }

            return mtd;
        }

    }
}

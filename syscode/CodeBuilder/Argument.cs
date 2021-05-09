using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Argument
    {
        public string NamedArg { get; set; }
        public Expression Arg { get; set; }

        public Argument(Expression arg)
        {
            this.Arg = arg;
        }

        public Argument(string namedArg, Expression arg)
        {
            this.NamedArg = namedArg;
            this.Arg = arg;
        }

        public static implicit operator Argument(Expression expr)
        {
            return new Argument(expr);
        }

        public static implicit operator Argument(string argument)
        {
            return new Argument(argument);
        }

        public override string ToString()
        {
            if (NamedArg != null)
                return $"{NamedArg} : {Arg}";
            else
                return Arg.ToString();
        }
    }
}

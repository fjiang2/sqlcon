using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Arguments
    {
        public Argument[] args { get; set; } = new Argument[] { };

        public Arguments()
        {
        }

        public Arguments(Argument arg)
        {
            args = new Argument[] { arg };
        }

        public Arguments(Argument arg1, Argument arg2)
        {
            args = new Argument[] { arg1, arg2 };
        }


        public bool IsEmpty
        {
            get
            {
                return args == null || args.Length == 0;
            }
        }

        public override string ToString()
        {
            return string.Join(", ", args.Select(arg => arg.ToString()));
        }
    }
}

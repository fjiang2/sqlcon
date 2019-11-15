using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Arguments
    {
        private List<Argument> arguments { get; } = new List<Argument>();

        public Arguments(params Argument[] arguments)
        {
            this.arguments.AddRange(arguments);
        }

        public void Add(Argument argument)
        {
            this.arguments.Add(argument);
        }

        public override string ToString()
        {
            return string.Join(", ", arguments);
        }
    }
}

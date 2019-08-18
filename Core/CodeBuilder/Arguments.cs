using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Arguments
    {
        private List<Expression> arguments { get; } = new List<Expression>();

        public Arguments(params Expression[] arguments)
        {
            this.arguments.AddRange(arguments);
        }

        public void Add(Expression argument)
        {
            this.arguments.Add(argument);
        }

        public override string ToString()
        {
            return string.Join(", ", arguments);
        }
    }
}

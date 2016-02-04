using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Arguments
    {
        private List<Argument> args = new List<Argument>();

        public Arguments()
        {
        }

        public Arguments(IEnumerable<Argument> args)
        {
            foreach (var arg in args)
                this.args.Add(arg);
        }

        public Arguments Add(Argument arg)
        {
            args.Add(arg);
            return this;
        }

        public Arguments Add(string userType, string name)
        {
            var arg = new Argument(new TypeInfo { userType = userType }, name);

            args.Add(arg);
            return this;
        }

        public Arguments Add<T>(string name)
        {
            var arg = new Argument(new TypeInfo { type = typeof(T) }, name);
            args.Add(arg);

            return this;
        }

        public bool IsEmpty
        {
            get
            {
                return args.Count == 0;
            }
        }

        public override string ToString()
        {
            return string.Join(", ", args.Select(arg => arg.ToString()));
        }
    }
}

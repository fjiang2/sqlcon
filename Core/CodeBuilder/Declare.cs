using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Declare  
    {
        public Attribute attribute { get; set; }

        public Modifier modifier { get; set; } = Modifier.Public;
        public TypeInfo type { get; set; } = new TypeInfo();

        protected string name;

        public Declare(string name)
        {
            this.name = name;
        }

        protected string Signture
        {
            get { return string.Format("{0} {1} {2}", new ModifierString(modifier), type, name); }
        }

        public override string ToString()
        {
            return Signture;
        }
    }
}

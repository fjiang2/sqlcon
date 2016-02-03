using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Attribute
    {
        public string name { get; set; }
        public string[] args { get; set; }
        
        public Attribute(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            if (args == null)
                return string.Format("[{0}]", name);
            else
                return string.Format("[{0}({1})]", name, string.Join(",", args));
        }
    }
}

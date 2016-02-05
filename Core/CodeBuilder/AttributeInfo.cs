using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class AttributeInfo
    {
        public string name { get; set; }
        public string[] args { get; set; }

        public Comment comment { get; set; }

        public AttributeInfo(string name)
        {
            this.name = name;
        }

    
        public override string ToString()
        {
            string text;
            if (args == null)
                text = string.Format("[{0}]", name);
            else
                text = string.Format("[{0}({1})]", name, string.Join(", ", args));

            if (comment != null)
                return $"{text}{comment}";
            else
                return text;
        }
    }
}

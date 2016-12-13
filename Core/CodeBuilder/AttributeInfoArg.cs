using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tie;

namespace Sys.CodeBuilder
{
    public class AttributeInfoArg
    {
        public string name { get; set; }
        public object value { get; set; }

        public AttributeInfoArg(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            string text;
            text = string.Format("{0} = {1}", name, value);
            return text;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Declare  : ICodeBlock
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
            get
            {
                if(type != null)
                    return string.Format("{0} {1} {2}", new ModifierString(modifier), type, name);
                else
                    return string.Format("{0} {1}", new ModifierString(modifier), name);
            }
        }

        protected CodeBlock block = new CodeBlock();

        public virtual CodeBlock GetBlock()
        {
            if (attribute != null)
                block.AppendLine(attribute.ToString());

            return block;
        }

        public override string ToString()
        {
            return GetBlock().ToString();
        }
    }
}

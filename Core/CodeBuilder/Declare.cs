using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Declare  : Buildable
    {
        public AttributeInfo attribute { get; set; }

        public Modifier modifier { get; set; } = Modifier.Public;
        public TypeInfo type { get; set; } = new TypeInfo();

        public Comment comment { get; set; }

        protected string name;

        public Declare(string name)
        {
            this.name = name;
        }

        public AttributeInfo AddAttribute<T>() where T : Attribute
        {
            var name = typeof(T).Name;
            name = name.Substring(0, name.Length - nameof(Attribute).Length);
            attribute = new AttributeInfo(name);

            return attribute;
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

      


        protected override CodeBlock BuildBlock()
        {
            CodeBlock block = base.BuildBlock();

            if (attribute != null)
                block.AppendLine(attribute.ToString());

            return block;
        }

    }
}

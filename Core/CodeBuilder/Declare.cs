using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Declare : Buildable
    {
        private List<AttributeInfo> attributes { get; set; } = new List<AttributeInfo>();

        public Modifier modifier { get; set; } = Modifier.Public;
        public TypeInfo type { get; set; } = new TypeInfo();

        public Comment comment { get; set; }

        protected string name;

        public Declare(string name)
        {
            this.name = name;
        }

        public void AddAttribute(AttributeInfo attr)
        {
            attributes.Add(attr);
        }

        public AttributeInfo AddAttribute<T>() where T : Attribute
        {
            var name = typeof(T).Name;
            name = name.Substring(0, name.Length - nameof(Attribute).Length);
            var attr = new AttributeInfo(name);
            attributes.Add(attr);

            return attr;
        }


        protected string Signture
        {
            get
            {
                if (type != null)
                    return string.Format("{0} {1} {2}", new ModifierString(modifier), type, name);
                else
                    return string.Format("{0} {1}", new ModifierString(modifier), name);
            }
        }




        protected override CodeBlock BuildBlock()
        {
            CodeBlock block = base.BuildBlock();

            if (attributes.Count != 0)
            {
                foreach (var attr in attributes)
                    block.AppendLine(attr.ToString());
            }

            return block;
        }

    }
}

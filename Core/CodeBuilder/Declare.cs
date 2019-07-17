using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Declare : Buildable
    {
        protected List<AttributeInfo> attributes { get; } = new List<AttributeInfo>();

        public Modifier modifier { get; set; } = Modifier.Public;
        public TypeInfo type { get; set; } = new TypeInfo();

        public Comment comment { get; set; }

        public string name { get; }

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
            this.AddAttribute(attr);

            return attr;
        }


        protected string Signature 
        {
            get
            {
                if (type != null)
                    return string.Format("{0} {1} {2}", new ModifierString(modifier), type, name);
                else
                    return string.Format("{0} {1}", new ModifierString(modifier), name);
            }
        }




        protected override void BuildBlock(CodeBlock block)
        {

            if (attributes.Count != 0)
            {
                foreach (var attr in attributes)
                    block.AppendLine(attr.ToString());
            }

        }

    }
}

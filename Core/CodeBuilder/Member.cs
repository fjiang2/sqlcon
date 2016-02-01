using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Member :Format
    {
        public AccessModifier modifier { get; set; }
        public TypeInfo type { get; set; }

        protected string name;

        public Member(string name)
        {
            this.name = name;
        }

        protected virtual string signature
        {
            get
            {
                return string.Format("{0} {1} {2}",
                new Modifier(modifier),
                type,
                name
                );
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", new Modifier(modifier), type, name);
        }
    }
}

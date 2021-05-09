using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Identifier
    {
        private readonly string name;

        public Identifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("identifier cannot be blank");

            this.name = name;
        }


        public override bool Equals(object obj)
        {
            Identifier id = obj as Identifier;
            if (id != null)
                return this.name.Equals(id.name);

            return false;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override string ToString()
        {
            return name;
        }

        public static bool operator ==(Identifier id1, Identifier id2)
        {
            return id1.name.Equals(id2.name);
        }

        public static bool operator !=(Identifier id1, Identifier id2)
        {
            return !id1.name.Equals(id2.name);
        }

        public static implicit operator Identifier(string ident)
        {
            return new Identifier(ident);
        }

        public static explicit operator string(Identifier ident)
        {
            return ident.name;
        }
    }
}

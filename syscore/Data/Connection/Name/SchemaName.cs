using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Data
{
    public class SchemaName
    {
        public static readonly SchemaName Dbo = new SchemaName(dbo);
        public const string dbo = "dbo";

        private string name;

        public SchemaName()
        {
            this.name = string.Empty;
        }

        public SchemaName(string name)
        {
            this.name = name;
        }

        public bool IsDbo => name == dbo;

        public bool IsEmpty => string.IsNullOrEmpty(name);

        public override bool Equals(object obj)
        {
            SchemaName schemaName = obj as SchemaName;
            if (obj == null)
                return false;

            return name.ToLower().Equals(schemaName.name.ToLower());
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override string ToString() => name;

        public static explicit operator string(SchemaName a)
        {
            return a.name;
        }

        public static implicit operator SchemaName(string name)
        {
            return new SchemaName(name);
        }

        public static bool operator ==(SchemaName a, SchemaName b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SchemaName a, SchemaName b)
        {
            return !a.Equals(b);
        }
    }
}

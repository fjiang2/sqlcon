using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data.Manager
{
    class PropertyDefinition
    {
        public readonly string PropertyName;
        public readonly string Type;

        public PropertyDefinition(string type, string name)
        {
            this.PropertyName = name;
            this.Type = type;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Type, PropertyName);
        }
    }
}

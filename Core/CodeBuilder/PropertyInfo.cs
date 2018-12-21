using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class PropertyInfo
    {
        public TypeInfo PropertyType { get; set; }
        public string PropertyName { get; set; }

        public override string ToString()
        {
            return PropertyName;
        }
    }
}

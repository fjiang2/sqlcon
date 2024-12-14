using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tie;

namespace Sys.CodeBuilder
{

    public static class ValueExtension
    {
  
        public static Value NewPropertyObject(TypeInfo type)
        {
            return new Value(new Dictionary<string, Value>()) { Type = type };
        }

        public static string ToPrimitive(object value)
        {
            //make double value likes integer, e.g. ToPrimitive(25.0) returns "25, ToPrimitive(25.3) returns "25.3"
            if (value is double)
            {
                return value.ToString();
            }
            else if (value is Guid)
            {
                return $"new Guid(\"{value}\")";
            }
            else if (value is CodeString)
            {
                return value.ToString();
            }
            else if (value is byte[])
            {
                var hex = (value as byte[])
                    .Select(b => $"0x{b:X}")
                    .Aggregate((b1, b2) => $"{b1},{b2}");
                return "new byte[] {" + hex + "}";
                //return "new byte[] {0x" + BitConverter.ToString((byte[])value).Replace("-", ",0x") + "}";
            }

            return VAL.Boxing(value).ToString();
        }

 
    }

}

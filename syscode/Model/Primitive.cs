//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        syscode(C# Code Builder)                                                                  //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sys.CodeBuilder
{
    public static class Primitive
    {
        public static string ToPrimitive(object obj)
        {
            //make double value likes integer, e.g. ToPrimitive(25.0) returns "25, ToPrimitive(25.3) returns "25.3"
            switch (obj)
            {
                case double value:
                    return value.ToString();

                case CodeString value:
                    return value.ToString();

                case Guid value:
                    return $"new Guid(\"{value}\")";

                case DateTime time:
                    return $"new DateTime({time.Year}, {time.Month}, {time.Day}, {time.Hour}, {time.Minute}, {time.Second})";

                case DateTimeOffset time:
                    return $"new DateTimeOffset({time.Year}, {time.Month}, {time.Day}, {time.Hour}, {time.Minute}, {time.Second}, {time.Offset})";

                
                case byte[] value:
                    {
                        var hex = value
                            .Select(b => $"0x{b:X}")
                            .Aggregate((b1, b2) => $"{b1},{b2}");
                        return "new byte[] {" + hex + "}";
                        //return "new byte[] {0x" + BitConverter.ToString((byte[])value).Replace("-", ",0x") + "}";
                    }
            }

            return ToCodeString(obj);
        }

        internal static string ToCodeString(this object obj)
        {
            StringWriter o = new StringWriter();

            switch (obj)
            {
                case null:
                    o.Write("null");
                    break;

                case bool value:
                    o.Write(value ? "true" : "false");
                    break;

                case char value:
                    o.Write("'{0}'", value);
                    break;

                case string value:
                    o.Write("\"");
                    for (int i = 0; i < value.Length; i++)
                    {
                        switch (value[i])
                        {
                            case '"':
                                o.Write("\\\"");
                                break;

                            case '\\':
                                o.Write("\\\\");
                                break;

                            case '\n':
                                o.Write("\\n");
                                break;

                            case '\r':
                                o.Write("\\r");
                                break;

                            case '\t':
                                o.Write("\\t");
                                break;

                            default:
                                o.Write(value[i]);
                                break;
                        }

                    }
                    o.Write("\"");
                    break;

                case Enum value:
                    o.Write(EnumBitFlags(value));
                    break;

                case byte b:
                    return $"0x{b:X}";

                default:
                    o.Write(obj);
                    break;

            }
            return o.ToString();
        }

        private static string EnumBitFlags(object host)
        {
            Type type = host.GetType();
            string fullName = type.FullName;
            if (Enum.IsDefined(type, host))
            {
                return string.Format("{0}.{1}", fullName, host);
            }

            string s = "";

            foreach (var fieldInfo in type.GetFields())
            {
                if (!fieldInfo.IsLiteral)
                    continue;

                int offset = (int)fieldInfo.GetValue(type);
                if (offset != 0 && ((int)host & offset) == offset)
                {
                    if (s != "")
                        s += "|";
                    s += string.Format("{0}.{1}", fullName, Enum.ToObject(type, offset).ToString());
                }
            }

            return s;

        }

    }
}

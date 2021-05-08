using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sys.CodeBuilder
{
    static class Extension
    {
        public static string ToCodeString(this object obj)
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

                case DateTime time:
                    o.Write($"new DateTime({time.Year}, {time.Month}, {time.Day}, {time.Hour}, {time.Minute}, {time.Second})");
                    break;

                case DateTimeOffset time:
                    o.Write($"new DateTimeOffset({time.Year}, {time.Month}, {time.Day}, {time.Hour}, {time.Minute}, {time.Second}, {time.Offset})");
                    break;

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

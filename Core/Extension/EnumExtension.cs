using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Sys
{
    public static class EnumExtension
    {


        public static string EnumBitFlags(object host)
        {
            Type type = host.GetType();

            string name = host.ToString();
            if ((name[0] < '0' || name[0] > '9'))
                return name;

            string s = "";

            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (!fieldInfo.IsLiteral)
                    continue;

                int offset = (int)fieldInfo.GetValue(type);
                if (offset != 0 && ((int)host & offset) == offset)
                {
                    if (s != "")
                        s += " | ";
                    s += Enum.ToObject(type, offset).ToString();
                }
            }

            return s;

        }

        public static Dictionary<int, string> EnumBitValues(this Type enumType)
        {
            int sum = 0;
            foreach (object ty in Enum.GetValues(enumType))
            {
                sum += (int)ty;
            }

            Dictionary<int, string> L = new Dictionary<int, string>();
            for (int ty = 0; ty <= sum; ty++)
            {
                string name = EnumBitFlags(Enum.ToObject(enumType, ty));
                L.Add(ty, name);
            }

            return L;
        }

    }
}

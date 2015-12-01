using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Sys.Data
{
    class CustomAttributeProvider
    {
        static Dictionary<Type, Dictionary<MemberInfo, Attribute[]>> cache = new Dictionary<Type, Dictionary<MemberInfo, Attribute[]>>();

        public static T[] GetAttributes<T>(MemberInfo memberInfo) where T : Attribute
        {
            Dictionary<MemberInfo, Attribute[]> dict;
            if (cache.ContainsKey(typeof(T)))
            {
                dict = cache[typeof(T)];
            }
            else
            {
                dict = new Dictionary<MemberInfo, Attribute[]>();
                cache.Add(typeof(T), dict);
            }

            T[] attributes;
            if (dict.ContainsKey(memberInfo))
            {
                attributes = (T[])dict[memberInfo];
            }
            else
            {
                attributes = (T[])memberInfo.GetCustomAttributes(typeof(T), true);
                dict.Add(memberInfo, attributes);
            }

            return attributes;
        }

    }
}

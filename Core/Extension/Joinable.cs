using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys
{
    public static class Joinable
    {
        public static void Foreach<TSource>(this IEnumerable<TSource> source, Action<TSource> action, Action delimiter)
        {
            bool first = true;

            foreach (var i in source)
            {
                if (!first)
                    delimiter();

                first = false;
                action(i);
            }
        }

        public static string Join<TSource>(this IEnumerable<TSource> source, string delimiter)
        {
            StringBuilder builder = new StringBuilder();
            source.Foreach(i => builder.Append(i), () => builder.Append(delimiter));
            return builder.ToString();
        }
    }
}

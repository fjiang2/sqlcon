using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys
{
    public static class Iteratable
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action, Action<T> delimiter)
        {
            bool first = true;

            foreach (var item in items)
            {
                if (!first)
                    delimiter(item);

                first = false;
                action(item);
            }
        }

        public static string Join<T>(this IEnumerable<T> items, string delimiter)
        {
            StringBuilder builder = new StringBuilder();
            items.ForEach(
                item => builder.Append(item), 
                _ => builder.Append(delimiter)
             );

            return builder.ToString();
        }
    }
}

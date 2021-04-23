using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys
{
    static class Iteratable
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

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

     
    }
}

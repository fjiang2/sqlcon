using System;
using System.Collections;
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

        public static void ForEach<T>(this IEnumerable items, Action<T> action, Action<T> delimiter)
        {
            bool first = true;

            foreach (var item in items)
            {
                if (!first)
                    delimiter((T)item);

                first = false;
                action((T)item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        public static string Concatenate<T>(this IEnumerable<T> items, string delimiter)
        {
            StringBuilder builder = new StringBuilder();
            items.ForEach(
                item => builder.Append(item),
                _ => builder.Append(delimiter)
             );

            return builder.ToString();
        }

        public static string Concatenate(this IEnumerable items, string delimiter)
        {
            StringBuilder builder = new StringBuilder();
            items.ForEach<object>(
                item => builder.Append(item),
                _ => builder.Append(delimiter)
             );

            return builder.ToString();
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="convert">convert substring to typeof(T)</param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static IEnumerable<T> Split<T>(this string text, Func<string, T> convert, string separator)
        {
            string[] items = text.Split(new string[] { separator }, StringSplitOptions.None);

            List<T> list = new List<T>();

            foreach (var item in items)
            {
                list.Add(convert(item));
            }

            return list;
        }

        public static IEnumerable<T> Split<T>(this string text, Func<string, T> convert)
        {
            return Split(text, convert, ",");
        }

    }

}

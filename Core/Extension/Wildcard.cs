using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Sys
{
    public static class Wildcard
    {
        public static IEnumerable<TSource> IsMatch<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, IEnumerable<string> patterns)
        {
            return source.Where(x => IsMatch(patterns, keySelector(x)));
        }

        public static IEnumerable<TSource> IsMatch<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, string pattern)
        {
            return source.Where(x => pattern.IsMatch(keySelector(x)));
        }

        public static bool IsMatch<TSource>(this TSource source, IEnumerable<string> patterns, Func<TSource, string> keySelector)
        {
            return IsMatch(patterns, keySelector(source));
        }

        public static bool IsMatch(this IEnumerable<string> patterns, string text)
        {
            foreach (var pattern in patterns)
            {
                if (IsMatch(pattern, text))
                    return true;
            }

            return false;
        }

        public static bool IsMatch(this string pattern, string text)
        {
            if (pattern.IndexOf('?') == -1 && pattern.IndexOf('*') == -1)
            {
                return pattern.ToUpper().Equals(text.ToUpper());
            }
            else
            {
                Regex regex = pattern.WildcardRegex();
                return regex.IsMatch(text);
            }
        }

        public static Regex WildcardRegex(this string pattern)
        {
            string x = "^" + Regex.Escape(pattern)
                                  .Replace(@"\*", ".*")
                                  .Replace(@"\?", ".")
                           + "$";

            Regex regex = new Regex(x, RegexOptions.IgnoreCase);
            return regex;
        }

    }
}

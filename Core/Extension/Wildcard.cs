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
        public static IEnumerable<TSource> IsMatch<TSource>(this IEnumerable<TSource> source, IEnumerable<string> patterns, Func<TSource, string> keySelector)
        {
            return source.Where(x => IsMatch(patterns, keySelector(x)));
        }


        public static bool IsMatch(this IEnumerable<string> patterns, string text)
        {
            bool matched;
            foreach (var pattern in patterns)
            {
                if (pattern.IndexOf('?') == -1 && pattern.IndexOf('*') == -1)
                {
                    matched = pattern.ToUpper().Equals(text.ToUpper());
                }
                else
                {
                    matched = IsMatch(pattern, text);
                }

                if (matched)
                    return true;
            }

            return false;
        }

        public static bool IsMatch(this string pattern, string text)
        {
            Regex regex = pattern.WildcardRegex();
            return regex.IsMatch(text);
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

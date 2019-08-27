using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Sys
{
    public static class Matchable
    {
        public static IEnumerable<TSource> IsMatch<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, IWildcard wildcard)
        {
            Wildcard<TSource> x = new Wildcard<TSource>(keySelector)
            {
                Pattern = wildcard.Pattern,
                Includes = wildcard.Includes,
                Excludes = wildcard.Excludes,
            };

            return x.Results(source);
        }

        public static IEnumerable<TSource> IsMatch<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, string pattern)
        {
            return source.Where(x => keySelector(x).IsMatch(pattern));
        }


        public static bool IsMatch(this string text, string pattern)
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

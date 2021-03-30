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
        public static IEnumerable<TSource> Matches<TSource>(this IEnumerable<TSource> source, Func<TSource, string> selector, IWildcard wildcard)
        {
            Wildcard<TSource> x = new Wildcard<TSource>(selector)
            {
                Pattern = wildcard.Pattern,
                Includes = wildcard.Includes,
                Excludes = wildcard.Excludes,
            };

            return x.Results(source);
        }

        public static IEnumerable<TSource> Matches<TSource>(this IEnumerable<TSource> source, Func<TSource, string> selector, string wildcard)
        {
            return source.Where(x => selector(x).IsMatch(wildcard));
        }

        public static bool IsMatch(this string text, IEnumerable<string> patterns)
        {
            if (patterns == null)
                return false;

            if (patterns.Count() == 0)
                return false;

            foreach (string pattern in patterns)
            {
                if (text.IsMatch(pattern))
                    return true;
            }

            return false;
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

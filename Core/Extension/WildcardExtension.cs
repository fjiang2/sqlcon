using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Sys
{
    public static class WildcardExtension
    {
        public static IEnumerable<TSource> IsMatch<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, Wildcard wildcard)
        {
            Wildcard<TSource> x = new Wildcard<TSource>(keySelector)
            {
                Pattern = wildcard.Pattern,
                Excludes = wildcard.Excludes,
                Includes = wildcard.Includes,
            };

            return x.Results(source);
        }

        public static IEnumerable<TSource> IsMatch<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, IEnumerable<string> patterns)
        {
            return source.Where(x => keySelector(x).IsMatch(patterns));
        }

        public static IEnumerable<TSource> IsMatch<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, string pattern)
        {
            return source.Where(x => IsMatch(keySelector(x), pattern));
        }

        public static bool IsMatch<TSource>(this TSource source, IEnumerable<string> patterns, Func<TSource, string> keySelector)
        {
            return keySelector(source).IsMatch(patterns);
        }

        public static bool IsMatch(this string text, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                if (IsMatch(text, pattern))
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

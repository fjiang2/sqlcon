using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sys;
using Sys.Data;

namespace Sys
{
    public class Wildcard<T> : Wildcard
    {
        private Func<T, string> selector;

        public Wildcard(Func<T, string> selector)
        {
            this.selector = selector;
        }

        public T[] Results(IEnumerable<T> tnames)
        {
            var names = tnames
                .Where(name => Include(name) && !Exclude(name))
                .ToArray();

            if (Pattern == null)
                return names;

            names = Search(Pattern, names);

            return names;
        }

        public bool Contains(T tname)
        {
            if (!Include(tname) || Exclude(tname))
                return false;

            if (Pattern == null)
                return true;

            return selector(tname).IsMatch(Pattern);
        }

        private bool Include(T tname)
        {
            if (Includes == null || Includes.Length == 0)
                return true;

            return IsMatch(selector(tname), Includes);
        }

        private bool Exclude(T tname)
        {
            if (Excludes == null || Excludes.Length == 0)
                return false;

            return IsMatch(selector(tname), Excludes);
        }

        private static bool IsMatch(string text, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                if (text.IsMatch(pattern))
                    return true;
            }

            return false;
        }

        private T[] Search(string pattern, T[] tnames)
        {
            return tnames.Where(x => selector(x).IsMatch(pattern)).ToArray();
        }

    }
}

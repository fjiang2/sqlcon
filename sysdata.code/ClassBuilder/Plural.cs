using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

#if WINDOWS
using System.Data.Entity.Design.PluralizationServices;
#endif

namespace Sys.Data.Code
{
    class Plural
    {
#if WINDOWS
        public static PluralizationService Pluralization
          => PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"));

        public static string Pluralize(string name) => Pluralization.Pluralize(name);
        public static string Singularize(string name) => Pluralization.Singularize(name);
#else
        public static string Pluralize(string name)
        {
            if (name.Length == 1)
                return name;

            if (name.IndexOf("_") > 0)
                return name;

            Dictionary<string, string> exceptions = new Dictionary<string, string>() {
                { "man", "men" },
                { "woman", "women" },
                { "child", "children" },
                { "tooth", "teeth" },
                { "foot", "feet" },
                { "mouse", "mice" },
                { "belief", "beliefs" } };

            if (exceptions.ContainsKey(name.ToLowerInvariant()))
            {
                return exceptions[name.ToLowerInvariant()];
            }

            if (name.EndsWith("y") && !name.EndsWith("ay") && !name.EndsWith("ey") && !name.EndsWith("iy") && !name.EndsWith("oy") && !name.EndsWith("uy"))
            {
                return name.Substring(0, name.Length - 1) + "ies";
            }

            if (name.EndsWith("us") || name.EndsWith("ss") || name.EndsWith("x") || name.EndsWith("ch") || name.EndsWith("sh") || name.EndsWith("o"))
            {
                return name + "es";
            }

            if (name.EndsWith("s"))
            {
                return name;
            }

            if (name.EndsWith("f") && name.Length > 1)
            {
                return name.Substring(0, name.Length - 1) + "ves";
            }

            if (name.EndsWith("fe") && name.Length > 2)
            {
                return name.Substring(0, name.Length - 2) + "ves";
            }

            return name + "s";
        }


        public static string Singularize(string word)
        {
            if (word.EndsWith("ss"))
                return word;
            if (word.EndsWith("ees"))
                word = word.Substring(0, word.Length - 1);
            else if (word.EndsWith("ies"))
                word = word.Substring(0, word.Length - 3) + "y";
            else if (word.EndsWith("es"))
            {
                char ch1 = word[word.Length - 3];
                char ch2 = word[word.Length - 4];

                if (!IsVowel(ch1))
                    word = word.Substring(0, word.Length - 1);
                else
                    word = word.Substring(0, word.Length - 2);
            }
            else if (word.EndsWith("s"))
            {
                char vowel = word[word.Length - 2];
                if (vowel != 'u')
                    word = word.Substring(0, word.Length - 1);
            }

            return word;
        }

#endif

        private static bool IsVowel(char ch)
        {
            return ch == 'a' || ch == 'e' || ch == 'i' || ch == 'o' || ch == 'u' || ch == 'y';
        }
    }
}

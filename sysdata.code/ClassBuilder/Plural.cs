using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

#if WINDOWS
using System.Data.Entity.Design.PluralizationServices;
#endif

namespace sqlcon
{
    class Plural
    {
#if WINDOWS
        public static PluralizationService Pluralization
          => PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"));

        public static string Pluralize(string name) => Pluralization.Pluralize(name);
        public static string Singularize(string name) => Pluralization.Singularize(name);
#else
        public static string Pluralize(string name) => name;
        public static string Singularize(string name) => name;
#endif

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace Sys.Data.Resource
{
    public class JsonFile
    {
        public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;
        public string Directory { get; set; } = ".";

        public JsonFile()
        {

        }

        public string FileName
        {
            get
            {
                if (CultureInfo == CultureInfo.InvariantCulture)
                    return "en.json";

                string shortName = CultureInfo.TwoLetterISOLanguageName.ToLower();

                return $"{shortName}.json";
            }
        }

        public string FullName => Path.Combine(Directory, FileName);

        public override string ToString()
        {
            return FullName;
        }
    }
}

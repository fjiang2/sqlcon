using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace Sys.Data.Resource
{
    class XlfFile : IResourceFile
    {
        public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;
        public string Directory { get; set; } = ".";

        public XlfFile()
        {

        }

        public string FileName
        {
            get
            {
                if (CultureInfo == CultureInfo.InvariantCulture)
                    return "messages.en.xlf";

                string shortName = CultureInfo.TwoLetterISOLanguageName.ToLower();

                return $"messages.{shortName}.xlf";
            }
        }

        public string FullName => Path.Combine(Directory, FileName);

        public override string ToString()
        {
            return FullName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace Sys.Data.Resource
{
    public class ResxFile
    {
        public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;
        public string Directory { get; set; } = ".";

        public ResxFile()
        {

        }

        public string FileName
        {
            get
            {
                if (CultureInfo == CultureInfo.InvariantCulture)
                    return "Resource.resx";

                string shortName = CultureInfo.TwoLetterISOLanguageName.ToLower();
                if (shortName == "en")
                    return "Resource.resx";

                return $"Resource.{shortName}.resx";
            }
        }

        public string FullName => Path.Combine(Directory, FileName);

        public override string ToString()
        {
            return FullName;
        }
    }
}
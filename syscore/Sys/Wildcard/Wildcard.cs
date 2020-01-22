using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sys;
using Sys.Data;

namespace Sys
{
    public class Wildcard : IWildcard
    {

        public string Pattern { get; set; }

    
        public string[] Includes { get; set; } = new string[] { };

   
        public string[] Excludes { get; set; } = new string[] { };

    }
}

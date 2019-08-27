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
        /// <summary>
        /// match all if it were null
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Inclusive pattern list
        /// include all if it were empty
        /// </summary>
        public string[] Includes { get; set; } = new string[] { };

        /// <summary>
        /// Exclusive pattern list
        /// Exclude nothing if it were empty
        /// </summary>
        public string[] Excludes { get; set; } = new string[] { };

    }
}

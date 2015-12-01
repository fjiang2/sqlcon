using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data;

namespace sqlcon
{
    class PathNode : IDataPath
    {
        public PathLevel Level {get; set;}
        public IDataPath Parent { get; set; }
        
        public PathNode()
        {
            this.Level = PathLevel.Unknown;
        }

        public string Path
        {
            get { return Level.ToString(); }
        }

        public override string ToString()
        {
            return Path;
        }
    }
}

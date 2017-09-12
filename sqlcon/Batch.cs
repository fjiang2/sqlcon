using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace sqlcon
{
    class Batch
    {
        private const string EXT = ".sqc";
        private string path;
        public bool IsBatch { get; } = false;

        public Batch(string path)
        {
            string ext = Path.GetExtension(path);

            //if no extension file defined
            if (ext == string.Empty)
            {
                this.path = $"{path}{EXT}";
                this.IsBatch = true;
            }
            else
            {
                this.path = path;
                this.IsBatch = ext == EXT;
            }
        }

        public bool Run(Configuration cfg, string[] args)
        {
            if (!IsBatch)
            {
                stdio.Error($"must be {EXT} file: {path}");
                return false;
            }

            if (Exists)
            {
                new SqlShell(cfg).DoBatch(path, args);
                return true;
            }
            else
            {
                stdio.Error($"file not found: {path}");
                return false;
            }
        }

        public bool Exists => File.Exists(path);

        public override string ToString()
        {
            return Path.GetFullPath(this.path);
        }
    }
}

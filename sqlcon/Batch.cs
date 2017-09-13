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
        private Configuration cfg;


        public bool IsBatch { get; } = false;

        public Batch(Configuration cfg, string path)
        {
            this.cfg = cfg;
            this.path = cfg.WorkingDirectory.GetFullPath(path, EXT);
            this.IsBatch = EXT == Path.GetExtension(this.path); ;
        }



        public bool Call(string[] args)
        {
            if (!IsBatch)
            {
                stdio.Error($"must be {EXT} file: {path}");
                return false;
            }

            if (Exists)
            {
                CallWithParameters(args);
                return true;
            }
            else
            {
                stdio.Error($"file not found: {path}");
                return false;
            }
        }

        /// <summary>
        /// parameters: %1 %2 %3 ...
        /// </summary>
        /// <param name="args"></param>
        private void CallWithParameters(string[] args)
        {
            string[] lines = File.ReadAllLines(path);

            List<string> L = new List<string>();
            foreach (string line in lines)
            {
                string cmd = line.Trim();
                if (cmd == string.Empty)
                    continue;

                for (int i = 1; i < args.Length; i++)
                {
                    cmd = cmd.Replace($"%{i}", args[i]);
                }

                L.Add(cmd);
            }

            new SqlShell(cfg).DoBatch(L);
        }

        public bool Exists => File.Exists(path);

        public override string ToString()
        {
            return Path.GetFullPath(this.path);
        }
    }
}

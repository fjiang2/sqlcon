using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sys.Data;
using Sys.Data.Comparison;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Tie;

namespace sqlcon
{
    class CompareConsole
    {
        Configuration cfg;

        public CompareConsole(Configuration cfg)
        {
            this.cfg = cfg;
        }


        public void Run(string[] args)
        {
            int i = 0;

            while (i < args.Length)
            {
                string arg = args[i++];
                switch (arg)
                {
                    case "/cfg":
                        i++;
                        break;


                    case "/i":
                        if (i < args.Length && !args[i].StartsWith("/"))
                        {
                            string inputfile = args[i++];
                            string server = cfg.GetValue<string>(Configuration._SERVER0);
                            var pvd = cfg.GetProvider(server);
                            var theSide = new Side(pvd);
                            theSide.ExecuteScript(inputfile);
                            break;
                        }
                        else
                        {
                            stdio.WriteLine("/i undefined sql script file name");
                            return;
                        }

                    case "/o":
                        if (i < args.Length && !args[i].StartsWith("/"))
                        {
                            cfg.OutputFile = args[i++];
                            break;
                        }
                        else
                        {
                            stdio.WriteLine("/o undefined sql script file name");
                            return;
                        }

                    default:
                        if (!string.IsNullOrEmpty(arg))
                            RunBatch(arg);
                        else
                            Program.Help();

                        return;
                }
            }


            new SqlShell(cfg).DoConsole();

        }

        const string EXT = ".sc";

        private bool RunBatch(string path)
        {
            string ext = Path.GetExtension(path);

            //no extension file
            if (ext == string.Empty)
            {
                path = $"{path}{EXT}";
            }
            else if (ext != EXT)
            {
                stdio.Error($"must be {EXT} file: {path}");
                return false;
            }


            if (File.Exists(path))
            {
                new SqlShell(cfg).DoBatch(path);
                return true;
            }

            stdio.Error($"file not found: {path}");

            return false;
        }
    }
}

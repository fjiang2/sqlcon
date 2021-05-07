using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Sys;
using Sys.Stdio;
using Sys.Data;

namespace sqlcon
{
    class Main
    {

        private ApplicationConfiguration cfg;
        public Shell Shell { get; private set; }

        public Main(ApplicationConfiguration cfg)
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
                            IConnectionConfiguration connection = cfg.Connection;
                            string inputfile = args[i++];
                            string server = connection.Home;
                            var pvd = connection.GetProvider(server);
                            var theSide = new Side(pvd);
                            theSide.ExecuteScript(inputfile, verbose: false);
                            break;
                        }
                        else
                        {
                            cout.WriteLine("/i undefined sql script file name");
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
                            cout.WriteLine("/o undefined sql script file name");
                            return;
                        }

                    default:
                        if (!string.IsNullOrEmpty(arg))
                            RunBatch(arg, args);
                        else
                            ShowHelp();

                        return;
                }
            }


            Shell = new Shell(cfg);
            Context.DS.AddHostObject(Context.SHELL, Shell);
            Shell.DoConsole();
        }



        private void RunBatch(string path, params string[] args)
        {
            Batch batch = new Batch(cfg, path);
            batch.Call(null, args);
        }

     

        public static void ShowHelp()
        {
            cout.WriteLine("SQL Server Command Console");
            cout.WriteLine("Usage: sqlcon");
            cout.WriteLine("     [/cfg configuration file name (.cfg)]");
            cout.WriteLine("     [/i sql script file name (.sql)]");
            cout.WriteLine("     [file] sqlcon command batch file name (.sqc)");
            cout.WriteLine();
            cout.WriteLine("/h,/?      : this help");
            cout.WriteLine($"/cfg       : congfiguration file default file: \"{ConfigurationEnvironment.Path.Personal}\"");
            cout.WriteLine("/i         : input sql script file name");
            cout.WriteLine("/o         : result of sql script");
            cout.WriteLine("examples:");
            cout.WriteLine("  sqlcon file1.sqc");
            cout.WriteLine("  sqlcon /cfg my.cfg");
            cout.WriteLine("  sqlcon /i script1.sql /o c:\\temp\\o.txt");
        }
    }
}

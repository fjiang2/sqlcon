using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sys;
using Tie;
using Sys.Stdio;

namespace sqlcon
{
    class Program
    {
        public static Configuration Configuration;

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "sqlcon";
            Console.WriteLine($"SQL Console [Version {SysExtension.ApplicationVerison}]");
            Console.WriteLine($"Copyright (c) 2014-{DateTime.Today.Year} Datconn. All rights reserved.");
            Console.WriteLine();

            Constant.MAX_CPU_REG_NUM = 2 * 1024;
            Constant.MAX_STRING_SIZE = 24 * 1024 * 1024;
            Constant.MAX_SRC_COL = 24 * 1024 * 1024;
            Constant.MAX_INSTRUCTION_NUM = 1 * 1024 * 1024;
            string cfgFile = sqlcon.Main.PrepareConfigureFile(false);

            int i = 0;
            while (i < args.Length)
            {
                switch (args[i++])
                {
                    case "/cfg":
                        if (i < args.Length && !args[i].StartsWith("/"))
                        {
                            cfgFile = args[i++];
                            goto L1;
                        }
                        else
                        {
                            cout.WriteLine("/cfg configuration file missing");
                            return;
                        }

                    case "/h":
                    case "/?":
                        sqlcon.Main.ShowHelp();
                        return;
                }

            }

            L1:


            Configuration = new Configuration();
            try
            {
                if (!Configuration.Initialize(cfgFile))
                    return;
            }
            catch (Exception ex)
            {
                cout.WriteLine("error on configuration file {0}, {1}:", cfgFile, ex.Message);
                return;
            }

#if DEBUG
            var site = new Main(Configuration);
            site.Run(args);
#else
            try
            {
                var site = new Main(Configuration);
                site.Run(args);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
                //stdio.ShowError(ex.StackTrace);
                cerr.WriteLine("fatal error, hit any key to exit");
                cin.ReadKey();
            }
#endif
        }


    }
}

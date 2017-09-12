using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sys;
using Tie;

namespace sqlcon
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "sqlcon";
            Console.WriteLine("SQL Console [Version {0}]", SysExtension.ApplicationVerison);
            Console.WriteLine("Copyright (c) 2014-2017 Datconn. All rights reserved.");
            Console.WriteLine();

            Constant.MAX_CPU_REG_NUM = 600;
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
                            stdio.WriteLine("/cfg configuration file missing");
                            return;
                        }

                    case "/h":
                    case "/?":
                        sqlcon.Main.ShowHelp();
                        return;
                }

            }

            L1:


            var cfg = new Configuration();
            try
            {
                if (!cfg.Initialize(cfgFile))
                    return;
            }
            catch (Exception ex)
            {
                stdio.WriteLine("error on configuration file {0}, {1}:", cfgFile, ex.Message);
                return;
            }

#if DEBUG
            var site = new Main(cfg);
            site.Run(args);
#else
            try
            {
                var site = new Main(cfg);
                site.Run(args);
            }
            catch (Exception ex)
            {
                stdio.ErrorFormat(ex.Message);
                //stdio.ShowError(ex.StackTrace);
                stdio.ErrorFormat("fatal error, hit any key to exit");
                stdio.ReadKey();
            }
            finally
            {
                stdio.Close();
            }
#endif
        }


    }
}

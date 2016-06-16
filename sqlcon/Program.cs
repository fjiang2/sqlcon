using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sys;

namespace sqlcon
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "sqlcon";
            Console.WriteLine("SQL Console [Version {0}]", SysExtension.ApplicationVerison);
            Console.WriteLine("Copyright (c) 2014-2016 Datconn. All rights reserved.");
            Console.WriteLine();

            Tie.Constant.MAX_CPU_REG_NUM = 600;
            string cfgFile = "user.cfg";
            cfgFile = PrepareCfgFile(cfgFile, false);

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
                        Help();
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
            var site = new CompareConsole(cfg);
            site.Run(args);
#else
            try
            {
                var site = new CompareConsole(cfg);
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

        private static string PrepareCfgFile(string cfgFile, bool overwrite)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            folder = Path.Combine(folder, "datconn", "sqlcon");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            //create empty text file if file is missing
            if (!File.Exists(cfgFile))
            {
                File.Create(cfgFile).Dispose();
            }

            string file = Path.Combine(folder, cfgFile);
            try
            {
                if (!File.Exists(file))
                {
                    File.Copy(cfgFile, file);
                }
                else 
                {
                    FileInfo f = new FileInfo(file);
                    if (f.Length == 0)
                        overwrite = true;

                    if (overwrite)
                        File.Copy(cfgFile, file, true);
                }
            }
            catch(Exception ex)
            {
                stdio.ErrorFormat("failed to initialize {0}, {1}", file, ex.Message);
            }

            return file;
        }

        public static void Help()
        {
            stdio.WriteLine("SQL Server Command Console");
            stdio.WriteLine("Usage: sqlcon");
            stdio.WriteLine("     [/cfg configuration file(.cfg)]");
            stdio.WriteLine("     [/f sql script file(.sql)]");
            stdio.WriteLine();
            stdio.WriteLine("/h,/?      : this help");
            stdio.WriteLine("/cfg       : congfiguration file default file:user.cfg]");
            stdio.WriteLine("/i         : input sql script file");
            stdio.WriteLine("/o         : result of comparsion(diff=server1-server2),sql script file");
        }
    }
}

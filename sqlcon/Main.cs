using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace sqlcon
{
    class Main
    {
        private const string _USER_CFG_TEMPLATE = "user.ini";
        private const string _USER_CFG = "user.cfg";

        private Configuration cfg;
        public Shell Shell { get; private set; }

        public Main(Configuration cfg)
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
                            RunBatch(arg, args);
                        else
                            ShowHelp();

                        return;
                }
            }


            Shell = new Shell(cfg);
            Context.DS.AddHostObject("$SHELL", Shell);
            Shell.DoConsole();
        }



        private void RunBatch(string path, params string[] args)
        {
            Batch batch = new Batch(cfg, path);
            batch.Call(args);
        }

        public static string PrepareConfigureFile(bool overwrite)
        {
            string cfgFile = _USER_CFG;
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            folder = Path.Combine(folder, "datconn", "sqlcon");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            bool exists = File.Exists(cfgFile);
            string file = Path.Combine(folder, cfgFile);
            try
            {
                if (!File.Exists(file))
                {
                    if (!exists)
                    {
                        if (File.Exists(_USER_CFG_TEMPLATE))
                        {
                            //copy user.cfg template
                            File.Copy(_USER_CFG_TEMPLATE, file);
                        }
                        else
                        {
                            //create empty text file if file is missing
                            File.Create(file).Dispose();
                        }
                    }
                    else
                        File.Copy(cfgFile, file);

                    return file;
                }

                if (exists)
                {
                    FileInfo f = new FileInfo(file);
                    if (f.Length == 0)
                        overwrite = true;

                    if (!overwrite)
                    {
                        FileInfo c = new FileInfo(cfgFile);
                        overwrite = c.LastWriteTime > f.LastWriteTime;
                    }

                    if (overwrite)
                        File.Copy(cfgFile, file, true);
                }
            }
            catch (Exception ex)
            {
                stdio.ErrorFormat("failed to initialize {0}, {1}", file, ex.Message);
            }

            return file;
        }

        public static void ShowHelp()
        {
            stdio.WriteLine("SQL Server Command Console");
            stdio.WriteLine("Usage: sqlcon");
            stdio.WriteLine("     [/cfg configuration file name (.cfg)]");
            stdio.WriteLine("     [/i sql script file name (.sql)]");
            stdio.WriteLine("     [file] sqlcon command batch file name (.sqc)");
            stdio.WriteLine();
            stdio.WriteLine("/h,/?      : this help");
            stdio.WriteLine($"/cfg       : congfiguration file default file:{_USER_CFG}]");
            stdio.WriteLine("/i         : input sql script file name");
            stdio.WriteLine("/o         : result of sql script");
            stdio.WriteLine("examples:");
            stdio.WriteLine("  sqlcon file1.sqc");
            stdio.WriteLine("  sqlcon /cfg my.cfg");
            stdio.WriteLine("  sqlcon /i script1.sql /o c:\\temp\\o.txt");
        }
    }
}

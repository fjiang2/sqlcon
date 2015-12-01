using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data;
using Sys.Data.Comparison;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Tie;
using Sys;

namespace sqlcon
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("SQL Server Command Console [Version {0}]", System.Reflection.Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine("Copyright (c) 2014-2015 Datconn. All rights reserved.");
            Console.WriteLine();

            Tie.Constant.MAX_CPU_REG_NUM = 600;
            string cfgFile = "user.cfg";

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

            try
            {
                var site = new CompareConsole(cfg);
                site.Run(args);
            }
            catch (Exception ex)
            {
                stdio.ShowError(ex.Message);
                //stdio.ShowError(ex.StackTrace);
                stdio.ShowError("fatal error, hit any key to exit");
                stdio.ReadKey();
            }
            finally
            {
                stdio.Close();
            }
        }
       
        public static void Help()
        {
            stdio.WriteLine("sqlcon v1.0");
            stdio.WriteLine("Usage: sqlcon");
            stdio.WriteLine("     [/cfg configuration file(.cfg)]");
            stdio.WriteLine("     [/s name1:name2]|[/s name]");
            stdio.WriteLine("     [/S server1:server2] [/U user1:user2] [/P password1:password2]");
            stdio.WriteLine("     [/c schema|data|row|exec|gen|shell]");
            stdio.WriteLine("     [/e table1,table2,...,table");
            stdio.WriteLine("     [/db datbase1:datbase2]|[/db datbase]");
            stdio.WriteLine("     [/dt table1:table2]|[/dt table(wildcard*,?)]");
            stdio.WriteLine("     [/f sql script file(.sql)]");
            stdio.WriteLine();
            stdio.WriteLine("/h,/?      : this help");
            stdio.WriteLine("/cfg       : congfiguration file default file:user.cfg]");
            stdio.WriteLine("/s         : server name defined on ini file]");
            stdio.WriteLine("/c schema  : compare schmea (default)");
            stdio.WriteLine("/c data    : compare data");
            stdio.WriteLine("/c gen     : generate table script from database on server1");
            stdio.WriteLine("/c row     : generate rows from table on server1");
            stdio.WriteLine("/c struct  : generate datbase schema structure in *.xml file on server1");
            stdio.WriteLine("/c exec    : run sql script on server 2, let server2 += diff");
            stdio.WriteLine("/c shell   : enter command window");
            stdio.WriteLine("/db        : database name");
            stdio.WriteLine("/dt        : table name (wildcard*,?)");
            stdio.WriteLine("/e         : excluded table list during 2 databases data comparing");
            stdio.WriteLine("/i         : input sql script file");
            stdio.WriteLine("/o         : result of comparsion(diff=server1-server2),sql script file");
            stdio.WriteLine("examples:");
            stdio.WriteLine("sqlcon /s localhost /db northwind:southwind /schema");
            stdio.WriteLine("sqlcon /S localhost /U sa /P password /db northwind /c schema");
            stdio.WriteLine("sqlcon /c data /db northwind /dt Cust*");
            stdio.WriteLine("sqlcon /c gen /dt Products");
        }
    }
}

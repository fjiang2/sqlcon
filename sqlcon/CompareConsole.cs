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

                switch (args[i++])
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
                        Program.Help();
                        return;

                }
            }

       
            new SqlShell(cfg).DoCommand();

        }
    }
}

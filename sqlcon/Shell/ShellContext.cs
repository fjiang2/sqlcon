using System;
using System.Collections.Generic;
using System.Linq;
using Sys.Data;
using Sys.Stdio;

namespace sqlcon
{
    class ShellContext
    {
        public Side theSide { get; set; }
        public Configuration cfg { get; }
        public PathManager mgr { get; }
        public Commandee commandee { get; }
        public const string THESIDE = "$TheSide";

        public ShellContext(Configuration cfg)
        {
            this.cfg = cfg;
            this.mgr = new PathManager(cfg);
            this.commandee = new Commandee(mgr);

            string server = cfg.GetValue<string>(Configuration._SERVER0);

            ConnectionProvider pvd = null;
            if (!string.IsNullOrEmpty(server))
                pvd = cfg.GetProvider(server);

            if (pvd != null)
            {
                theSide = new Side(pvd);
                ChangeSide(theSide);
            }
            else if (cfg.Providers.Count() > 0)
            {
                theSide = new Side(cfg.Providers.First());
                ChangeSide(theSide);
            }
            else
            {
                cerr.WriteLine("database server not defined");
            }
        }

        public void ChangeSide(Side side)
        {
            if (side == null)
            {
                cerr.WriteLine("undefined side");
                return;
            }

            this.theSide = side;
            Context.DS.AddHostObject(THESIDE, side);

            commandee.chdir(theSide.Provider.ServerName, theSide.DatabaseName);
        }

    }
}

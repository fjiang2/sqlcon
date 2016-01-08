using System;
using System.Collections.Generic;
using System.Linq;

namespace sqlcon
{
    class ShellContext
    {
        public Side theSide { get; set; }
        public Configuration cfg { get; }
        public PathManager mgr { get; }
        public Commandee commandee { get; }

        public ShellContext(Configuration cfg)
        {
            this.cfg = cfg;
            this.mgr = new PathManager(cfg);
            this.commandee = new Commandee(mgr);

            string server = cfg.GetValue<string>(Configuration._SERVER0);
            var pvd = cfg.GetProvider(server);
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
                throw new Exception("SQL Server not defined");
            }
        }

        protected void ChangeSide(Side side)
        {
            if (side == null)
            {
                stdio.ErrorFormat("undefined side");
                return;
            }

            this.theSide = side;
            Context.DS.AddHostObject(Context.THESIDE, side);

            commandee.chdir(theSide.Provider.ServerName, theSide.DatabaseName);
        }

    }
}

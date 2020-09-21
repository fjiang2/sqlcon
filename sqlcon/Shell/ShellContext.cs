using System;
using System.Collections.Generic;
using System.Linq;
using Sys.Data;
using Sys.Stdio;
using Sys;

namespace sqlcon
{
    class ShellContext
    {
        public Side theSide { get; set; }
        public IApplicationConfiguration cfg { get; }
        public IConnectionConfiguration connection { get; }
        public PathManager mgr { get; }
        public Commandee commandee { get; }
        public const string THESIDE = "$TheSide";

        public ShellContext(IApplicationConfiguration cfg)
        {
            this.cfg = cfg;
            this.connection = cfg.Connection;
            this.mgr = new PathManager(connection);
            this.commandee = new Commandee(mgr);

            string server = connection.Home;

            ConnectionProvider pvd = null;
            if (!string.IsNullOrEmpty(server))
                pvd = connection.GetProvider(server);

            if (pvd != null)
            {
                theSide = new Side(pvd);
                ChangeSide(theSide);
            }
            else if (connection.Providers.Count() > 0)
            {
                theSide = new Side(connection.Providers.First());
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

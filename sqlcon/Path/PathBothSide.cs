using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data;
using Sys;

namespace sqlcon
{
    class PathBothSide
    {
        public PathSide ps1
        {
            get; private set;
        }

        public PathSide ps2
        {
            get; private set;
        }

        private PathManager mgr;

        private bool valid = false;

        public PathBothSide(PathManager mgr, ApplicationCommand cmd)
        {
            this.mgr = mgr;
            ps1 = new PathSide(mgr, cmd);
            ps2 = new PathSide(mgr, cmd);

            var f1 = ps1.SetSource(cmd.arg1);
            var f2 = ps2.SetSink(cmd.arg2);
            valid = f1 && f2;
        }

        public bool Invalid => !valid;
        


        public void Run(Action<TableName, TableName> action)
        {
            if (!valid)
                return;

            var dname2 = mgr.GetPathFrom<DatabaseName>(ps2.Node);
            foreach (var tname1 in ps1.MatchedTables)
            {
                TableName tname2 = mgr.GetPathFrom<TableName>(ps2.Node);
                if (tname2 == null)
                {
                    tname2 = new TableName(dname2, tname1.SchemaName, tname1.Name);
                }

                action(tname1, tname2);

            }
        }
    }
}

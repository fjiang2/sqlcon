using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data;
using Sys;

namespace sqlcon
{
    class PathSide
    {
        public Side side
        {
            get; private set;
        }

        private DatabaseName dname;
        private TreeNode<IDataPath> node;
        private TableName[] T;   //wildcard matched tables

        private PathManager mgr;

        public PathSide(PathManager mgr)
        {
            this.mgr = mgr;
        }

        public TableName[] MatchedTables
        {
            get { return this.T; }
        }

        public TreeNode<IDataPath> Node
        {
            get { return this.node; }
        }

        public bool SetSource(string source)
        {
            return SetSource(source,  "source");
        }

        private bool SetSource(string source,  string sourceText)
        {
            if (source == null)
            {
                stdio.ErrorFormat("invalid argument");
                return false;
            }

            var path = new PathName(source);

            node = mgr.Navigate(path);
            if (node == null)
            {
                stdio.ErrorFormat("invalid path:" + path);
                return false;
            }

            dname = mgr.GetPathFrom<DatabaseName>(node);
            if (dname == null)
            {
                stdio.ErrorFormat("warning: {0} database is unavailable", sourceText);
                return false;
            }

            var server = mgr.GetPathFrom<ServerName>(node);
            side = new Side(server.Provider, dname);

            T = new TableName[] { };

            if (path.wildcard != null)
            {
                var m1 = new MatchedDatabase(dname, path.wildcard, mgr.Configuration.compareExcludedTables);
                T = m1.MatchedTableNames;
            }
            else
            {
                TableName tname = mgr.GetPathFrom<TableName>(node);
                if (tname == null)
                {
                    T = dname.GetTableNames();
                }
                else
                {
                    T = new TableName[] { tname };
                }
            }

            return true;
        }

        public bool SetSink(string sink)
        {
            if (sink != null)
            {
                return SetSource(sink,"destination");
            }
            else
                node = mgr.current;

            dname = mgr.GetPathFrom<DatabaseName>(node);
            if (dname == null)
            {
                stdio.ErrorFormat("warning: destination database is unavailable");
                return false;
            }

            T = null;
            var server = mgr.GetPathFrom<ServerName>(node);
            side = new Side(server.Provider, dname);

            return true;
        }

    }
}

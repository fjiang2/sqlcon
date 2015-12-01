using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys;
using Sys.Data;

namespace sqlcon
{
    partial class PathManager
    {
        public void Expand(TreeNode<IDataPath> pt, bool refresh)
        {
            if (pt == RootNode)
                return;

            if (ExpandServerName(pt, refresh)) return;
            if (ExpandDatabaseName(pt, refresh)) return;
            if (ExpandTableName(pt, refresh)) return;
        }

        private static bool ExpandServerName(TreeNode<IDataPath> pt, bool refresh)
        {
            if (!(pt.Item is ServerName))
                return false;

            ServerName sname = (ServerName)pt.Item;
            if (refresh || pt.Nodes.Count == 0)
            {
                if (refresh)
                    pt.Nodes.Clear();

                if (sname.Disconnected)
                    return false;

                DatabaseName[] dnames = sname.GetDatabaseNames();
                foreach (var dname in dnames)
                    pt.Nodes.Add(new TreeNode<IDataPath>(dname));
            }

            return true;
        }


        private static bool ExpandDatabaseName(TreeNode<IDataPath> pt, bool refresh)
        {
            if (!(pt.Item is DatabaseName))
                return false;

            DatabaseName dname = (DatabaseName)pt.Item;
            
            if (refresh || pt.Nodes.Count == 0)
            {
                if (refresh)
                    pt.Nodes.Clear();

                TableName[] tnames = dname.GetTableNames();
                if (tnames == null)
                    return false;

                foreach (var tname in tnames)
                    pt.Nodes.Add(new TreeNode<IDataPath>(tname));

                TableName[] vnames = dname.GetViewNames();
                foreach (var vname in vnames)
                    pt.Nodes.Add(new TreeNode<IDataPath>(vname));
            }

            return true;
        }



        private static bool ExpandTableName(TreeNode<IDataPath> pt, bool refresh)
        {
            if (!(pt.Item is TableName))
                return false;

            TableName tname = (TableName)pt.Item;
            if (refresh || pt.Nodes.Count == 0)
            {
                if (refresh)
                    pt.Nodes.Clear();

                //TableName[] tnames = tname.;
                //foreach (var tname in tnames)
                //    node.Nodes.Add(new TreeNode<IDataElementName>(tname));
            }

            return true;
        }
        
    }
}

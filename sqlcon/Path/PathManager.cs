using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data;
using System.Text.RegularExpressions;
using Sys;
using Sys.Data;

namespace sqlcon
{

    partial class PathManager
    {
        private IConnectionConfiguration cfg;
        private Tree<IDataPath> tree;


        public PathManager(IConnectionConfiguration cfg)
        {
            tree = new Tree<IDataPath>();
            current = RootNode;

            this.cfg = cfg;
            var snames = cfg.Providers.Select(pvd => pvd.ServerName).Distinct().ToList(); 

            foreach (var sname in snames)
            {
                var snode = new TreeNode<IDataPath>(sname);
                tree.Nodes.Add(snode);
            }
        }

        public bool Refreshing { get; set; }

        private static bool IsMatch(string wildcard, string text)
        {
            if (wildcard != null)
            {
                return text.IsMatch(wildcard);
            }

            return true;
        }


        public override string ToString()
        {
            List<string> items = new List<string>();
            var p = current;
            while (p != RootNode)
            {
                items.Add(p.Item.Path);
                p = p.Parent;
            }

            items.Reverse();
            return "\\" + string.Join("\\", items);
        }
    }
}

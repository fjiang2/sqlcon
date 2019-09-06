using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Sys;
using Sys.Data;

namespace sqlcon.Windows
{
    public class DbServerNodeUI : DbTreeNodeUI
    {
        private DbTreeUI tree;

        public DbServerNodeUI(DbTreeUI tree, ServerName sname)
            : base($"{sname.Path} ({sname.Provider.DataSource})", "server.png")
        {
            this.tree = tree;
            Path = sname;
            Expanded += node_Expanded;
            Selected += node_Selected;
        }

        public ServerName ServerName => (ServerName)Path;

        private void node_Expanded(object sender, RoutedEventArgs e)
        {
            ExpandNode();
        }

        public void ExpandNode()
        {
            DbTreeNodeUI theItem = this;
            ServerName sname = theItem.Path as ServerName;

            if (theItem.Items.Count > 0)
                return;

            if (sname.Disconnected)
            {
                theItem.ChangeImage("server_error.png");
                return;
            }

            foreach (DatabaseName dname in sname.GetDatabaseNames().OrderBy(d => d.Name))
            {
                DbTreeNodeUI item = new DbDatabaseNodeUI(tree, dname);
                theItem.Items.Add(item);
            }
        }

        private void node_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is DbTreeNodeUI node)
            {
                //tree.chdir(node.Path);
                tree.chdir($@"\{ServerName.Path}");
            }

            e.Handled = true;
        }

        public override bool IsMatch(string wildcard)
        {
            ServerName sname = (ServerName)Path;
            return sname.Path.IsMatch(wildcard);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Sys.Data;

namespace sqlcon.Windows
{
    class DbServerNodeUI : DbTreeNodeUI
    {
        private DbTreeUI tree;

        public DbServerNodeUI(DbTreeUI tree, ServerName sname)
            : base($"{sname.Path} ({sname.Provider.DataSource})", "server.png")
        {
            this.tree = tree;
            Path = sname;
            Expanded += serverName_Expanded;
            Selected += node_Selected;
        }

        private void serverName_Expanded(object sender, RoutedEventArgs e)
        {
            DbTreeNodeUI theItem = (DbTreeNodeUI)sender;
            ServerName sname = theItem.Path as ServerName;

            ExpandNode(theItem, sname);
        }

        public void ExpandNode(DbTreeNodeUI theItem, ServerName sname)
        {
            if (theItem.Items.Count > 0)
                return;

            if (sname.Disconnected)
            {
                theItem.ChangeImage("server_error.png");
                return;
            }

            foreach (DatabaseName dname in sname.GetDatabaseNames())
            {
                DbTreeNodeUI item = new DbDatabaseNodeUI(tree, dname);
                theItem.Items.Add(item);
            }
        }

        private void node_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is DbTreeNodeUI node)
            {
                tree.chdir(node.Path);
            }

            e.Handled = true;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Sys.Data;

namespace sqlcon.Windows
{
    public class DbDatabaseNodeUI : DbTreeNodeUI
    {
        private DbTreeUI tree;

        public DbDatabaseNodeUI(DbTreeUI tree, DatabaseName dname)
        : base(dname.Path, "database.png")
        {
            this.tree = tree;
            Path = dname;
            Expanded += node_Expanded;
            Selected += node_Selected;
        }

        public DatabaseName DatabaseName => (DatabaseName)Path;

        private void node_Expanded(object sender, RoutedEventArgs e)
        {
            ExpandNode();
        }


        public void ExpandNode()
        {
            DbTreeNodeUI theItem = this;
            DatabaseName dname = theItem.Path as DatabaseName;

            if (theItem.Items.Count > 0)
                return;

            foreach (TableName tname in dname.GetTableNames())
            {
                DbTreeNodeUI item = new DbTableNodeUI(tree, tname);
                theItem.Items.Add(item);
            }
        }

        private void node_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is DbTreeNodeUI node)
            {
                //  tree.chdir(node.Path);
                ServerName sname = DatabaseName.ServerName;
                tree.chdir($@"\{sname.Path}\{DatabaseName.Path}");
            }

            e.Handled = true;
        }
    }
}

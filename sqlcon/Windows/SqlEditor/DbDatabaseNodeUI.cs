using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Sys.Data;

namespace sqlcon.Windows
{
    class DbDatabaseNodeUI : DbTreeNodeUI
    {
        private DbTreeUI tree;

        public DbDatabaseNodeUI(DbTreeUI tree, DatabaseName dname)
        : base(dname.Path, "database.png")
        {
            this.tree = tree;
            Path = dname;
            Expanded += databaseName_Expanded;
            Selected += node_Selected;
        }

        private void databaseName_Expanded(object sender, RoutedEventArgs e)
        {
            DbTreeNodeUI theItem = (DbTreeNodeUI)sender;
            DatabaseName dname = theItem.Path as DatabaseName;
            ExpandNode(theItem, dname);
        }


        public void ExpandNode(DbTreeNodeUI theItem, DatabaseName dname)
        {
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
                tree.chdir(node.Path);
            }

            e.Handled = true;
        }
    }
}

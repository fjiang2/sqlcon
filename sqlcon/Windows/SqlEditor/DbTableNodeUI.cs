using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Sys.Data;

namespace sqlcon.Windows
{
    public class DbTableNodeUI : DbTreeNodeUI
    {
        private DbTreeUI tree;

        public DbTableNodeUI(DbTreeUI tree, TableName tname)
            : base(tname.Path, "Table_16x16.png")
        {
            this.tree = tree;
            Path = tname;
            Expanded += node_Expanded;
            Selected += node_Selected;
        }

        public TableName TableName => (TableName)Path;

        private void node_Expanded(object sender, RoutedEventArgs e)
        {
            ExpandNode();
        }

        public void ExpandNode()
        {
            DbTreeNodeUI theItem = this;
            TableName tname = theItem.Path as TableName;

            if (theItem.Items.Count > 0)
                return;

            TableSchema schema = new TableSchema(tname);
            foreach (ColumnSchema column in schema.Columns)
            {
                DbTreeNodeUI item = new DbColumnNodeUI(tree, column);
                theItem.Items.Add(item);
            }
        }


        private void node_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is DbTreeNodeUI node)
            {
                //tree.chdir(node.Path);
                DatabaseName dname = TableName.DatabaseName;
                ServerName sname = dname.ServerName;
                tree.chdir($@"\{sname.Path}\{dname.Path}\{TableName.ShortName}");
            }

            e.Handled = true;
        }


    }
}

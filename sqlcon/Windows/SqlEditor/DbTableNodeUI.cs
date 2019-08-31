using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Sys.Data;

namespace sqlcon.Windows
{
    class DbTableNodeUI : DbTreeNodeUI
    {
        private DbTreeUI tree;

        public DbTableNodeUI(DbTreeUI tree, TableName tname)
            :base(tname.Path, "Table_16x16.png")
        {
            this.tree = tree;
            Path = tname;
            Expanded += tableName_Expanded;
            Selected += node_Selected;
        }

        private void tableName_Expanded(object sender, RoutedEventArgs e)
        {
            DbTreeNodeUI theItem = (DbTreeNodeUI)sender;
            TableName tname = theItem.Path as TableName;

            ExpandNode(theItem, tname);
        }

        public void ExpandNode(DbTreeNodeUI theItem, TableName tname)
        {
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
                tree.chdir(node.Path);
            }

            e.Handled = true;
        }

      
    }
}

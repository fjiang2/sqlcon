using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using Sys.Data;
using Sys.Data.IO;
using System.ComponentModel;
using Sys;

namespace sqlcon.Windows
{
    class DbTreeUI : TreeView
    {
        PathManager mgr;
        string path;

        public event EventHandler<EventArgs<string>> PathChanged;

        public DbTreeUI()
        {
        }

        public void CreateTree(Configuration cfg)
        {
            this.mgr = new PathManager(cfg);
            createTree(cfg, this);
        }

        private void createTree(Configuration cfg, TreeView treeView)
        {
            foreach (var pvd in cfg.Providers)
            {
                ServerName sname = pvd.ServerName;
                TreeViewItem item = new TreeViewItem { Header = $"{sname.Path} ({sname.Provider.DataSource})" };
                treeView.Items.Add(item);
                item.Tag = sname;
                item.Expanded += serverName_Expanded;
                //item.MouseDoubleClick += serverName_MouseDoubleClick;
            }

            return;
        }

        //private void serverName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    string path = string.Format("\\{0}\\{1}\\", serverName.Path, databaseName.Path);
        //}

        private void chdir(string path)
        {
            PathName pathName = new PathName(path);
            var node = mgr.Navigate(pathName);
            if (node != null)
            {
                mgr.current = node;
                PathChanged?.Invoke(this, new EventArgs<string>(path));
            }
        }

        private void serverName_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem theItem = (TreeViewItem)sender;
            ServerName sname = theItem.Tag as ServerName;
            chdir($@"\{sname.Path}");

            if (theItem.Items.Count > 0)
                return;

            foreach (DatabaseName dname in sname.GetDatabaseNames())
            {
                var item = new TreeViewItem { Header = dname.Path };
                theItem.Items.Add(item);
                item.Tag = dname;
                item.Expanded += databaseName_Expanded;
            }
        }

        private void databaseName_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem theItem = (TreeViewItem)sender;
            DatabaseName dname = theItem.Tag as DatabaseName;
            ServerName sname = dname.ServerName;

            chdir($@"\{sname.Path}\{dname.Path}");

            if (theItem.Items.Count > 0)
                return;

            foreach (TableName tname in dname.GetTableNames())
            {
                var item = new TreeViewItem { Header = tname.Path };
                theItem.Items.Add(item);
                item.Tag = tname;
                item.Expanded += tableName_Expanded;
            }
        }

        private void tableName_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem theItem = (TreeViewItem)sender;
            TableName tname = theItem.Tag as TableName;
            DatabaseName dname = tname.DatabaseName;
            ServerName sname = dname.ServerName;

            chdir($@"\{sname.Path}\{dname.Path}\{tname.ShortName}");

            if (theItem.Items.Count > 0)
                return;

            TableSchema schema = new TableSchema(tname);
            foreach (ColumnSchema column in schema.Columns)
            {
                var item = new TreeViewItem { Header = ColumnSchema.GetSQLField(column) };
                theItem.Items.Add(item);
                item.Tag = column;
            }
        }
    }
}

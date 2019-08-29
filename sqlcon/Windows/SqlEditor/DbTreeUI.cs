﻿using System;
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
using Sys.Stdio;
using Sys;

namespace sqlcon.Windows
{
    class DbTreeUI : TreeView
    {
        private PathManager mgr;
        public event EventHandler<EventArgs<TreeNode<IDataPath>>> PathChanged;

        public DbTreeUI()
        {
            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            this.ContextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem
            {
                Header = "Select Top 1000 Rows",
                Command = SqlCommands.Select,
            };
            menuItem.Click += MenuItem_Click;

            ContextMenu.Items.Add(menuItem);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedItem == null)
                return;

            MenuItem menuItem = (MenuItem)sender;
            DbTreeNodeUI node = (DbTreeNodeUI)this.SelectedItem;
            MenuAction((string)menuItem.Header, node.Path);
        }

        private void MenuAction(string header, IDataPath path)
        {

        }

        public void CreateTree(Configuration cfg)
        {
            this.mgr = new PathManager(cfg);
            createTree(cfg, this);
        }

        private void chdir(IDataPath node)
        {
            switch (node)
            {
                case ServerName sname:
                    {
                        chdir($@"\{sname.Path}");
                    }
                    break;

                case DatabaseName dname:
                    {
                        ServerName sname = dname.ServerName;
                        chdir($@"\{sname.Path}\{dname.Path}");
                    }
                    break;

                case TableName tname:
                    {
                        DatabaseName dname = tname.DatabaseName;
                        ServerName sname = dname.ServerName;
                        chdir($@"\{sname.Path}\{dname.Path}\{tname.ShortName}");
                    }
                    break;
            }
        }

        private void chdir(string path)
        {
            PathName pathName = new PathName(path);
            TreeNode<IDataPath> node = mgr.Navigate(pathName);
            if (node != null)
            {
                mgr.current = node;
                PathChanged?.Invoke(this, new EventArgs<TreeNode<IDataPath>>(node));
            }
        }

        private void createTree(Configuration cfg, TreeView treeView)
        {
            foreach (var pvd in cfg.Providers)
            {
                ServerName sname = pvd.ServerName;
                DbTreeNodeUI item = new DbTreeNodeUI($"{sname.Path} ({sname.Provider.DataSource})", "server.png") { Path = sname };
                treeView.Items.Add(item);
                item.Expanded += serverName_Expanded;
                item.Selected += node_Selected;
            }

            return;
        }

        private void node_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is DbTreeNodeUI node)
            {
                chdir(node.Path);
            }

            e.Handled = true;
        }


        private void serverName_Expanded(object sender, RoutedEventArgs e)
        {
            DbTreeNodeUI theItem = (DbTreeNodeUI)sender;
            ServerName sname = theItem.Path as ServerName;

            if (theItem.Items.Count > 0)
                return;

            if (sname.Disconnected)
            {
                theItem.ChangeImage("server_error.png");
                return;
            }

            foreach (DatabaseName dname in sname.GetDatabaseNames())
            {
                DbTreeNodeUI item = new DbTreeNodeUI(dname.Path, "database.png") { Path = dname };
                theItem.Items.Add(item);
                item.Expanded += databaseName_Expanded;
                item.Selected += node_Selected;
            }
        }

        private void databaseName_Expanded(object sender, RoutedEventArgs e)
        {
            DbTreeNodeUI theItem = (DbTreeNodeUI)sender;
            DatabaseName dname = theItem.Path as DatabaseName;

            if (theItem.Items.Count > 0)
                return;

            foreach (TableName tname in dname.GetTableNames())
            {
                DbTreeNodeUI item = new DbTreeNodeUI(tname.Path, "Table_16x16.png") { Path = tname };
                theItem.Items.Add(item);
                item.Expanded += tableName_Expanded;
                item.Selected += node_Selected;
            }
        }

        private void tableName_Expanded(object sender, RoutedEventArgs e)
        {
            DbTreeNodeUI theItem = (DbTreeNodeUI)sender;
            TableName tname = theItem.Path as TableName;

            if (theItem.Items.Count > 0)
                return;

            TableSchema schema = new TableSchema(tname);
            foreach (ColumnSchema column in schema.Columns)
            {
                string image = "AlignHorizontalTop_16x16.png";
                if (column.IsPrimary || column.IsForeignKey)
                    image = "key.png";

                DbTreeNodeUI item = new DbTreeNodeUI(GetSQLField(column), image) { Path = tname };
                theItem.Items.Add(item);
            }
        }

        private static string GetSQLField(ColumnSchema column)
        {
            string ty = column.GetSQLType();
            List<string> list = new List<string>();
            if (column.IsPrimary)
                list.Add("PK");

            if (column.IsForeignKey)
                list.Add("FK");

            if (column.IsIdentity)
                list.Add("++");

            list.Add(ty);
            list.Add(column.Nullable ? "null" : "not null");

            if (column.IsComputed)
            {
                list.Add($"={column.Definition}");
            }

            string line = string.Join(", ", list);
            return $"{column.ColumnName} ({line})";
        }

        public void RunFilter(string wildcard)
        {
            Func<DbTreeNodeUI, bool> filter;

            if (string.IsNullOrEmpty(wildcard))
                filter = item => true;
            else
                filter = item => item.Path.Path.IsMatch(wildcard);

            foreach (DbTreeNodeUI item in this.Items)
                SetVisibility(item, filter);
        }

        private void SetVisibility(DbTreeNodeUI item, Func<DbTreeNodeUI, bool> filter)
        {
            if (item.Path is TableName)
            {
                bool macthed = filter(item);
                if (macthed)
                    item.Visibility = Visibility.Visible;
                else
                    item.Visibility = Visibility.Collapsed;
            }

            foreach (DbTreeNodeUI theItem in item.Items)
            {
                SetVisibility(theItem, filter);
            }
        }
    }

    public class DbTreeNodeUI : TreeViewItem
    {
        public IDataPath Path { get; set; }
        public string Text { get; }

        public DbTreeNodeUI(string text, string imageName)
        {
            this.Text = text;
            var label = WpfUtils.NewImageLabel(text, imageName);
            this.Header = label;

            Foreground = Brushes.White;
            Background = Brushes.Black;
        }

        public void ChangeImage(string imageName)
        {
            StackPanel panel = (StackPanel)this.Header;
            Image image = panel.Children[0] as Image;
            image.Source = WpfUtils.NewBitmapImage(imageName);
        }

        public override string ToString()
        {
            return Path.Path;
        }
    }
}

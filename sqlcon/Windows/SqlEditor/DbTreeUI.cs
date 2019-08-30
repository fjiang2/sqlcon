using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
                Command = SqlCommands.Select1000,
                CommandParameter = 1000,
            };
            ContextMenu.Items.Add(menuItem);

            menuItem = new MenuItem
            {
                Header = "Select All Rows",
                Command = SqlCommands.Select,
                CommandParameter = 0,
            };
            ContextMenu.Items.Add(menuItem);
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

        private TreeNode<IDataPath> chdir(string path)
        {
            PathName pathName = new PathName(path);
            TreeNode<IDataPath> node = mgr.Navigate(pathName);
            if (node != null)
            {
                mgr.current = node;
                PathChanged?.Invoke(this, new EventArgs<TreeNode<IDataPath>>(node));
            }

            return node;
        }

        public void GoHome(string home)
        {
            PathName pathName = new PathName(home);
            string[] S = pathName.FullSegments;

            if (S.Length < 2)
                return;

            foreach (DbTreeNodeUI snode in this.Items)
            {
                ServerName sname = (ServerName)snode.Path;
                if (string.Compare(sname.Path, S[1], ignoreCase: true) != 0)
                    continue;

                snode.IsExpanded = true;
                snode.IsSelected = true;
                ExpandNode(snode, sname);

                if (S.Length < 3)
                {
                    chdir(home);
                    return;
                }

                foreach (DbTreeNodeUI dnode in snode.Items)
                {
                    DatabaseName dname = (DatabaseName)dnode.Path;
                    if (string.Compare(dname.Path, S[2], ignoreCase: true) != 0)
                        continue;

                    dnode.IsExpanded = true;
                    dnode.IsSelected = true;
                    ExpandNode(dnode, dname);

                    if (S.Length < 4)
                    {
                        chdir(home);
                        return;
                    }

                    foreach (DbTreeNodeUI tnode in dnode.Items)
                    {
                        TableName tname = (TableName)tnode.Path;
                        if (string.Compare(tname.Path, S[3], ignoreCase: true) != 0)
                            continue;

                        tnode.IsExpanded = true;
                        tnode.IsSelected = true;

                        chdir(home);
                        return;
                    }
                }
            }
        }

        private void createTree(Configuration cfg, TreeView treeView)
        {
            var L = cfg.Providers.OrderBy(x => x.ServerName.Path);
            foreach (var pvd in L)
            {
                ServerName sname = pvd.ServerName;
                DbTreeNodeUI item = new DbTreeNodeUI($"{sname.Path} ({sname.Provider.DataSource})", "server.png")
                {
                    Path = sname
                };

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

            ExpandNode(theItem, sname);
        }

        private void databaseName_Expanded(object sender, RoutedEventArgs e)
        {
            DbTreeNodeUI theItem = (DbTreeNodeUI)sender;
            DatabaseName dname = theItem.Path as DatabaseName;
            ExpandNode(theItem, dname);
        }

        private void tableName_Expanded(object sender, RoutedEventArgs e)
        {
            DbTreeNodeUI theItem = (DbTreeNodeUI)sender;
            TableName tname = theItem.Path as TableName;

            ExpandNode(theItem, tname);
        }

        private void ExpandNode(DbTreeNodeUI theItem, ServerName sname)
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
                DbTreeNodeUI item = new DbTreeNodeUI(dname.Path, "database.png") { Path = dname };
                theItem.Items.Add(item);
                item.Expanded += databaseName_Expanded;
                item.Selected += node_Selected;
            }
        }


        private void ExpandNode(DbTreeNodeUI theItem, DatabaseName dname)
        {
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

        private static void ExpandNode(DbTreeNodeUI theItem, TableName tname)
        {
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
            wildcard = wildcard.Trim();
            if (wildcard == string.Empty)
            {
                ShowAllNodes();
                return;
            }

            if (wildcard.IndexOf('*') == -1 && wildcard.IndexOf('?') == -1)
                wildcard = $"*{wildcard}*";

            foreach (DbTreeNodeUI item in this.Items)
                SetVisibility(item, wildcard);
        }

        private bool SetVisibility(DbTreeNodeUI item, string wildcard)
        {
            if (item.Path is TableName)
            {
                TableName tname = (TableName)item.Path;
                bool macthed = tname.ShortName.IsMatch(wildcard);
                if (macthed)
                    item.Visibility = Visibility.Visible;
                else
                    item.Visibility = Visibility.Collapsed;

                return macthed;
            }

            bool found = false;
            foreach (DbTreeNodeUI theItem in item.Items)
            {
                if (SetVisibility(theItem, wildcard))
                {
                    if (!found)
                        found = true;
                }
            }

            if (found)
                item.Visibility = Visibility.Visible;
            else
                item.Visibility = Visibility.Collapsed;

            return found;
        }

        private void ShowAllNodes()
        {
            foreach (DbTreeNodeUI item in this.Items)
                ShowAllNodes(item);
        }

        private void ShowAllNodes(DbTreeNodeUI item)
        {
            item.Visibility = Visibility.Visible;
            foreach (DbTreeNodeUI theItem in item.Items)
            {
                ShowAllNodes(theItem);
            }
        }
    }
}

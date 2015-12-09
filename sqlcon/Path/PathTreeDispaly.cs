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

        public void Display(TreeNode<IDataPath> pt, Command cmd)
        {
            if (DisplayServerNodes(pt, cmd)) return;
            if (DisplayDatabaseNodes(pt, cmd)) return;
            if (DisplayTableNodes(pt, cmd)) return;
            if (DisplayTableSubNodes(pt, cmd)) return;
            if (_DisplayLocatorNodes(pt, cmd)) return;
            if (DisplayViewNodes(pt, cmd)) return;
        }

        private bool DisplayServerNodes(TreeNode<IDataPath> pt, Command cmd)
        {
            if (pt != RootNode)
                return false;

            int i = 0;
            int count = 0;
            int h = 0;
            CancelableWork.CanCancel(cancelled =>
            {
                foreach (var node in pt.Nodes)
                {
                    if (cancelled())
                        return CancelableState.Cancelled;

                    ServerName sname = (ServerName)node.Item;
                    ++i;

                    if (IsMatch(cmd.wildcard, sname.Path))
                    {
                        count++;
                        if (node.Nodes.Count == 0)
                        {
                            ExpandServerName(node, Refreshing);
                        }

                        stdio.WriteLine("{0,4} {1,26} <SVR> {2,10} Databases", sub(i), sname.Path, sname.Disconnected ? "?" : node.Nodes.Count.ToString());
                        h = PagePause(cmd, ++h);
                    }
                }

                stdio.WriteLine("\t{0} Server(s)", count);
                return  CancelableState.Completed;
            });

            return true;
        }

        private static bool DisplayDatabaseNodes(TreeNode<IDataPath> pt, Command cmd)
        {
            if (!(pt.Item is ServerName))
                return false;

            ServerName sname = (ServerName)pt.Item;
            if (sname.Disconnected)
            {
                stdio.WriteLine("\t? Database(s)");
            }
            else
            {
                int i = 0;
                int count = 0;
                int h = 0;
                foreach (var node in pt.Nodes)
                {
                    DatabaseName dname = (DatabaseName)node.Item;
                    ++i;

                    if (IsMatch(cmd.wildcard, dname.Path))
                    {
                        count++;
                        if (node.Nodes.Count == 0)
                            ExpandDatabaseName(node, cmd.Refresh);

                        stdio.WriteLine("{0,4} {1,26} <DB> {2,10} Tables/Views", sub(i), dname.Name, node.Nodes.Count);
                        h = PagePause(cmd, ++h);
                    }
                }

                stdio.WriteLine("\t{0} Database(s)", count);
            }

            return true;
        }


        private static bool DisplayTableNodes(TreeNode<IDataPath> pt, Command cmd)
        {
            if (!(pt.Item is DatabaseName))
                return false;

            DatabaseName dname = (DatabaseName)pt.Item;
            if (cmd.HasStorage)
            {
                displayTable(dname.StorageSchema(), "Storage");
                return true;
            }

            int i = 0;
            int[] count = new int[] { 0, 0 };
            int h = 0;
            foreach (var node in pt.Nodes)
            {
                TableName tname = (TableName)node.Item;
                ++i;

                if (IsMatch(cmd.wildcard, tname.Path))
                {
                    if (!tname.IsViewName) count[0]++;
                    if (tname.IsViewName) count[1]++;

                    stdio.WriteLine("{0,5} {1,15}.{2,-37} <{3}>", sub(i), tname.SchemaName, tname.Name, tname.IsViewName ? "VIEW" : "TABLE");

                    h = PagePause(cmd, ++h);
                }
            }

            
            stdio.WriteLine("\t{0} Table(s)", count[0]);
            stdio.WriteLine("\t{0} View(s)", count[1]);

            return true;
        }

        private static int PagePause(Command cmd, int h)
        {
            if (cmd.HasPage && h >= Console.WindowHeight - 1)
            {
                h = 0;
                stdio.Write("press any key to continue...");
                stdio.ReadKey();
                stdio.WriteLine();
            }
            return h;
        }



        private static bool DisplayTableSubNodes(TreeNode<IDataPath> pt, Command cmd)
        {
            if (!(pt.Item is TableName))
                return false;

            TableName tname = (TableName)pt.Item;

            bool flag = false;

            if (cmd.HasDefinition)
            {
                if (tname.IsViewName)
                {
                    stdio.WriteLine("cannot display view structure");
                    return false;
                }

                _DisplayColumnNodes(cmd, tname);
                flag = true;
            }

            if (cmd.HasPrimaryKey)
            {
                displayTable(tname.PrimaryKeySchema(), "Primary Key(s)");
                flag = true;
            }

            if (cmd.HasForeignKey)
            {
                displayTable(tname.ForeignKeySchema(), "Foreign Key(s)");
                flag = true;
            }

            if (cmd.HasDependency)
            {
                displayTable(tname.DependenySchema(), "Dependencies");
                flag = true;
            }

            if (cmd.HasIdentityKey)
            {
                displayTable(tname.IdentityKeySchema(), "Identity Key(s)");
                flag = true;
            }

            if (cmd.HasIndex)
            {
                displayTable(tname.IndexSchema(), "Indices");
                flag = true;
            }


            if (cmd.HasStorage)
            {
                displayTable(tname.StorageSchema(), "Storage");
                flag = true;
            }


            if (flag)
                return true;

            _DisplayLocatorNodes(pt, cmd);
            return true;
        }

        private static void displayTable(DataTable dt, string title)
        {
            if (dt.Rows.Count > 0)
            {
                stdio.WriteLine("<{0}>", title);
                dt.ToConsole();
            }
            else
            {
                stdio.WriteLine(title+ " not found");
            }
        }


        private static void _DisplayColumnNodes(Command cmd, TableName tname)
        {
            TableSchema schema = new TableSchema(tname);
            stdio.WriteLine("TABLE: {0}", tname.Path);

            int i = 0;
            int count = 0;
            int h = 0;
            foreach (IColumn column in schema.Columns)
            {
                if (IsMatch(cmd.wildcard, column.ColumnName))
                {
                    count++;

                    List<string> L = new List<string>();
                    if (column.IsIdentity) L.Add("++");
                    if (column.IsPrimary) L.Add("pk");
                    if ((column as ColumnSchema).FkContraintName != null) L.Add("fk");
                    string keys = string.Join(",", L);

                    stdio.WriteLine("{0,5} {1,26} {2,-16} {3,10} {4,10}",
                       sub(++i),
                       string.Format("[{0}]", column.ColumnName),
                       ColumnSchema.GetSQLType(column),
                       keys,
                       column.Nullable ? "null" : "not null");

                    h = PagePause(cmd, ++h);
                }
            }

            stdio.WriteLine("\t{0} Column(s)", count);
        }

        private static bool _DisplayLocatorNodes(TreeNode<IDataPath> pt, Command cmd)
        {
            int i = 0;
            int count = 0;
            foreach (var node in pt.Nodes)
            {
                IDataPath item = node.Item;
                ++i;

                if (IsMatch(cmd.wildcard, item.Path))
                {
                    count++;
                    stdio.WriteLine("{0,5} {1}", sub(i), item);
                }
            }

            stdio.WriteLine("\t{0} Item(s)", count);
            return true;
        }

        private static bool DisplayViewNodes(TreeNode<IDataPath> pt, Command cmd)
        {
            if (!(pt.Item is TableName))
                return false;

            TableName vname = (TableName)pt.Item;
            if (!vname.IsViewName)
                return false;

            DataTable schema = vname.ViewSchema();
            stdio.WriteLine("VIEW: {0}", vname.Path);

            int i = 0;
            int count = 0;
            int h = 0;
            foreach (DataRow row in schema.Rows)
            {
                string columnName = string.Format("{0}", row["COLUMN_NAME"]);
                if (IsMatch(cmd.wildcard, columnName))
                {
                    count++;

                    stdio.WriteLine("{0,5} {1,26} {2,-16} {3,10}",
                        sub(++i),
                        string.Format("{0}", columnName),
                        row["DATA_TYPE"],
                        (string)row["IS_NULLABLE"] =="YES" ? "null" : "not null");

                    h = PagePause(cmd, ++h);
                }

            }
            stdio.WriteLine("\t{0} Column(s)", count);

            return true;
        }

      
        private static string sub(int i)
        {
            return string.Format("[{0}]", i);
        }
    }
}

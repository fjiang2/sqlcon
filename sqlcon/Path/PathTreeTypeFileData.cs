using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys;
using Sys.Data;


namespace sqlcon
{
    partial class PathManager
    {
        private TableOut tout = null;

        public bool TypeFile(TreeNode<IDataPath> pt, Command cmd)
        {
            if (TypeFileData(pt, cmd)) return true;
            if (TypeLocatorData(pt, cmd)) return true;
            if (TypeLocatorColumnData(pt, cmd)) return true;

            return false;
        }

        public TableOut Tout
        {
            get { return this.tout; }
        }


        private bool TypeFileData(TreeNode<IDataPath> pt, Command cmd)
        {
            if (pt.Item is ServerName)
            {
                ServerName sname = (ServerName)pt.Item;

                //display all tables in the database server, when type of server is DbFile
                if (sname.Provider.Type == ConnectionProviderType.DbFile)
                {
                    int index = 1;
                    foreach (DatabaseName dname in sname.GetDatabaseNames())
                    {
                        cout.WriteLine();
                        cout.WriteLine($"({index++}) {dname.Name}");
                        foreach (TableName tname in dname.GetTableNames())
                        {
                            cout.WriteLine($"[{tname.ShortName}]");
                            tout = new TableOut(tname);
                            tout.Display(cmd);
                        }
                    }

                    return true;
                }
            }

            if (pt.Item is DatabaseName)
            {
                DatabaseName dname = (DatabaseName)pt.Item;
                foreach (TableName tname in dname.GetTableNames())
                {
                    cout.WriteLine();
                    cout.WriteLine($"[{tname.ShortName}]");
                    tout = new TableOut(tname);
                    tout.Display(cmd);
                }
                return true;
            }

            if (pt.Item is TableName)
            {
                TableName tname = (TableName)pt.Item;

                tout = new TableOut(tname);
                return tout.Display(cmd);
            }

            return false;
        }


        private bool TypeLocatorData(TreeNode<IDataPath> pt, Command cmd)
        {
            if (!(pt.Item is Locator))
                return false;

            TableName tname = this.GetCurrentPath<TableName>();
            Locator locator = GetCombinedLocator(pt);

            var xnode = pt;
            while (xnode.Parent.Item is Locator)
            {
                xnode = xnode.Parent;
                locator.And((Locator)xnode.Item);
            }

            tout = new TableOut(tname);
            return tout.Display(cmd, "*", locator);

        }

        private bool TypeLocatorColumnData(TreeNode<IDataPath> pt, Command cmd)
        {
            if (!(pt.Item is ColumnPath))
                return false;

            ColumnPath column = (ColumnPath)pt.Item;
            Locator locator = null;
            TableName tname = null;

            if (pt.Parent.Item is Locator)
            {
                locator = (Locator)pt.Parent.Item;
                tname = (TableName)pt.Parent.Parent.Item;
            }
            else
                tname = (TableName)pt.Parent.Item;

            tout = new TableOut(tname);
            return tout.Display(cmd, column.Columns, locator);
        }

    }
}

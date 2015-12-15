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
using Sys.Data.Comparison;
using Tie;

namespace sqlcon
{
    interface ITabCompletion
    {
        string[] TabCandidates(string argument);
    }

    class Commandee : ITabCompletion
    {
        private PathManager mgr;
        private TreeNode<IDataPath> pt;

        public Commandee(PathManager mgr)
        {
            this.mgr = mgr;
        }

        public string[] TabCandidates(string argument)
        {
            var pt = mgr.current;
            var paths = pt.Nodes
                .Where(row => row.Item.Path.ToLower().StartsWith(argument.ToLower()))
                .Select(row => row.Item.Path).ToArray();
            return paths;
        }

        private bool Navigate(Command cmd)
        {
            this.pt = mgr.current;

            if (cmd.Path1 != null)
            {
                pt = mgr.Navigate(cmd.Path1);
                if (pt == null)
                {
                    stdio.ErrorFormat("invalid path");
                    return false;
                }
            }

            return true;
        }

        public void chdir(ServerName serverName, DatabaseName databaseName)
        {
            string path = string.Format("\\{0}\\{1}\\", serverName.Path, databaseName.Path);
            PathName pathName = new PathName(path);
            var node = mgr.Navigate(pathName);
            if (node != null)
            {
                mgr.current = node;
            }
            else
                stdio.ErrorFormat("invalid path:" + path);
        }

        public bool chdir(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("command cd or chdir");
                stdio.WriteLine("cd [path]              : change directory");
                stdio.WriteLine("cd \\                  : change to root directory");
                stdio.WriteLine("cd ..                  : change to the parent directory");
                stdio.WriteLine("cd ...                 : change to the grand parent directory");
                stdio.WriteLine("cd ~                   : change to default database defined on the connection string, or change to default server");
                return true;
            }


            if (cmd.wildcard != null)
            {
                stdio.ErrorFormat("invalid path");
                return false;
            }

            if (!Navigate(cmd))
                return false;
            else
            {
                mgr.current = pt;
                return true;
            }
        }



        public void dir(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("command dir or ls");
                stdio.WriteLine("dir [path]             : display current directory");
                stdio.WriteLine("options:   /def        : display table structure");
                stdio.WriteLine("options:   /pk         : display table primary keys");
                stdio.WriteLine("options:   /fk         : display table foreign keys");
                stdio.WriteLine("options:   /ik         : display table identity keys");
                stdio.WriteLine("options:   /dep        : display table dependencies");
                stdio.WriteLine("options:   /ind        : display table index/indices");
                stdio.WriteLine("options:   /sto        : display table storage");
                stdio.WriteLine("options:   /refresh    : refresh table structure");
                return;
            }

            if (!Navigate(cmd))
                return;

            if (cmd.Refresh)
                pt.Nodes.Clear();

            if (pt.Nodes.Count == 0)
                mgr.Expand(pt, true);

            mgr.Display(pt, cmd);

        }

        public void set(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("set assignment                      : update value by current table or locator");
                stdio.WriteLine("set col1=val1, col2= val2           : update column by current table or locator");
                stdio.WriteLine("set col[n1]=val1, col[n2]=val2      : update by row-id, n1,n2 is row-id");
                stdio.WriteLine("    --use command type /r to display row-id");
                return;
            }

            if (string.IsNullOrEmpty(cmd.args))
            {
                stdio.ErrorFormat("argument cannot be empty");
                return;
            }

            var pt = mgr.current;
            if (!(pt.Item is Locator) && !(pt.Item is TableName))
            {
                stdio.ErrorFormat("table is not selected");
                return;
            }

            Locator locator = mgr.GetCombinedLocator(pt);
            TableName tname = mgr.GetCurrentPath<TableName>();

            SqlBuilder builder = new SqlBuilder().UPDATE(tname).SET(cmd.args);
            if (locator != null)
            {
                builder.WHERE(locator);
            }
            else if (mgr.Tout != null && mgr.Tout.TableName == tname && mgr.Tout.HasPhysloc)
            {
                try
                {
                    var x = ParsePhysLocStatement(mgr.Tout.Table, cmd.args);
                    if (x != null)
                        builder = x;
                }
                catch (TieException)
                {
                    stdio.ErrorFormat("invalid set assigment");
                    return;
                }
                catch (Exception ex2)
                {
                    stdio.ErrorFormat(ex2.Message);
                    return;
                }
            }

            try
            {
                int count = builder.SqlCmd.ExecuteNonQuery();
                stdio.WriteLine("{0} of row(s) affected", count);
            }
            catch (Exception ex)
            {
                stdio.ErrorFormat(ex.Message);
            }
        }

        private SqlBuilder ParsePhysLocStatement(UniqueTable table, string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            text = text.Trim();

            Memory ds = new Memory();

            Script.Evaluate(text, ds);
            if (ds.Names.Count() == 0)
                return null;

            SqlBuilder sum = null;

            foreach (VAR name in ds.Names)
            {
                string column = (string)name;
                VAL val = ds[name];

                if (!val.IsList)
                    continue;

                for (int i = 0; i < val.Size; i++)
                {
                    VAL s = val[i];
                    if (s.IsNull)
                        continue;

                    SqlBuilder builder = table.WriteValue(column, i, s.HostValue);

                    if (sum == null)
                        sum = builder;
                    else
                        sum += builder;
                }
            }

            return sum;
        }


        public void del(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("command del or erase: drop tables or delete data rows");
                stdio.WriteLine("del tablename               : drop table");
                stdio.WriteLine("del [sql where clause]      : delete current table filtered rows");
                stdio.WriteLine("example:");
                stdio.WriteLine(@"local> del Northwind\Products       : drop table Products");
                stdio.WriteLine(@"local\Northwind\Products> del       : delete all rows of table Products");
                stdio.WriteLine(@"local\Northwind\Products> del col1=1 and col2='match' : del rows matched on columns:c1 or c2");
                return;
            }

            var pt = mgr.current;
            if (!(pt.Item is Locator) && !(pt.Item is TableName))
            {
                TableName[] T = null;
                if (cmd.arg1 != null)
                {
                    PathName path = new PathName(cmd.arg1);
                    var node = mgr.Navigate(path);
                    if (node != null)
                    {
                        var dname = mgr.GetPathFrom<DatabaseName>(node);
                        if (dname != null)
                        {
                            if (cmd.wildcard != null)
                            {
                                var m = new MatchedDatabase(dname, cmd.wildcard, new string[] { });
                                T = m.MatchedTableNames;
                            }
                            else
                            {
                                var _tname = mgr.GetPathFrom<TableName>(node);
                                if (_tname != null)
                                    T = new TableName[] { _tname };
                                else
                                {
                                    stdio.ErrorFormat("invalid path");
                                    return;
                                }
                            }
                        }
                        else
                        {
                            stdio.ErrorFormat("database is unavailable");
                            return;
                        }
                    }
                    else
                    {
                        stdio.ErrorFormat("invalid path");
                        return;
                    }
                }

                if (T != null && T.Length > 0)
                {
                    if(!stdio.YesOrNo("are you sure to drop {0} tables (y/n)?", T.Length))
                        return;

                    try
                    {
                        var sqlcmd = new SqlCmd(T[0].Provider, string.Empty);
                        sqlcmd.ExecuteNonQueryTransaction(T.Select(row => string.Format("DROP TABLE {0}", row)));
                        stdio.ErrorFormat("completed to drop table(s):\n{0}", string.Join<TableName>("\n", T));
                    }
                    catch (Exception ex)
                    {
                        stdio.ErrorFormat(ex.Message);
                    }
                }
                else
                    stdio.ErrorFormat("table is not selected");

                return;
            }


            TableName tname = null;
            Locator locator = null;
            if (pt.Item is Locator)
            {
                locator = mgr.GetCombinedLocator(pt);
                tname = mgr.GetCurrentPath<TableName>();
                if (!string.IsNullOrEmpty(cmd.args))
                    locator.And(new Locator(cmd.args));
            }

            if (pt.Item is TableName)
            {
                tname = (TableName)pt.Item;
                if (!string.IsNullOrEmpty(cmd.args))
                    locator = new Locator(cmd.args);
            }

            if (locator == null)
                stdio.Write("are you sure to delete all rows (y/n)?");
            else
                stdio.Write("are you sure to delete (y/n)?");

            if (stdio.ReadKey() != ConsoleKey.Y)
                return;

            stdio.WriteLine();

            try
            {
                int count;
                if (locator == null)
                    count = new SqlBuilder().DELETE(tname).SqlCmd.ExecuteNonQuery();
                else
                    count = new SqlBuilder().DELETE(tname).WHERE(locator).SqlCmd.ExecuteNonQuery();

                stdio.WriteLine("{0} of row(s) affected", count);
            }
            catch (Exception ex)
            {
                stdio.ErrorFormat(ex.Message);
            }
        }


        public void mkdir(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("command md or mkdir");
                stdio.WriteLine("md [sql where clause]  : filter current table rows");
                stdio.WriteLine("example:");
                stdio.WriteLine("md col1=1 and col2='match' : filter rows matched on columns:c1 or c2");
                return;
            }

            TreeNode<IDataPath> pt = mgr.current;

            if (!(pt.Item is TableName) && !(pt.Item is Locator))
            {
                stdio.ErrorFormat("must add filter underneath table or locator");
                return;
            }

            if (string.IsNullOrEmpty(cmd.args))
                return;

            var xnode = mgr.TryAddWhereOrColumns(pt, cmd.args);
            //if (xnode != pt)
            //{
            //    //jump to the node just created
            //    mgr.current = xnode;
            //    mgr.Display(xnode, cmd);
            //}
        }

        public void rmdir(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("command rd or rmdir");
                stdio.WriteLine("rm [sql where clause] : remove locators");
                stdio.WriteLine("rm #1 : remove the locator node");
                return;
            }

            if (!Navigate(cmd))
                return;

            pt = pt.Parent;

            if (!(pt.Item is TableName))
            {
                stdio.ErrorFormat("cannot remove filter underneath non-Table");
                return;
            }


            var nodes = pt.Nodes.Where(node => node.Item is Locator && (node.Item as Locator).Path == cmd.Path1.name);
            if (nodes.Count() > 0)
            {
                if(!stdio.YesOrNo("are you sure to delete (y/n)?"))
                    return;

                foreach (var node in nodes)
                {
                    pt.Nodes.Remove(node);
                }

            }
            else
            {
                int result;
                if (int.TryParse(cmd.Path1.name, out result))
                {
                    result--;

                    if (result >= 0 && result < pt.Nodes.Count)
                    {
                        if(!stdio.YesOrNo("are you sure to delete (y/n)?"))
                            return;

                        var node = pt.Nodes[result];
                        pt.Nodes.Remove(node);
                    }
                }
            }
        }

        public void type(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("display current data, or search pattern");
                stdio.WriteLine("type [path]|[pattern]|[where]  : display current data, or search pattern");
                stdio.WriteLine("options:");
                stdio.WriteLine("   /top:n              : display top n records");
                stdio.WriteLine("   /all                : display all records");
                stdio.WriteLine("   /t                  : display table in vertical grid");
                stdio.WriteLine("   /r                  : display row-id");
                stdio.WriteLine("   /json               : generate json data");
                stdio.WriteLine("   /c#                 : generate C# data");
                stdio.WriteLine("   /dup                : list duplicated rows, e.g. type /dup /col:c1,c2");
                stdio.WriteLine("   /col:c1,c2,..       : display columns, or search on columns");
                stdio.WriteLine("example:");
                stdio.WriteLine("type match*s /col:c1,c2 : display rows matched on columns:c1 or c2");
                stdio.WriteLine("type id=20             : display rows where id=20");
                return;
            }

            if (!Navigate(cmd))
                return;

            if (!mgr.TypeFile(pt, cmd))
                stdio.ErrorFormat("invalid arguments");
        }


     

        public void xcopy(Command cmd, CompareSideType sideType)
        {
            if (cmd.HasHelp)
            {
                if (sideType == CompareSideType.copy)
                {
                    stdio.WriteLine("copy schema or records from table1 to table2, support table name wildcards");
                    stdio.WriteLine("copy table1 [table2] [/s]");
                }
                else if (sideType == CompareSideType.sync)
                {
                    stdio.WriteLine("synchronize schema or records from table1 to table2");
                    stdio.WriteLine("sync table1 [table2] [/s] : sync table1' records to table2");
                }
                else if (sideType == CompareSideType.compare)
                {
                    stdio.WriteLine("compare schema or records from table1 to table2");
                    stdio.WriteLine("comp table1 [table2] [/s] : sync table1' records to table2");
                }
                stdio.WriteLine("support table name wildcards");
                stdio.WriteLine("[/s]                       : table schema, default table records");
                return;
            }

            CancelableWork.CanCancel(cancelled =>
            {
                PathBothSide both = new PathBothSide(mgr, cmd);
                var dname2 = mgr.GetPathFrom<DatabaseName>(both.ps2.Node);
                if (both.ps1.MatchedTables == null)
                    return CancelableState.Completed;

                foreach (var tname1 in both.ps1.MatchedTables)
                {
                    if (cancelled())
                        return CancelableState.Cancelled;

                    TableName tname2 = mgr.GetPathFrom<TableName>(both.ps2.Node);
                    if (tname2 == null)
                    {
                        tname2 = new TableName(dname2, tname1.SchemaName, tname1.ShortName);
                    }

                    var adapter = new CompareAdapter(both.ps1.side, both.ps2.side);
                    //stdio.WriteLine("start to {0} from {1} to {2}", sideType, tname1, tname2);
                    var sql = adapter.CompareTable(cmd.IsSchema ? ActionType.CompareSchema : ActionType.CompareData,
                        sideType, tname1, tname2, mgr.Configuration.PK, cmd.Columns);

                    if (sideType == CompareSideType.compare)
                    {
                        if (sql == string.Empty)
                        {
                            stdio.WriteLine("source {0} and destination {1} are identical", tname1, tname2);
                        }
                        continue;
                    }

                    if (sql == string.Empty)
                    {
                        stdio.WriteLine("nothing changes made on destination {0}", tname2);
                    }
                    else
                    {
                        bool exists = tname2.Exists();
                        try
                        {
                            var sqlcmd = new SqlCmd(both.ps2.side.Provider, sql);
                            int count = sqlcmd.ExecuteNonQueryTransaction();
                            if (exists)
                            {
                                if (count >= 0)
                                    stdio.WriteLine("{0} row(s) changed at destination {1}", count, tname2);
                                else
                                    stdio.WriteLine("command(s) completed successfully at destination {1}", count, tname2);
                            }
                            else
                                stdio.WriteLine("table {0} created at destination", tname2);
                        }
                        catch (Exception ex)
                        {
                            stdio.ErrorFormat(ex.Message);
                            return CancelableState.Completed;
                        }
                    }
                } // loop for

                return CancelableState.Completed;
            });
        }

        public void rename(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("rename column name, table name and database name");
                stdio.WriteLine("ren table1 table2");
                return;
            }

            if (cmd.arg1 == null)
            {
                stdio.ErrorFormat("invalid argument");
                return;
            }
        }

        public void let(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("let assignment              : update key-value table row, key-value table must be defined on the sqlcon.cfg or user.cfg");
                stdio.WriteLine("let key=value               : update column by current table or locator");
                stdio.WriteLine("example:");
                stdio.WriteLine("let Smtp.Host=\"127.0.0.1\" : update key-value row, it's equivalent to UPDATE table SET [Value]='\"127.0.0.1\"' WHERE [Key]='Smtp.Host'");
                return;
            }

            if (string.IsNullOrEmpty(cmd.args))
            {
                stdio.ErrorFormat("argument cannot be empty");
                return;
            }

            var pt = mgr.current;
            if (!(pt.Item is Locator) && !(pt.Item is TableName))
            {
                stdio.ErrorFormat("table is not selected");
                return;
            }

            if (this.mgr.Configuration.dictionarytables.Count == 0)
            {
                stdio.ErrorFormat("key-value tables is undefined");
                return;
            }

            TableName tname = mgr.GetCurrentPath<TableName>();
            var setting = this.mgr.Configuration.dictionarytables.FirstOrDefault(row => row.TableName.ToUpper() == tname.Name.ToUpper());
            if (setting == null)
            {
                stdio.ErrorFormat("current table is not key-value tables");
                return;
            }

            string[] kvp = cmd.args.Split('=');

            string key = null;
            string value = null;

            if (kvp.Length == 1)
            {
                key = kvp[0].Trim();
            }
            else if (kvp.Length == 2)
            {
                key = kvp[0].Trim();
                value = kvp[1].Trim();
            }

            if (string.IsNullOrEmpty(key))
            {
                stdio.ErrorFormat("invalid assignment");
                return;
            }

            Locator locator = new Locator(setting.KeyName.ColumnName() == key);
            SqlBuilder builder = new SqlBuilder().SELECT.COLUMNS(setting.ValueName.ColumnName()).FROM(tname).WHERE(locator);
            var L = new SqlCmd(builder).FillDataColumn<string>(0);
            if (L.Count() == 0)
            {
                stdio.ErrorFormat("undefined key: {0}", key);
                return;
            }

            if (kvp.Length == 1)
            {
                stdio.ErrorFormat("{0} = {1}", key, L.First());
                return;
            }

            builder = new SqlBuilder()
                .UPDATE(tname)
                .SET(setting.ValueName.ColumnName() == value)
                .WHERE(locator);

            try
            {
                int count = builder.SqlCmd.ExecuteNonQuery();
                stdio.WriteLine("{0} of row(s) affected", count);
            }
            catch (Exception ex)
            {
                stdio.ErrorFormat(ex.Message);
            }
        }

        public void clean(Command cmd, Configuration cfg)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("clean duplicated rows");
                stdio.WriteLine("clean [path]|[pattern]|  : clean current database or table, or search pattern");
                stdio.WriteLine("options:");
                stdio.WriteLine("   /col:c1,c2,..         : clean columns, compare column c1, c2, ...");
                stdio.WriteLine("   /d                    : commit cleaning duplicated rows on database server, otherwise display # of duplicated rows");
                stdio.WriteLine("example:");
                stdio.WriteLine("clean match*s /col:c1,c2 : clean duplicated rows by comparing columns:c1 and c2");
                stdio.WriteLine("clean                    : clean by comparing entire row");
                return;
            }

            if (!Navigate(cmd))
                return;


            if (pt.Item is TableName)
            {
                var tname = (TableName)pt.Item;
                var dup = new DuplicatedTable(tname, cmd.Columns);
                if (cmd.Has("d"))
                {
                    int count = dup.Clean();
                    stdio.WriteLine("completed to clean {0} #rows at {1}", count, tname);
                }
                else
                {
                    int count = dup.DuplicatedRowCount();
                    if (count == 0)
                        stdio.WriteLine("no duplicated rows at {0}", tname);
                    else
                        stdio.WriteLine("{0} duplicated row(s) at {1}", count, tname);
                }
                return;
            }


            if (pt.Item is DatabaseName)
            {
                var dname = (DatabaseName)pt.Item;
                var m = new MatchedDatabase(dname, cmd.wildcard, cfg.compareExcludedTables);
                var T = m.MatchedTableNames;

                CancelableWork.CanCancel(cancelled =>
                {
                    foreach (var tn in T)
                    {
                        if (cancelled())
                            return CancelableState.Cancelled;

                        if (cmd.Has("d"))
                        {
                            stdio.WriteLine("start to clean {0}", tn);
                            var dup = new DuplicatedTable(tn, cmd.Columns);
                            int count = dup.Clean();
                            stdio.WriteLine("cleaned {0} #rows", count);
                        }
                        else
                        {
                            stdio.WriteLine("start to query {0}", tn);
                            var dup = new DuplicatedTable(tn, cmd.Columns);
                            int count = dup.DuplicatedRowCount();
                            if (count == 0)
                                stdio.WriteLine("distinct rows");
                            else
                                stdio.WriteLine("{0} duplicated row(s)", count, tn);
                        }
                  
                    }

                    return CancelableState.Completed;
                });

                return;
            }

            stdio.ErrorFormat("select database or table first");
        }


        public void export(Command cmd, Configuration cfg, ShellContext context)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("export data, schema, class, and template");
                stdio.WriteLine("export /insert  : export INSERT INTO script on current table/database");
                stdio.WriteLine("  [/if]        : option /if generate if exists row then UPDATE else INSERT");
                stdio.WriteLine("export /create  : generate CREATE TABLE script on current table/database");
                stdio.WriteLine("export /select  : generate SELECT FROM WHERE template");
                stdio.WriteLine("export /update  : generate UPDATE SET WHERE template");
                stdio.WriteLine("export /delete  : generate DELETE FROM WHERE template");
                stdio.WriteLine("export /schema  : generate database schema xml file");
                stdio.WriteLine("export /data    : generate database data xml file");
                stdio.WriteLine("export /class   : generate C# table class");
                return;
            }

            if (!Navigate(cmd))
                return;

            if (pt.Item is TableName || pt.Item is DatabaseName)
            {
                var exporter = new Exporter(mgr, pt, cfg);

                if (cmd.Has("insert"))
                    exporter.ExportInsert(cmd);
                else if (cmd.Has("create"))
                    exporter.ExportCreate();
                else if (cmd.Has("select"))
                    exporter.ExportScud(SqlScriptType.SELECT);
                else if (cmd.Has("delete"))
                    exporter.ExportScud(SqlScriptType.DELETE);
                else if (cmd.Has("update"))
                    exporter.ExportScud(SqlScriptType.UPDATE);
                else if (cmd.Has("schema"))
                    exporter.ExportSchema();
                else if (cmd.Has("data"))
                    exporter.ExportData();
                else if (cmd.Has("class"))
                    exporter.ExportClass();
                else
                    stdio.ErrorFormat("invalid command");
            }
            else
                stdio.ErrorFormat("select database or table first");
        }
     
    }
}

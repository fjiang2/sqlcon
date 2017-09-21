using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.IO;
using Sys;
using Sys.Data;
using Sys.Data.Comparison;
using Sys.Data.IO;
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
        public CommandState ErrorCode { get; private set; } = CommandState.OK;

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

        private bool Navigate(PathName path)
        {
            this.pt = mgr.current;

            if (path != null)
            {
                pt = mgr.Navigate(path);
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
                stdio.WriteLine("Change current database directory");
                stdio.WriteLine("command cd or chdir");
                stdio.WriteLine("cd [path]              : change database directory");
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

            if (!Navigate(cmd.Path1))
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
                stdio.WriteLine("dir [path]     : display current directory");
                stdio.WriteLine("options: ");
                stdio.WriteLine("   /def        : display table structure");
                stdio.WriteLine("   /pk         : display table primary keys");
                stdio.WriteLine("   /fk         : display table foreign keys");
                stdio.WriteLine("   /ik         : display table identity keys");
                stdio.WriteLine("   /dep        : display table dependencies");
                stdio.WriteLine("   /ind        : display table index/indices");
                stdio.WriteLine("   /sto        : display table storage");
                stdio.WriteLine("   /refresh    : refresh table structure");
                return;
            }

            if (!Navigate(cmd.Path1))
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
                stdio.WriteLine(@"local> del Northwind\Products       : drop table [Products]");
                stdio.WriteLine(@"local\Northwind\Products> del       : delete all rows of table [Products]");
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
                                var m = new MatchedDatabase(dname, cmd.wildcard, null);
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
                    if (!stdio.YesOrNo("are you sure to drop {0} tables (y/n)?", T.Length))
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

            if (!Navigate(cmd.Path1))
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
                if (!stdio.YesOrNo("are you sure to delete (y/n)?"))
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
                        if (!stdio.YesOrNo("are you sure to delete (y/n)?"))
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
                stdio.WriteLine("   /dup                : list duplicated rows, e.g. type /dup /col:c1,c2");
                stdio.WriteLine("   /col:c1,c2,..       : display columns, or search on columns");
                stdio.WriteLine("   /edit               : edit mode");
                stdio.WriteLine("example:");
                stdio.WriteLine("type match*s /col:c1,c2 : display rows matched on columns:c1 or c2");
                stdio.WriteLine("type id=20             : display rows where id=20");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            if (!mgr.TypeFile(pt, cmd))
                stdio.ErrorFormat("invalid arguments");
        }




        public void copy(Command cmd, CompareSideType sideType)
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

            CancelableWork.CanCancel(cts =>
            {
                PathBothSide both = new PathBothSide(mgr, cmd);
                var dname2 = mgr.GetPathFrom<DatabaseName>(both.ps2.Node);
                if (both.ps1.MatchedTables == null)
                    return;

                foreach (var tname1 in both.ps1.MatchedTables)
                {
                    if (cts.IsCancellationRequested)
                        return;

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
                            return;
                        }
                    }
                } // loop for

                return;
            });
        }

        public void rename(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("rename column name, table name, modify column");
                stdio.WriteLine();
                stdio.WriteLine("ren [database] newdatasbase   : rename database or current database to newdatabase");
                stdio.WriteLine("ren [table] newtable          : rename table or current table to newtable");
                stdio.WriteLine("ren column newcolumn          : rename column on current table to newcolumn");
                stdio.WriteLine();
                return;
            }

            if (cmd.arg1 == null)
            {
                stdio.ErrorFormat("invalid argument");
                return;
            }
        }

        public void attrib(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("command attrib: update column property");
                stdio.WriteLine("add primary key, foreign key or identity key");
                stdio.WriteLine("columns:");
                stdio.WriteLine("  attrib [table] +c:col1=varchar(2)+null : add column or alter column");
                stdio.WriteLine("  attrib [table] +c:col1=varchar(10)     : add column or alter column");
                stdio.WriteLine("  attrib [table] -c:col1                 : remove column");
                stdio.WriteLine("primary keys:");
                stdio.WriteLine("  attrib [table] +p:col1,col2            : add primary key");
                stdio.WriteLine("  attrib [table] +p:col1,col2            : remove primary key");
                stdio.WriteLine("foreign keys:");
                stdio.WriteLine("  attrib [table] +f:col1=table2[.col2]   : add foreign key");
                stdio.WriteLine("  attrib [table] -f:col1                 : remove foreign key");
                stdio.WriteLine("identiy key:");
                stdio.WriteLine("  attrib [table] +i:col1                 : add identity");
                stdio.WriteLine("  attrib [table] -i:col1                 : remove identity");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;


            if (!(pt.Item is TableName))
            {
                stdio.ErrorFormat("table is not selected");
                return;
            }


            if (cmd.options.Has("+c"))
            {
                TableName tname = (TableName)pt.Item;
                string expr = cmd.options.GetValue("+c");
                string[] items = expr.Split(new string[] { "=", "+" }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length != 2 && items.Length != 3)
                {
                    stdio.ErrorFormat("invalid expression:{0}, correct is col1=type or col1=type+null", expr);
                    return;
                }
                string column = items[0];
                string type = items[1];
                string nullable = "NOT NULL";
                if (items.Length == 3 && items[2] == "null")
                    nullable = "NULL";

                string SQL;
                var schema = new TableSchema(tname);

                if (schema.Columns.Where(c => c.ColumnName.ToLower() == column.ToLower()).Count() != 0)
                    SQL = $"ALTER TABLE [{tname.Name}] ALTER COLUMN {column} {type} {nullable}";
                else
                    SQL = $"ALTER TABLE [{tname.Name}] ADD {column} {type} {nullable}";

                ExecuteNonQuery(tname.Provider, SQL);
                return;
            }

            if (cmd.options.Has("-c"))
            {
                TableName tname = (TableName)pt.Item;
                string column = cmd.options.GetValue("-c");
                string SQL = $"ALTER TABLE [{tname.Name}] DROP COLUMN {column}";
                ExecuteNonQuery(tname.Provider, SQL);
                return;
            }

            if (cmd.options.Has("+f"))
            {
                TableName fkName = (TableName)pt.Item;
                string expr = cmd.options.GetValue("+f");
                string[] items = expr.Split('=');

                if (items.Length != 2)
                {
                    stdio.ErrorFormat("invalid foreign key expression:{0}, correct is col1=pktable.col2", expr);
                    return;
                }

                string fkColumn = items[0];
                string pkName = items[1];
                string pkColumn = fkColumn;

                items = items[1].Split('.');
                if (items.Length == 2)
                {
                    pkName = items[0];
                    if (items[1] != string.Empty)
                        pkColumn = items[1];
                }
                else if (items.Length == 1)
                {
                    pkName = items[0];
                    pkColumn = fkColumn;
                }
                else
                {
                    stdio.ErrorFormat("invalid foreign key expression:{0}, correct is col1=pktable.col2", expr);
                    return;
                }


                //generate unique constraint name
                string constraintName = $"FK_{fkName.Name}_{pkName}";
                try
                {
                    string[] exists = fkName
                        .ForeignKeySchema()
                        .AsEnumerable()
                        .Select(row => row.Field<string>("Constraint_Name"))
                        .ToArray();

                    int i = 1;
                    while (exists.Contains(constraintName) && i < 1000)
                    {
                        constraintName += i++;
                    }
                }
                catch (Exception ex)
                {
                    stdio.ErrorFormat($"fails in generating foreign key constraint name, {ex.Message}");
                    return;
                }

                //check fkColumn, pkColumn is valid
                string SQL = $"ALTER TABLE [{fkName.Name}] ADD CONSTRAINT [{constraintName}] FOREIGN KEY([{fkColumn}]) REFERENCES [{pkName}]([{pkColumn}])";
                ExecuteNonQuery(fkName.Provider, SQL);
                return;
            }

            if (cmd.options.Has("+p"))
            {
                TableName tname = (TableName)pt.Item;
                string expr = cmd.options.GetValue("+p");
                string SQL = $"ALTER TABLE [{tname.Name}] ADD PRIMARY KEY(expr)";
                ExecuteNonQuery(tname.Provider, SQL);
                return;
            }

            if (cmd.options.Has("+i"))
            {
                TableName tname = (TableName)pt.Item;
                string column = cmd.options.GetValue("+i");
                string SQL = @"
ALTER TABLE {0} ADD {1} INT IDENTITY(1, 1)
ALTER TABLE {0} DROP COLUMN {2}
sp_rename '{1}', '{2}', 'COLUMN'";
                string.Format(SQL, tname.Name, $"_{column}_", column);
                ExecuteNonQuery(tname.Provider, SQL);
                return;
            }

        }

        private static int ExecuteNonQuery(ConnectionProvider provider, string sql)
        {
            try
            {
                return new SqlCmd(provider, sql).ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                stdio.ErrorFormat("{0}", ex.Message);
            }

            return -1;
        }

        public void let(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("let assignment              : variable assign statement ");
                stdio.WriteLine("let key=value               : update column by current table or locator");
                stdio.WriteLine("example:");
                stdio.WriteLine("let Host=\"127.0.0.1\"      : value of variable Host is '\"127.0.0.1\"'");
                stdio.WriteLine("let a=12                    : value of variable Host is '\"127.0.0.1\"'");
                return;
            }

            if (string.IsNullOrEmpty(cmd.args))
            {
                stdio.ErrorFormat("assignment cannot be empty");
                return;
            }

            try
            {
                Script.Execute($"{cmd.args};", Context.DS);
            }
            catch (Exception ex)
            {
                stdio.ErrorFormat("execute error: {0}", ex.Message);
            }
        }

        public void let1(Command cmd)
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

            if (!Navigate(cmd.Path1))
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
                var m = new MatchedDatabase(dname, cmd.wildcard, cfg.compareIncludedTables);
                var T = m.MatchedTableNames;

                CancelableWork.CanCancel(cts =>
                {
                    foreach (var tn in T)
                    {
                        if (cts.IsCancellationRequested)
                            return;

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


                });

                return;
            }

            stdio.ErrorFormat("select database or table first");
        }

        public void import(Command cmd, Configuration cfg, ShellContext context)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("import file");
                stdio.WriteLine("option:");
                stdio.WriteLine("   /fmt:xml,ds  : load System.Data.DataSet xml file");
                stdio.WriteLine("   /fmt:xml,dt  : load System.Data.DataTable xml file");
                stdio.WriteLine("   /fmt:txt     : load text/csv file");
            }

            string file = cmd.arg1;
            if (file == null)
            {
                stdio.ErrorFormat("file name not specified");
                return;
            }

            if (!File.Exists(file))
            {
                stdio.ErrorFormat($"file \"{file}\" not exist");
                return;
            }

            string fmt = cmd.GetValue("fmt") ?? Path.GetExtension(file).ToLower();
            switch (fmt)
            {
                case ".xml":
                case "xml":
                case "xml,ds":
                    var ds = new DataSet();
                    try
                    {
                        ds.ReadXml(file, XmlReadMode.ReadSchema); ;
                        ShellHistory.SetLastResult(ds);
                        stdio.WriteLine($"{typeof(DataSet).FullName} xml file \"{file}\" has been loaded");
                    }
                    catch (Exception ex)
                    {
                        stdio.ErrorFormat($"invalid {typeof(DataSet).FullName} xml file, {ex.Message}");
                        return;
                    }
                    break;

                case "xml,dt":
                    var dt = new DataTable();
                    try
                    {
                        dt.ReadXml(file); ;
                        ShellHistory.SetLastResult(dt);
                    }
                    catch (Exception ex)
                    {
                        stdio.ErrorFormat($"invalid {typeof(DataTable).FullName} xml file, {ex.Message}");
                        return;
                    }
                    stdio.WriteLine($"{typeof(DataTable).FullName} xml file \"{file}\" has been loaded");
                    break;

                case "txt":
                    break;

                default:
                    stdio.ErrorFormat("invalid command");
                    break;
            }
        }

        public void export(Command cmd, Configuration cfg, ShellContext context)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("export data, schema, class, and template on current selected server/db/table");
                stdio.WriteLine("option:");
                stdio.WriteLine("   /insert  : export INSERT INTO script on current table/database");
                stdio.WriteLine("   [/if]    : option /if generate if exists row then UPDATE else INSERT");
                stdio.WriteLine("   /create  : generate CREATE TABLE script on current table/database");
                stdio.WriteLine("   /select  : generate SELECT FROM WHERE template");
                stdio.WriteLine("   /update  : generate UPDATE SET WHERE template");
                stdio.WriteLine("   /save    : generate IF EXISTS UPDATE ELSE INSERT template");
                stdio.WriteLine("   /delete  : generate DELETE FROM WHERE template, delete rows with foreign keys constraints");
                stdio.WriteLine("   /schema  : generate database schema xml file");
                stdio.WriteLine("   /data    : generate database/table data xml file");
                stdio.WriteLine("   /dpo     : generate C# table class");
                stdio.WriteLine("   /l2s     : generate C# Linq to SQL class");
                stdio.WriteLine("   /dc1     : generate C# data contract class and extension class from last result");
                stdio.WriteLine("   /dc2     : generate C# data contract class from last result");
                stdio.WriteLine("      [/method:foo] default convert method is defined on the .cfg");
                stdio.WriteLine("      [/col:pk1,pk2] default primary key is the first column");
                stdio.WriteLine("   /entity  : generate C# method copy/compare/clone for Entity framework");
                stdio.WriteLine("      [/base:type] define base class or interface, use ~ to represent generic class itself, delimited by ;");
                stdio.WriteLine("   /csv     : generate table csv file");
                stdio.WriteLine("   /json    : generate json from last result");
                stdio.WriteLine("   /c#      : generate C# data from last result");
                stdio.WriteLine("      [/type:dict|list|enum] data type, default is list");
                stdio.WriteLine("   /conf    : generate Config class from last result or .cfg");
                stdio.WriteLine("      [/type:k|d|f|p|F|P] class type, default is kdP");
                stdio.WriteLine("          k : generate const key");
                stdio.WriteLine("          d : generate default value");
                stdio.WriteLine("          P : generate static property to get setting");
                stdio.WriteLine("          F : generate static field to get setting");
                stdio.WriteLine("          p : generate hierarchial property to get setting");
                stdio.WriteLine("          f : generate hierarchial field to get setting");
                stdio.WriteLine("      [/in:path] input path(.cfg)");
                stdio.WriteLine("      [/get:name] GetValue method name");
                stdio.WriteLine("      [/kc:name] class name of const key");
                stdio.WriteLine("      [/dc:name] class name of default value");
                stdio.WriteLine("code generation options");
                stdio.WriteLine("      [/ns:name] default name space is defined on the .cfg");
                stdio.WriteLine("      [/class:name] default class name is defined on the .cfg");
                stdio.WriteLine("      [/using:assembly] allow the use of types in a namespace, delimited by ;");
                stdio.WriteLine("      [/out:path] output directory or file name (.cs)");

                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            if (pt.Item is TableName || pt.Item is Locator || pt.Item is DatabaseName || pt.Item is ServerName)
            {
                var exporter = new Exporter(mgr, pt, cfg);

                if (cmd.Has("insert"))
                    exporter.ExportInsert(cmd);
                else if (cmd.Has("create"))
                    exporter.ExportCreate(cmd);
                else if (cmd.Has("select"))
                    exporter.ExportScud(SqlScriptType.SELECT);
                else if (cmd.Has("delete"))
                    exporter.ExportScud(SqlScriptType.DELETE);
                else if (cmd.Has("update"))
                    exporter.ExportScud(SqlScriptType.UPDATE);
                else if (cmd.Has("save"))
                    exporter.ExportScud(SqlScriptType.INSERT_OR_UPDATE);
                else if (cmd.Has("schema"))
                    exporter.ExportSchema();
                else if (cmd.Has("data"))
                    exporter.ExportData(cmd);
                else if (cmd.Has("dpo"))
                    exporter.ExportClass(cmd);
                else if (cmd.Has("csv"))
                    exporter.ExportCsvFile(cmd);
                else if (cmd.Has("dc1"))
                    exporter.ExportDataContract(cmd, 1);
                else if (cmd.Has("dc2"))
                    exporter.ExportDataContract(cmd, 2);
                else if (cmd.Has("entity"))
                    exporter.ExportEntityClass(cmd);
                else if (cmd.Has("l2s"))
                    exporter.ExportLinq2SQLClass(cmd);
                else if (cmd.ToJson)
                {
                    DataTable dt = ShellHistory.LastTable();
                    if (dt != null)
                    {
                        stdio.WriteLine(TableOut.ToJson(dt));
                    }
                    else
                    {
                        stdio.ErrorFormat("display data table first by sql clause or command [type]");
                    }
                }
                else if (cmd.ToCSharp)
                    exporter.ExportCSharpData(cmd);
                else if (cmd.Has("conf"))
                    exporter.ExportConf(cmd);
                else
                    stdio.ErrorFormat("invalid command options");
            }
            else
                stdio.ErrorFormat("select server, database or table first");
        }

        public void mount(Command cmd, Configuration cfg)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("mount database server");
                stdio.WriteLine("mount alias=server_name   : alias must start with letter");
                stdio.WriteLine("options:");
                stdio.WriteLine("   /db:database           : initial catalog, default is 'master'");
                stdio.WriteLine("   /u:username            : user id, default is 'sa'");
                stdio.WriteLine("   /p:password            : password, default is empty, use Windows Security when /u /p not setup");
                stdio.WriteLine("   /pvd:provider          : sqloledb, xmlfile, default is SQL client");
                stdio.WriteLine("example:");
                stdio.WriteLine("  mount ip100=192.168.0.100\\sqlexpress /u:sa /p:p@ss");
                stdio.WriteLine("  mount web=http://192.168.0.100/db/northwind.xml /u:sa /p:p@ss");
                stdio.WriteLine("  mount xml=file://c:\\db\\northwind.xml");
                return;
            }

            if (cmd.arg1 == null)
            {
                stdio.ErrorFormat("invalid arguments");
                return;
            }

            var items = cmd.arg1.Split('=');
            if (items.Length != 2)
            {
                stdio.ErrorFormat("invalid arguments, correct format is alias=server_name");
                return;
            }
            string serverName = items[0].Trim();
            string dataSource = items[1].Trim();

            StringBuilder builder = new StringBuilder();
            string pvd = cmd.GetValue("pvd");
            if (pvd != null)
            {
                if (pvd != "sqloledb" && pvd != "xmlfile")
                {
                    stdio.ErrorFormat("provider={0} is not supported", pvd);
                    return;
                }
                builder.AppendFormat("provider={0};", pvd);
            }
            else
            {
                if (dataSource.StartsWith("file://") || dataSource.StartsWith("http://") || dataSource.StartsWith("ftp://"))
                    builder.Append("provider=xmlfile;");
            }


            builder.AppendFormat("data source={0};", dataSource);

            string db = cmd.GetValue("db");
            if (db != null)
                builder.AppendFormat("initial catalog={0};", db);
            else
                builder.Append("initial catalog=master;");

            string userId = cmd.GetValue("u");
            string password = cmd.GetValue("p");

            if (userId == null && password == null)
            {
                builder.Append("integrated security=SSPI;packet size=4096");
            }
            else
            {
                if (userId != null)
                    builder.AppendFormat("User Id={0};", userId);
                else
                    builder.Append("User Id=sa;");

                if (password != null)
                    builder.AppendFormat("Password={0}", password);
                else
                    builder.Append("Password=");
            }

            string connectionString = builder.ToString();

            ConnectionProvider provider = ConnectionProviderManager.Register(serverName, connectionString);
            if (!provider.CheckConnection())
            {
                stdio.ErrorFormat("database is offline or wrong parameter");
                return;
            }
            var snode = new TreeNode<IDataPath>(provider.ServerName);

            var result = cfg.Providers.FirstOrDefault(row => row.ServerName.Path == serverName);
            if (result != null)
            {
                cfg.Providers.Remove(result);

                var node = mgr.RootNode.Nodes.FirstOrDefault(row => row.Item.Path == serverName);
                if (node != null)
                    mgr.RootNode.Nodes.Remove(node);
            }


            cfg.Providers.Add(provider);
            mgr.RootNode.Nodes.Add(snode);

            var xnode = mgr.Navigate(new PathName("\\" + serverName));
            if (xnode != null)
            {
                mgr.current = xnode;
            }
        }

        public void umount(Command cmd, Configuration cfg)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("unmount database server");
                stdio.WriteLine("unmount alias             : alias must start with letter");
                stdio.WriteLine("example:");
                stdio.WriteLine("  umount ip100");
                return;
            }

            if (cmd.arg1 == null)
            {
                stdio.ErrorFormat("invalid arguments");
                return;
            }

            var items = cmd.arg1.Split('=');
            string serverName = cmd.arg1;

            var result = cfg.Providers.FirstOrDefault(row => row.ServerName.Path == serverName);
            if (result != null)
            {
                cfg.Providers.Remove(result);

                var node = mgr.RootNode.Nodes.FirstOrDefault(row => row.Item.Path == serverName);
                if (node != null)
                    mgr.RootNode.Nodes.Remove(node);
            }

            var sname = mgr.GetCurrentPath<ServerName>();
            if (sname != null && sname.Path == serverName)
            {
                var xnode = mgr.Navigate(new PathName("\\"));
                if (xnode != null)
                {
                    mgr.current = xnode;
                }
            }
        }





        public void OpenEditor()
        {
            DataTable dt = ShellHistory.LastTable();

            if (dt == null)
            {
                stdio.ErrorFormat("select table first");
                return;
            }

            var editor = new Windows.TableEditor(mgr.Configuration, new UniqueTable(null, dt));

            editor.ShowDialog();
        }

        private static void OpenDirectory(string path, string message)
        {
            if (System.IO.Directory.Exists(path))
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "Explorer";
                process.StartInfo.Arguments = path;
                process.Start();
            }
            else
                stdio.ErrorFormat("{0} path not exist: {1}", message, path);
        }

        public void xcopy(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("xcopy large size records, support table/database name wildcards");
                stdio.WriteLine("   table must have same structure");
                stdio.WriteLine("xcopy database1 [database2]");
                stdio.WriteLine("xcopy table1 [table2]");
                stdio.WriteLine("       /col:c1[=d1],c2[=d2],...         copy selected columns (mapping)");
                stdio.WriteLine("       /s                               compare table schema");
                stdio.WriteLine("note that: to xcopy selected records of table, mkdir locator first, example:");
                stdio.WriteLine(@"  \local\NorthWind\Products> md ProductId<2000");
                stdio.WriteLine(@"  \local\NorthWind\Products> xcopy 1 \local\db");
                return;
            }

            CancelableWork.CanCancel(cts =>
            {
                PathBothSide both = new PathBothSide(mgr, cmd);
                var dname2 = mgr.GetPathFrom<DatabaseName>(both.ps2.Node);
                if (both.ps1.MatchedTables == null)
                    return;

                foreach (var tname1 in both.ps1.MatchedTables)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    TableName tname2 = mgr.GetPathFrom<TableName>(both.ps2.Node);
                    if (tname2 == null)
                        tname2 = new TableName(dname2, tname1.SchemaName, tname1.ShortName);

                    if (cmd.IsSchema)
                    {
                        string result = Compare.TableSchemaDifference(CompareSideType.compare, tname1, tname2);
                        if (!string.IsNullOrEmpty(result))
                        {
                            stdio.ErrorFormat("destination table is not compatible or doesn't exist");
                            continue;
                        }
                    }

                    List<SqlBulkCopyColumnMapping> maps = new List<SqlBulkCopyColumnMapping>();
                    if (cmd.Columns.Length > 0)
                    {
                        SqlBulkCopyColumnMapping mapping;
                        foreach (var column in cmd.Columns)
                        {
                            string[] items = column.Split('=');
                            if (items.Length == 2)
                                mapping = new SqlBulkCopyColumnMapping(items[0], items[1]);
                            else
                                mapping = new SqlBulkCopyColumnMapping(column, column);

                            maps.Add(mapping);
                        }
                    }

                    TableReader tableReader;
                    if (both.ps1.Node.Item is Locator)
                    {
                        Locator locator = mgr.GetCombinedLocator(both.ps1.Node);
                        string where = locator.Path;
                        tableReader = new TableReader(tname1, where.Inject());
                    }
                    else
                        tableReader = new TableReader(tname1);

                    long _cnt = tableReader.Count;
                    int count = 0;
                    if (_cnt < int.MaxValue)
                        count = (int)_cnt;
                    else
                    {
                        count = int.MaxValue;
                        stdio.Error($"total count={_cnt}, too many rows, progress bar may not be accurate");
                    }
                    stdio.Write("copying {0} ", tname1.Name);
                    using (var progress = new ProgressBar { Count = count })
                    {
                        TableBulkCopy bulkCopy = new TableBulkCopy(tableReader);
                        try
                        {
                            bulkCopy.CopyTo(tname2, maps.ToArray(), cts, progress);
                        }
                        catch (Exception ex)
                        {
                            stdio.Error(ex.Message);
                        }

                        if (cts.IsCancellationRequested)
                            progress.Report(count);
                    }

                    if (!cts.IsCancellationRequested)
                        stdio.WriteLine(", Done.");
                }
            });
        }

        public void execute(Command cmd, Side theSide)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("execute sql script file");
                stdio.WriteLine("execute file (.sql)");
                stdio.WriteLine("examples:");
                stdio.WriteLine("  execute northwind.sql       : execute single sql script file");
                return;
            }

            string inputfile;
            if (cmd.arg1 != null)
                inputfile = cmd.Configuration.WorkingDirectory.GetFullPath(cmd.arg1, ".sql");
            else
            {
                stdio.ErrorFormat("input undefined");
                return;
            }

            if (theSide.ExecuteScript(inputfile))
                ErrorCode = CommandState.OK;
            else
                ErrorCode = CommandState.SQL_FAILS;
        }

        public void edit(Command cmd, Side theSide)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("edit, view and execute sql script");
                stdio.WriteLine("edit                          : create new file and edit");
                stdio.WriteLine("edit [file]                   : edit file, it is read-only if file is hyperlink");
                stdio.WriteLine("options:");
                stdio.WriteLine("   /usr                       : FTP user name");
                stdio.WriteLine("   /pwd                       : FTP password");
                stdio.WriteLine("examples:");
                stdio.WriteLine("  edit c:\\db\\northwind.sql");
                stdio.WriteLine("  edit file://datconn/northwind.sql");
                stdio.WriteLine("  edit http://www.datconn.com/demos/northwind.sql");
                stdio.WriteLine("  edit ftp://www.datconn.com/demos/northwind.sql /usr:user /pwd:password");
                return;
            }

            FileLink fileLink = null;
            if (cmd.arg1 != null)
            {
                string inputfile = cmd.arg1;

                if (inputfile.IndexOf("://") < 0)
                {
                    if (Path.GetDirectoryName(inputfile) == string.Empty)
                    {
                        string path = cmd.Configuration.GetValue<string>("MyDocuments", Directory.GetCurrentDirectory());
                        inputfile = $"{path}\\{inputfile}";
                    }
                }

                fileLink = FileLink.CreateLink(inputfile, cmd.GetValue("usr"), cmd.GetValue("pwd"));

                try
                {
                    if (!fileLink.Exists)
                    {
                        if (!fileLink.IsLocalLink)
                        {
                            stdio.ErrorFormat("file {0} doesn't exist", fileLink);
                            return;
                        }
                        else
                        {
                            File.WriteAllText(inputfile, string.Empty);
                            fileLink = FileLink.CreateLink(inputfile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    stdio.Error(ex.Message);
                    return;
                }

            }

            try
            {
                var editor = new Windows.SqlEditor(cmd.Configuration, theSide.Provider, fileLink);
                editor.ShowDialog();
            }
            catch (Exception ex)
            {
                stdio.Error(ex.Message);
                return;
            }
        }

        public void open(Command cmd, Configuration cfg)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("open files in the editor");
                stdio.WriteLine("open files");
                stdio.WriteLine("options:");
                stdio.WriteLine("   log              : open log file");
                stdio.WriteLine("   working          : open working directory");
                stdio.WriteLine("   last             : open GUI viewer to see the last data table retrieved");
                stdio.WriteLine("   output           : open output file");
                stdio.WriteLine("   config [/s]      : open user configure file, /s open system configurate");
                stdio.WriteLine("   dpo              : open table class output directory");
                stdio.WriteLine("   dc               : open data contract class output directory");
                stdio.WriteLine("   l2s              : open Linq to SQL class output directory");
                stdio.WriteLine("   release          : open release notes");

                return;
            }

            string path;

            switch (cmd.arg1)
            {
                case "output":
                    stdio.OpenEditor(cfg.OutputFile);
                    break;

                case "log":
                    stdio.OpenEditor(Context.GetValue<string>("log"));
                    break;

                case "config":
                    if (cmd.IsSchema)
                        stdio.OpenEditor("sqlcon.cfg");
                    else
                        stdio.OpenEditor(cfg.CfgFile);
                    break;

                case "release":
                    stdio.OpenEditor("ReleaseNotes.txt");
                    break;

                case "working":
                    path = cfg.WorkingDirectory.CurrentDirectory;
                    OpenDirectory(path, "working directory");
                    break;

                case "dpo":
                    path = cfg.GetValue<string>("dpo.path", $"{Configuration.MyDocuments}\\DataModel\\Dpo");
                    OpenDirectory(path, "dpo class");
                    break;

                case "dc":
                    path = cfg.GetValue<string>("dc.path", $"{Configuration.MyDocuments}\\DataModel\\DataContracts");
                    OpenDirectory(path, "data contract class");
                    break;

                case "l2s":
                    path = cfg.GetValue<string>("l2s.path", $"{Configuration.MyDocuments}\\DataModel\\L2s");
                    OpenDirectory(path, "data Linq to SQL class");
                    break;

                case "last":
                    OpenEditor();
                    break;

                default:
                    stdio.ErrorFormat("invalid arguments");
                    return;
            }


        }

        public void save(Command cmd, Configuration cfg)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("save [file]");
                stdio.WriteLine("options:");
                stdio.WriteLine("  /output       : copy sql script ouput to clipboard");
                stdio.WriteLine("  /last         : last dataset to xml file");
                stdio.WriteLine("example:");
                stdio.WriteLine("  save /output");
                stdio.WriteLine("  save products.xml /last");
                return;
            }

            if (cmd.Has("output"))
            {
                if (!File.Exists(cfg.OutputFile))
                {
                    stdio.ErrorFormat("no output file found : {0}", cfg.OutputFile);
                    return;
                }
                using (var reader = new StreamReader(cfg.OutputFile))
                {
                    string data = reader.ReadToEnd();
                    System.Windows.Clipboard.SetText(data);
                    stdio.WriteLine("copied to clipboard");
                }
            }
            else if (cmd.Has("last"))
            {
                var ds = ShellHistory.LastDataSet();
                if (ds == null)
                {
                    stdio.ErrorFormat("last result is null");
                    return;
                }

                string file = cmd.arg1;
                if (file == null)
                {
                    stdio.ErrorFormat("file name missing");
                    return;
                }

                try
                {
                    string path = Path.GetDirectoryName(file);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    ds.WriteXml(file, XmlWriteMode.WriteSchema);
                    stdio.WriteLine($"last result saved into {file}");
                }
                catch (Exception ex)
                {
                    stdio.Error(ex.Message);
                }
            }
            else
            {
                stdio.ErrorFormat("invalid arguments");
            }

            return;
        }

        public void echo(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("Displays messages, or turns command-echoing on or off");
                stdio.WriteLine("  echo [on | off]");
                stdio.WriteLine("  echo [message]");
                stdio.WriteLine("Type echo without parameters to display the current echo setting.");
                return;
            }

            string text = cmd.args;
            if (string.IsNullOrEmpty(text))
            {
                string status = "on";
                if (!stdio.echo)
                    status = "off";

                Console.WriteLine($"echo is {status}");
                return;
            }

            switch (text)
            {
                case "on":
                    stdio.echo = true;
                    break;

                case "off":
                    stdio.echo = false;
                    break;

                default:
                    stdio.WriteLine(text);
                    break;
            }

            return;
        }
    }
}

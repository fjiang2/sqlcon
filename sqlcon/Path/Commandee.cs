using Sys;
using Sys.Data;
using Sys.Data.Comparison;
using Sys.Data.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Sys.Stdio;
using Tie;

namespace sqlcon
{

    internal class Commandee : ITabCompletion
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
                    cerr.WriteLine("invalid path");
                    return false;
                }
            }

            return true;
        }

        private static bool IsReadonly(TableName tname)
        {
            bool ro = tname.DatabaseName.ServerName.Provider.IsReadOnly;
            if (ro)
                cout.WriteLine("it is read-only table");

            return ro;
        }

        private static bool IsReadonly(DatabaseName dname)
        {
            bool ro = dname.ServerName.Provider.IsReadOnly;
            if (ro)
                cout.WriteLine("it is read-only database");

            return ro;
        }

        private static bool IsReadonly(ServerName sname)
        {
            bool ro = sname.Provider.IsReadOnly;
            if (ro)
                cout.WriteLine("it is read-only database server");

            return ro;
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
                cerr.WriteLine($"invalid path: {path}");
        }

        public bool chdir(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("Change current database directory");
                cout.WriteLine("command cd or chdir");
                cout.WriteLine("cd [path]              : change database directory");
                cout.WriteLine("cd \\                   : change to root directory");
                cout.WriteLine("cd ..                  : change to the parent directory");
                cout.WriteLine("cd ...                 : change to the grand parent directory");
                cout.WriteLine("cd ~                  : change to default database (initial-catalog)");
                cout.WriteLine("cd ~~                   : change to home directory");
                return true;
            }

            if (cmd.wildcard != null)
            {
                cerr.WriteLine("invalid path");
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



        public void dir(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("command dir or ls");
                cout.WriteLine("dir [path]     : display current directory");
                cout.WriteLine("options:");
                cout.WriteLine("   /def        : display table structure");
                cout.WriteLine("   /pk         : display table primary keys");
                cout.WriteLine("   /fk         : display table foreign keys");
                cout.WriteLine("   /ik         : display table identity keys");
                cout.WriteLine("   /dep        : display table dependencies");
                cout.WriteLine("   /ind        : display table index/indices");
                cout.WriteLine("   /sto        : display table storage");
                cout.WriteLine("   /refresh    : refresh table structure");
                cout.WriteLine("   /let:var    : save output to variable \"var\"");
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

        public void set(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("set assignment                      : update value by current table or locator");
                cout.WriteLine("set col1=val1, col2= val2           : update column by current table or locator");
                cout.WriteLine("set col[n1]=val1, col[n2]=val2      : update by row-id, n1,n2 is row-id");
                cout.WriteLine("    --use command type /r to display row-id");
                return;
            }

            if (string.IsNullOrEmpty(cmd.args))
            {
                cerr.WriteLine("argument cannot be empty");
                return;
            }

            var pt = mgr.current;
            if (!(pt.Item is Locator) && !(pt.Item is TableName))
            {
                cerr.WriteLine("table is not selected");
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
                    cerr.WriteLine("invalid set assigment");
                    return;
                }
                catch (Exception ex2)
                {
                    cerr.WriteLine(ex2.Message);
                    return;
                }
            }

            try
            {
                int count = builder.SqlCmd.ExecuteNonQuery();
                cout.WriteLine("{0} of row(s) affected", count);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
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


        public void del(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("command del or erase: drop tables or delete data rows");
                cout.WriteLine("del tablename               : drop table");
                cout.WriteLine("del [sql where clause]      : delete current table filtered rows");
                cout.WriteLine("example:");
                cout.WriteLine(@"local> del Northwind\Products       : drop table [Products]");
                cout.WriteLine(@"local\Northwind\Products> del       : delete all rows of table [Products]");
                cout.WriteLine(@"local\Northwind\Products> del col1=1 and col2='match' : del rows matched on columns:c1 or c2");
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
                                var m = new MatchedDatabase(dname, cmd);
                                T = m.TableNames();
                            }
                            else
                            {
                                var _tname = mgr.GetPathFrom<TableName>(node);
                                if (_tname != null)
                                    T = new TableName[] { _tname };
                                else
                                {
                                    cerr.WriteLine("invalid path");
                                    return;
                                }
                            }
                        }
                        else
                        {
                            cerr.WriteLine("database is unavailable");
                            return;
                        }
                    }
                    else
                    {
                        cerr.WriteLine("invalid path");
                        return;
                    }
                }

                if (T != null && T.Length > 0)
                {
                    if (!cin.YesOrNo($"are you sure to drop {T.Length} tables (y/n)?"))
                        return;

                    try
                    {
                        var sqlcmd = new SqlCmd(T[0].Provider, string.Empty);
                        sqlcmd.ExecuteNonQueryTransaction(T.Select(row => string.Format("DROP TABLE {0}", row)));
                        string text = string.Join<TableName>("\n", T);
                        cerr.WriteLine($"completed to drop table(s):\n{text}");
                    }
                    catch (Exception ex)
                    {
                        cerr.WriteLine(ex.Message);
                    }
                }
                else
                    cerr.WriteLine("table is not selected");

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
                cout.Write("are you sure to delete all rows (y/n)?");
            else
                cout.Write("are you sure to delete (y/n)?");

            if (cin.ReadKey() != ConsoleKey.Y)
                return;

            cout.WriteLine();

            try
            {
                int count;
                if (locator == null)
                    count = new SqlBuilder().DELETE(tname).SqlCmd.ExecuteNonQuery();
                else
                    count = new SqlBuilder().DELETE(tname).WHERE(locator).SqlCmd.ExecuteNonQuery();

                cout.WriteLine("{0} of row(s) affected", count);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
            }
        }


        public void mkdir(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("command md or mkdir");
                cout.WriteLine("md [sql where clause]           : filter current table rows");
                cout.WriteLine("options:");
                cout.WriteLine("   /name:directory              : filter name");
                cout.WriteLine("example:");
                cout.WriteLine("md col1=1                       : filter rows matched on columns:c1");
                cout.WriteLine("md \"col1=1 and col2='match'\"    : filter rows matched on columns:c1 or c2");
                cout.WriteLine("md \"age > 60\" /name:senior      : filter rows matched age>60 and display as senior");
                return;
            }

            TreeNode<IDataPath> pt = mgr.current;

            if (!(pt.Item is TableName) && !(pt.Item is Locator))
            {
                cerr.WriteLine("must add filter underneath table or locator");
                return;
            }

            if (string.IsNullOrEmpty(cmd.args))
                return;

            var xnode = mgr.TryAddWhereOrColumns(pt, cmd);
            //if (xnode != pt)
            //{
            //    //jump to the node just created
            //    mgr.current = xnode;
            //    mgr.Display(xnode, cmd);
            //}
        }

        public void rmdir(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("command rd or rmdir");
                cout.WriteLine("rm [filter name] : remove locators/filters");
                cout.WriteLine("rm #1 : remove the locator node#");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            pt = pt.Parent;

            if (!(pt.Item is TableName))
            {
                cerr.WriteLine("cannot remove filter underneath non-Table");
                return;
            }


            var nodes = pt.Nodes.Where(node => node.Item is Locator && (node.Item as Locator).Path == cmd.Path1.name).ToArray();
            if (nodes.Count() > 0)
            {
                if (!cin.YesOrNo("are you sure to delete (y/n)?"))
                    return;

                foreach (var node in nodes)
                {
                    pt.Nodes.Remove(node);
                }

            }
            else
            {
                if (int.TryParse(cmd.Path1.name, out int result))
                {
                    result--;

                    if (result >= 0 && result < pt.Nodes.Count)
                    {
                        if (!cin.YesOrNo("are you sure to delete (y/n)?"))
                            return;

                        var node = pt.Nodes[result];
                        pt.Nodes.Remove(node);
                    }
                }
            }
        }

        public void type(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("display current data, or search pattern");
                cout.WriteLine("type [path]|[pattern]|[where]  : display current data, or search pattern");
                cout.WriteLine("options:");
                cout.WriteLine("   /top:n              : display top n records");
                cout.WriteLine("   /all                : display all records");
                cout.WriteLine("   /t                  : display table in vertical grid");
                cout.WriteLine("   /r                  : display row-id");
                cout.WriteLine("   /json               : generate json data");
                cout.WriteLine("   /dup                : list duplicated rows, e.g. type /dup /col:c1,c2");
                cout.WriteLine("   /col:c1,c2,..       : display columns, or search on columns");
                cout.WriteLine("   /edit               : edit mode");
                cout.WriteLine("example:");
                cout.WriteLine("type match*s /col:c1,c2 : display rows matched on columns:c1 or c2");
                cout.WriteLine("type id=20             : display rows where id=20");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            if (!mgr.TypeFile(pt, cmd))
                cerr.WriteLine("invalid arguments");
        }




        public void copy(ApplicationCommand cmd, CompareSideType sideType)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("use \"/pk:table1=pk1+pk2,table=pk1\" if primary key doesn't exist");
                if (sideType == CompareSideType.copy)
                {
                    cout.WriteLine("copy schema or records from table1 to table2, support table name wildcards");
                    cout.WriteLine("copy table1 [table2] [/s]");
                }
                else if (sideType == CompareSideType.sync)
                {
                    cout.WriteLine("synchronize schema or records from table1 to table2");
                    cout.WriteLine("sync table1 [table2] [/s] : sync table1' records to table2");
                }
                else if (sideType == CompareSideType.compare)
                {
                    cout.WriteLine("compare schema or records from table1 to table2");
                    cout.WriteLine("comp table1 [table2] [/s] : sync table1' records to table2");
                }
                cout.WriteLine("support table name wildcards");
                cout.WriteLine("[/s]                       : table schema, default table records");
                return;
            }

            CancelableWork.CanCancel(cts =>
            {
                PathBothSide both = new PathBothSide(mgr, cmd);
                var dname2 = mgr.GetPathFrom<DatabaseName>(both.ps2.Node);
                if (both.ps1.MatchedTables == null || both.ps1.MatchedTables.Length == 0)
                {
                    cout.WriteLine("no table found");
                    return;
                }

                foreach (var tname1 in both.ps1.MatchedTables)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    TableName tname2 = mgr.GetPathFrom<TableName>(both.ps2.Node);
                    if (tname2 == null)
                    {
                        tname2 = new TableName(dname2, tname1.SchemaName, tname1.ShortName);
                    }

                    var adapter = new CompareAdapter(cmd, both.ps1.side, both.ps2.side);
                    //stdio.WriteLine("start to {0} from {1} to {2}", sideType, tname1, tname2);
                    var sql = adapter.CompareTable(cmd.IsSchema ? ActionType.CompareSchema : ActionType.CompareData,
                        sideType, tname1, tname2, cmd.PK, cmd.Columns);

                    if (sideType == CompareSideType.compare)
                    {
                        if (sql == string.Empty)
                        {
                            cout.WriteLine("source {0} and destination {1} are identical, or table is not found", tname1, tname2);
                        }
                        continue;
                    }

                    if (sql == string.Empty)
                    {
                        cout.WriteLine("nothing changes made on destination {0}", tname2);
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
                                    cout.WriteLine("{0} row(s) changed at destination {1}", count, tname2);
                                else
                                    cout.WriteLine("command(s) completed successfully at destination {1}", count, tname2);
                            }
                            else
                                cout.WriteLine("table {0} created at destination", tname2);
                        }
                        catch (Exception ex)
                        {
                            cerr.WriteLine(ex.Message);
                            return;
                        }
                    }
                } // loop for

                return;
            });
        }

        public void compare(ApplicationCommand cmd, IConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("compare table schema or records");
                cout.WriteLine("compare path1 [path2]  : compare data");
                cout.WriteLine("compare [/s]           : compare schema");
                cout.WriteLine("compare [/e]           : find common existing table names");
                cout.WriteLine("compare [/count]       : compare number of rows");
                cout.WriteLine("        [/pk]          : if primary key doesn't exist");
                cout.WriteLine("                         for example /pk:table1=pk1+pk2,table=pk1");
                cout.WriteLine();
                return;
            }

            PathBothSide both = new PathBothSide(mgr, cmd);
            string fileName = cmd.OutputFile(cfg);
            using (var writer = fileName.CreateStreamWriter(cmd.Append))
            {
                ActionType type;
                if (cmd.IsSchema)
                    type = ActionType.CompareSchema;
                else
                    type = ActionType.CompareData;

                if (cmd.Has("count"))
                    type = ActionType.CompareRowCount;

                if (both.Invalid)
                {
                    return;
                }

                var adapter = new CompareAdapter(cmd, both.ps1.side, both.ps2.side);
                var T1 = both.ps1.MatchedTables;
                var T2 = both.ps2.MatchedTables;

                if (cmd.Has("e"))   //find common existing table names
                {
                    var L1 = T1.Select(t => t.ShortName.ToUpper());
                    var L2 = T2.Select(t => t.ShortName.ToUpper());
                    var C = L1.Intersect(L2).ToArray();

                    T1 = T1.Where(t => C.Contains(t.ShortName.ToUpper())).ToArray();
                    T2 = T2.Where(t => C.Contains(t.ShortName.ToUpper())).ToArray();
                }

                var sql = adapter.Run(type, T1, T2, cmd);
                writer.Write(sql);
            }
            cout.WriteLine($"result in \"{fileName}\"");
        }

        public void rename(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("rename column name, table name, modify column");
                cout.WriteLine();
                cout.WriteLine("ren [database] newdatasbase   : rename database or current database to newdatabase");
                cout.WriteLine("ren [table] newtable          : rename table or current table to newtable");
                cout.WriteLine("ren column newcolumn          : rename column on current table to newcolumn");
                cout.WriteLine();
                return;
            }

            if (cmd.arg1 == null)
            {
                cerr.WriteLine("invalid argument");
                return;
            }
        }

        public void attrib(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("command attrib: update column property");
                cout.WriteLine("add primary key, foreign key or identity key");
                cout.WriteLine("columns:");
                cout.WriteLine("  attrib [table] +c:col1=varchar(2)+null : add column or alter column");
                cout.WriteLine("  attrib [table] +c:col1=varchar(10)     : add column or alter column");
                cout.WriteLine("  attrib [table] -c:col1                 : remove column");
                cout.WriteLine("primary keys:");
                cout.WriteLine("  attrib [table] +p:col1,col2            : add primary key");
                cout.WriteLine("  attrib [table] +p:col1,col2            : remove primary key");
                cout.WriteLine("foreign keys:");
                cout.WriteLine("  attrib [table] +f:col1=table2[.col2]   : add foreign key");
                cout.WriteLine("  attrib [table] -f:col1                 : remove foreign key");
                cout.WriteLine("identiy key:");
                cout.WriteLine("  attrib [table] +i:col1                 : add identity");
                cout.WriteLine("  attrib [table] -i:col1                 : remove identity");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;


            if (!(pt.Item is TableName))
            {
                cerr.WriteLine("table is not selected");
                return;
            }


            if (cmd.options.Has("+c"))
            {
                TableName tname = (TableName)pt.Item;
                string expr = cmd.options.GetValue("+c");
                string[] items = expr.Split(new string[] { "=", "+" }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length != 2 && items.Length != 3)
                {
                    cerr.WriteLine($"invalid expression:{expr}, correct is col1=type or col1=type+null");
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
                    cerr.WriteLine($"invalid foreign key expression:{expr}, correct is col1=pktable.col2");
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
                    cerr.WriteLine($"invalid foreign key expression:{expr}, correct is col1=pktable.col2");
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
                    cerr.WriteLine($"fails in generating foreign key constraint name, {ex.Message}");
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
                cerr.WriteLine(ex.Message);
            }

            return -1;
        }

        public void let(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("let assignment              : variable assign statement ");
                cout.WriteLine("let key=value               : update column by current table or locator");
                cout.WriteLine("examples:");
                cout.WriteLine("let Host=\"127.0.0.1\"      : value of variable Host is \"127.0.0.1\"");
                cout.WriteLine("let a=12                    : value of variable a is 12");
                return;
            }

            if (string.IsNullOrEmpty(cmd.args))
            {
                cerr.WriteLine("assignment cannot be empty");
                return;
            }

            try
            {
                Script.Execute($"{cmd.args};", Context.DS);
            }
            catch (Exception ex)
            {
                cerr.WriteLine($"execute error: {ex.Message}");
            }
        }

        public void let1(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("let assignment              : update key-value table row, key-value table must be defined on the sqlcon.cfg or user.cfg");
                cout.WriteLine("let key=value               : update column by current table or locator");
                cout.WriteLine("example:");
                cout.WriteLine("let Smtp.Host=\"127.0.0.1\" : update key-value row, it's equivalent to UPDATE table SET [Value]='\"127.0.0.1\"' WHERE [Key]='Smtp.Host'");
                return;
            }

            if (string.IsNullOrEmpty(cmd.args))
            {
                cerr.WriteLine("argument cannot be empty");
                return;
            }

            var pt = mgr.current;
            if (!(pt.Item is Locator) && !(pt.Item is TableName))
            {
                cerr.WriteLine("table is not selected");
                return;
            }

            KeyValueTable setting = new KeyValueTable
            {
                TableName = "Config",
                KeyName = "Key",
                ValueName = "Value",
            };

            TableName tname = mgr.GetCurrentPath<TableName>();

            if (setting == null)
            {
                cerr.WriteLine("current table is not key-value tables");
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
                cerr.WriteLine("invalid assignment");
                return;
            }

            Locator locator = new Locator(setting.KeyName.ColumnName() == key);
            SqlBuilder builder = new SqlBuilder().SELECT.COLUMNS(setting.ValueName.ColumnName()).FROM(tname).WHERE(locator);
            var L = new SqlCmd(builder).FillDataColumn<string>(0);
            if (L.Count() == 0)
            {
                cerr.WriteLine($"undefined key: {key}");
                return;
            }

            if (kvp.Length == 1)
            {
                cerr.WriteLine($"{key} = {L.First()}");
                return;
            }

            builder = new SqlBuilder()
                .UPDATE(tname)
                .SET(setting.ValueName.ColumnName() == value)
                .WHERE(locator);

            try
            {
                int count = builder.SqlCmd.ExecuteNonQuery();
                cout.WriteLine("{0} of row(s) affected", count);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
            }
        }

        public void clean(ApplicationCommand cmd, IConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("clean duplicated rows");
                cout.WriteLine("clean [path]|[pattern]|  : clean current database or table, or search pattern");
                cout.WriteLine("options:");
                cout.WriteLine("   /col:c1,c2,..         : clean columns, compare column c1, c2, ...");
                cout.WriteLine("   /d                    : commit cleaning duplicated rows on database server, otherwise display # of duplicated rows");
                cout.WriteLine("example:");
                cout.WriteLine("clean match*s /col:c1,c2 : clean duplicated rows by comparing columns:c1 and c2");
                cout.WriteLine("clean                    : clean by comparing entire row");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            if (pt.Item is TableName tname)
            {
                if (IsReadonly(tname))
                    return;

                var dup = new DuplicatedTable(tname, cmd.Columns);
                if (cmd.Has("d"))
                {
                    int count = dup.Clean();
                    cout.WriteLine("completed to clean {0} #rows at {1}", count, tname);
                }
                else
                {
                    int count = dup.DuplicatedRowCount();
                    if (count == 0)
                        cout.WriteLine("no duplicated rows at {0}", tname);
                    else
                        cout.WriteLine("{0} duplicated row(s) at {1}", count, tname);
                }
                return;
            }


            if (pt.Item is DatabaseName dname)
            {
                if (IsReadonly(dname))
                    return;


                var m = new MatchedDatabase(dname, cmd);
                var T = m.TableNames();

                CancelableWork.CanCancel(cts =>
                {
                    foreach (var tn in T)
                    {
                        if (cts.IsCancellationRequested)
                            return;

                        if (cmd.Has("d"))
                        {
                            cout.WriteLine("start to clean {0}", tn);
                            var dup = new DuplicatedTable(tn, cmd.Columns);
                            int count = dup.Clean();
                            cout.WriteLine("cleaned {0} #rows", count);
                        }
                        else
                        {
                            cout.WriteLine("start to query {0}", tn);
                            var dup = new DuplicatedTable(tn, cmd.Columns);
                            int count = dup.DuplicatedRowCount();
                            if (count == 0)
                                cout.WriteLine("distinct rows");
                            else
                                cout.WriteLine("{0} duplicated row(s)", count, tn);
                        }

                    }


                });

                return;
            }

            cerr.WriteLine("select database or table first");
        }

        public void import(ApplicationCommand cmd, IConfiguration cfg, ShellContext context)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("import file");
                cout.WriteLine("option:");
                cout.WriteLine("   /fmt:xml,ds   : load System.Data.DataSet xml file as last result");
                cout.WriteLine("   /fmt:xml,dt   : load System.Data.DataTable xml file as last result");
                cout.WriteLine("   /fmt:txt      : load text file and import into current table");
                cout.WriteLine("   /fmt:csv      : import .csv data into current table");
                cout.WriteLine("      [/col:c1,c2,...] csv columns mapping");
                cout.WriteLine("   /fmt:cfg      : import .cfg data into current config table");
                cout.WriteLine("      [/key:column] column of key on config table");
                cout.WriteLine("      [/value:column] column of value config table");
                cout.WriteLine("      [/col:c1=v1,c2=v2,...] default values for not null columns");
                cout.WriteLine("e.g. import c:\\conf.cfg /fmt:cfg /key:Key /value:Value /col:[Inactive]=0");
                return;
            }

            string file = cmd.arg1;
            if (file == null)
            {
                cerr.WriteLine("file name not specified");
                return;
            }

            if (!File.Exists(file))
            {
                cerr.WriteLine($"cannot find the file \"{file}\"");
                return;
            }

            string fmt = cmd.GetValue("fmt");
            if (fmt == null)
            {
                string ext = Path.GetExtension(file);
                if (ext.StartsWith("."))
                    fmt = ext.Substring(1).ToLower();
            }

            switch (fmt)
            {
                case "xml":
                case "xml,ds":
                    var ds = new DataSet();
                    try
                    {
                        ds.ReadXml(file, XmlReadMode.ReadSchema); ;
                        ShellHistory.SetLastResult(ds);
                        cout.WriteLine($"{typeof(DataSet).FullName} xml file \"{file}\" has been loaded");
                    }
                    catch (Exception ex)
                    {
                        cerr.WriteLine($"invalid {typeof(DataSet).FullName} xml file, {ex.Message}");
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
                        cerr.WriteLine($"invalid {typeof(DataTable).FullName} xml file, {ex.Message}");
                        return;
                    }
                    cout.WriteLine($"{typeof(DataTable).FullName} xml file \"{file}\" has been loaded");
                    break;

                case "txt":
                    break;

                case "cfg":
                case "csv":
                    TableName tname = mgr.GetCurrentPath<TableName>();
                    if (tname == null)
                    {
                        cerr.WriteLine("cannot find the table to import data");
                        return;
                    }

                    int count = 0;
                    var importer = new Importer(cmd);
                    if (fmt == "csv")
                        count = importer.ImportCsv(file, tname, cmd.Columns);
                    else if (fmt == "cfg")
                        count = importer.ImportCfg(file, tname);

                    cout.WriteLine($"{count} row(s) imported");
                    break;

                case "tie":
                    new TieClassBuilder(cmd).Done();
                    break;

                default:
                    cerr.WriteLine("invalid command");
                    break;
            }
        }

        public void export(ApplicationCommand cmd, IConfiguration cfg, ShellContext context)
        {
            if (cmd.HasHelp)
            {
                Exporter.Help();
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            if (pt.Item is TableName || pt.Item is Locator || pt.Item is DatabaseName || pt.Item is ServerName)
            {
                var exporter = new Exporter(mgr, pt, cmd, cfg);
                exporter.Run();
            }
            else
                cerr.WriteLine("select server, database or table first");
        }

        public void mount(ApplicationCommand cmd, IConnectionConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("mount database server");
                cout.WriteLine("mount alias=server_name   : alias must start with letter");
                cout.WriteLine("options:");
                cout.WriteLine("   /db:database           : initial catalog, default is 'master'");
                cout.WriteLine("   /u:username            : user id, default is 'sa'");
                cout.WriteLine("   /p:password            : password, default is empty, use Windows Security when /u /p not setup");
                cout.WriteLine("   /pvd:provider          : default is SQL Server client");
                cout.WriteLine("        sqldb               SQL Server, default provider");
                cout.WriteLine("        sqloledb            ODBC Database Server");
                cout.WriteLine("        file/db/xml         sqlcon Database Schema, default provider for xml file");
                cout.WriteLine("        file/dataset/json   System.Data.DataSet");
                cout.WriteLine("        file/dataset/xml    System.Data.DataSet");
                cout.WriteLine("        file/datalake/json  Dictionary<string, System.Data.DataSet>");
                cout.WriteLine("        file/datalake/xml   Dictionary<string, System.Data.DataSet>");
                cout.WriteLine("        file/assembly       .Net assembly dll");
                cout.WriteLine("        file/c#             C# data contract classes");
                cout.WriteLine("        riadb               Remote Invoke Agent");
                cout.WriteLine("example:");
                cout.WriteLine("  mount ip100=192.168.0.100\\sqlexpress /u:sa /p:p@ss");
                cout.WriteLine("  mount web=http://192.168.0.100/db/northwind.xml /u:sa /p:p@ss");
                cout.WriteLine("  mount xml=file://c:\\db\\northwind.xml");
                cout.WriteLine("  mount cs=file://c:\\db\\northwind.cs /pvd:file/c#");
                return;
            }

            if (cmd.arg1 == null)
            {
                cerr.WriteLine("invalid arguments");
                return;
            }

            var items = cmd.arg1.Split('=');
            if (items.Length != 2)
            {
                cerr.WriteLine("invalid arguments, correct format is alias=server_name");
                return;
            }
            string serverName = items[0].Trim();
            string dataSource = items[1].Trim();

            StringBuilder builder = new StringBuilder();
            string pvd = cmd.GetValue("pvd");
            if (pvd != null)
            {
                if (pvd != "sqloledb" && pvd != "xmlfile" && !pvd.StartsWith("file/"))
                {
                    cerr.WriteLine($"provider={pvd} is not supported");
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
                cerr.WriteLine("database is offline or wrong parameter");
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

        public void umount(ApplicationCommand cmd, IConnectionConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("unmount database server");
                cout.WriteLine("unmount alias             : alias must start with letter");
                cout.WriteLine("example:");
                cout.WriteLine("  umount ip100");
                return;
            }

            if (cmd.arg1 == null)
            {
                cerr.WriteLine("invalid arguments");
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

                cout.WriteLine($"umount server \"{serverName}\" done");
            }
            else
            {
                cerr.WriteLine($"server \"{serverName}\" doesn't exist");
                return;
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
                cerr.WriteLine("select table first");
                return;
            }
#if WINDOWS
            var editor = new Windows.TableEditor(new UniqueTable(null, dt));

            editor.ShowDialog();
#else
            cerr.WriteLine("doesn't support to open editor");
#endif
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
                cerr.WriteLine($"{message} path not exist: {path}");
        }

        public void xcopy(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("xcopy large size records, support table/database name wildcards");
                cout.WriteLine("   table must have same structure");
                cout.WriteLine("xcopy database1 [database2]");
                cout.WriteLine("xcopy table1 [table2]");
                cout.WriteLine("       /col:c1[=d1],c2[=d2],...         copy selected columns (mapping)");
                cout.WriteLine("       /s                               compare table schema");
                cout.WriteLine("note that: to xcopy selected records of table, mkdir locator first, example:");
                cout.WriteLine(@"  \local\NorthWind\Products> md ProductId<200 /name:p200");
                cout.WriteLine(@"  \local\NorthWind\Products> xcopy p200 \local\db");
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
                            cerr.WriteLine("destination table is not compatible or doesn't exist");
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
                        cerr.WriteLine($"total count={_cnt}, too many rows, progress bar may not be accurate");
                    }
                    cout.Write($"copying {tname1.Name} ");
                    using (var progress = new ProgressBar { Count = count })
                    {
                        TableBulkCopy bulkCopy = new TableBulkCopy(tableReader);
                        try
                        {
                            bulkCopy.CopyTo(tname2, maps.ToArray(), cts, progress);
                        }
                        catch (Exception ex)
                        {
                            cerr.WriteLine(ex.Message);
                        }

                        if (cts.IsCancellationRequested)
                            progress.Report(count);
                    }

                    if (!cts.IsCancellationRequested)
                        cout.WriteLine($", Done on rows({_cnt}).");
                }
            });
        }

        public void execute(ApplicationCommand cmd, IConfiguration cfg, Side theSide)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("execute sql script file");
                cout.WriteLine("execute file (.sql)");
                cout.WriteLine("examples:");
                cout.WriteLine("  execute northwind.sql       : execute single sql script file");
                return;
            }

            string inputfile;
            if (cmd.arg1 != null)
                inputfile = cfg.WorkingDirectory.GetFullPath(cmd.arg1, ".sql");
            else
            {
                cerr.WriteLine("input undefined");
                return;
            }

            if (theSide.ExecuteScript(inputfile))
                ErrorCode = CommandState.OK;
            else
                ErrorCode = CommandState.SQL_FAILS;
        }

        public void edit(ApplicationCommand cmd, IConfiguration cfg, IConnectionConfiguration connection, Side theSide)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("edit, view and execute sql script");
                cout.WriteLine("edit                          : create new file and edit");
                cout.WriteLine("edit [file]                   : edit file, it is read-only if file is hyperlink");
                cout.WriteLine("options:");
                cout.WriteLine("   /usr                       : FTP user name");
                cout.WriteLine("   /pwd                       : FTP password");
                cout.WriteLine("examples:");
                cout.WriteLine("  edit c:\\db\\northwind.sql");
                cout.WriteLine("  edit file://datconn/northwind.sql");
                cout.WriteLine("  edit http://www.datconn.com/demos/northwind.sql");
                cout.WriteLine("  edit ftp://www.datconn.com/demos/northwind.sql /usr:user /pwd:password");
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
                        string path = cfg.GetValue<string>("MyDocuments", Directory.GetCurrentDirectory());
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
                            cerr.WriteLine($"file {fileLink} doesn't exist");
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
                    cerr.WriteLine(ex.Message);
                    return;
                }

            }
#if WINDOWS
            try
            {
                var editor = new Windows.SqlEditor(connection, theSide.Provider, mgr.ToString(), fileLink);
                editor.ShowDialog();
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
                return;
            }
#else
			cerr.WriteLine("doesn't support editor");
#endif			
        }

        public void open(ApplicationCommand cmd, IConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("open file in the editor or open directory in the Explorer");
                cout.WriteLine("open file or directory ");
                cout.WriteLine("options:");
                cout.WriteLine("   log              : open log file");
                cout.WriteLine("   working          : open working directory");
                cout.WriteLine("   last             : open GUI viewer to see the last data table retrieved");
                cout.WriteLine("   output           : open output file");
                cout.WriteLine("   config [/s]      : open user configure file, /s open system configurate");
                cout.WriteLine("   dpo              : open table class output directory");
                cout.WriteLine("   dc|dc1|dc2       : open data contract class output directory");
                cout.WriteLine("   l2s              : open Linq to SQL class output directory");
                cout.WriteLine("   release          : open release notes");
                cout.WriteLine("   file-name.sqc    : open batch file");
                cout.WriteLine("   file-name.sql    : open SQL script file");
                cout.WriteLine("   file-name.sqt    : open Tie file");

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
                        stdio.OpenEditor(cfg.UserConfigurationFile);
                    break;

                case "release":
                    stdio.OpenEditor("ReleaseNotes.txt");
                    break;

                case "working":
                    path = cfg.WorkingDirectory.CurrentDirectory;
                    OpenDirectory(path, "working directory");
                    break;

                case "dpo":
                    path = cfg.GetValue<string>(ConfigKey._GENERATOR_DPO_PATH, $"{Configuration.MyDocuments}\\DataModel\\Dpo");
                    OpenDirectory(path, "dpo class");
                    break;

                case "dc":
                case "dc1":
                case "dc2":
                    path = cfg.GetValue<string>(ConfigKey._GENERATOR_DC_PATH, $"{Configuration.MyDocuments}\\DataModel\\DataContracts");
                    OpenDirectory(path, "data contract class");
                    break;

                case "l2s":
                    path = cfg.GetValue<string>(ConfigKey._GENERATOR_L2S_PATH, $"{Configuration.MyDocuments}\\DataModel\\L2s");
                    OpenDirectory(path, "data Linq to SQL class");
                    break;

                case "last":
                    OpenEditor();
                    break;

                default:
                    if (open(cmd.arg1))
                        return;

                    cerr.WriteLine("invalid arguments");
                    return;
            }

            bool open(string filename)
            {
                string[] EXT = new string[] { ".sqc", ".sql", ".sqt" };
                foreach (string ext in EXT)
                {
                    string _path = cfg.WorkingDirectory.GetFullPath(filename, ext);
                    if (File.Exists(_path))
                    {
                        stdio.OpenEditor(_path);
                        return true;
                    }
                }

                return false;
            }
        }

        public void save(ApplicationCommand cmd, IConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("save [file]");
                cout.WriteLine("options:");
                cout.WriteLine("  /output       : copy sql script ouput to clipboard for Windows only");
                cout.WriteLine("example:");
                cout.WriteLine("  save /output");
                return;
            }

            if (cmd.Has("output"))
            {
                if (!File.Exists(cfg.OutputFile))
                {
                    cerr.WriteLine($"no output file found : {cfg.OutputFile}");
                    return;
                }
                using (var reader = new StreamReader(cfg.OutputFile))
                {
                    string data = reader.ReadToEnd();
#if WINDOWS					
                    System.Windows.Clipboard.SetText(data);
                    cout.WriteLine("copied to clipboard");
#endif
                }
            }
            else
            {
                cerr.WriteLine("invalid arguments");
            }

            return;
        }

        public void echo(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("Displays messages, or turns command-echoing on or off");
                cout.WriteLine("  echo [on | off]");
                cout.WriteLine("  echo [message]");
                cout.WriteLine("Type echo without parameters to display the current echo setting.");
                return;
            }

            string text = cmd.args;
            if (string.IsNullOrEmpty(text))
            {
                string status = "on";
                if (!cout.echo)
                    status = "off";

                cout.WriteLine($"echo is {status}");
                return;
            }

            switch (text)
            {
                case "on":
                    cout.echo = true;
                    break;

                case "off":
                    cout.echo = false;
                    break;

                default:
                    cout.WriteLine(text);
                    break;
            }

            return;
        }


        public void check(ApplicationCommand cmd, Side theSide)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("check data correctness");
                cout.WriteLine("check [path]                   : check data on current table");
                cout.WriteLine("options:");
                cout.WriteLine("   /syntax                       : check key-value pair syntax");
                cout.WriteLine("   /key:c1                     : column name of key variable");
                cout.WriteLine("   /value:c2                   : column name of value expression");
                cout.WriteLine("examples:");
                cout.WriteLine("  check  dbo.config /syntax /key:Key /value:Value");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            if (!(pt.Item is TableName))
            {
                cerr.WriteLine("table is not selected");
                return;
            }

            TableName tname = pt.Item as TableName;

            if (cmd.Has("syntax"))
            {
                string colKey = cmd.GetValue("key") ?? "Key";
                string colValue = cmd.GetValue("value") ?? "Value";

                SqlBuilder builder = new SqlBuilder().SELECT.COLUMNS(new string[] { colKey, colValue }).FROM(tname);
                var L = new SqlCmd(builder).ToList(row => new { Key = row.GetField<string>(colKey), Value = row.GetField<string>(colValue) });

                Memory DS = new Memory();
                foreach (var kvp in L)
                {
                    try
                    {
                        Script.Execute($"{kvp.Key}=0;", DS);
                    }
                    catch (Exception)
                    {
                        cerr.WriteLine($"invalid key={kvp.Key}");
                    }

                    try
                    {
                        VAL val = Script.Evaluate(kvp.Value, DS);
                    }
                    catch (Exception ex)
                    {
                        cerr.WriteLine($"invalid value={kvp.Value} on key={kvp.Key}, {ex.Message}");
                    }
                }

                cout.WriteLine($"{L.Count()} items checking completed");
                return;
            }


            cerr.WriteLine($"invalid command");
            return;
        }


        public void last(ApplicationCommand cmd, IConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("last command display, load or save last dataset");
                cout.WriteLine("last [path]                :");
                cout.WriteLine("options:");
                cout.WriteLine("  /load                    : load C#, json or xml file to last dataset");
                cout.WriteLine("  /save                    : save last dataset to json or xml file");
                cout.WriteLine("  /datalake                : format of json file is data lake");
                cout.WriteLine("example:");
                cout.WriteLine("  last                     : display last dataset");
                cout.WriteLine("  last products.cs         : display dataset file in c# format");
                cout.WriteLine("  last products.xml        : display dataset file in xml format");
                cout.WriteLine("  last products.json       : display dataset file in json format");
                cout.WriteLine("  last lake.json  /datalake: display data lake file");
                cout.WriteLine("  last products.xml /save  : save last dataset to a xml file");
                cout.WriteLine("  last /save               : use table name as file name and save");
                cout.WriteLine("  last products.json /save : save last dataset to a json file");
                cout.WriteLine("  last products.cs  /load  : load c# file to last dataset");
                cout.WriteLine("  last products.xml /load  : load xml file to last dataset");
                cout.WriteLine("  last products.json /load : load json file to last dataset");
                return;
            }

            string file = cmd.arg1;
            if (file == null && !cmd.Has("save"))
            {
                DataSet ds = ShellHistory.LastDataSet();
                if (ds != null)
                {
                    foreach (DataTable dt in ds.Tables)
                    {
                        cout.WriteLine($"[{dt.TableName}]");
                        dt.ToConsole();
                    }
                }
                else
                    cout.WriteLine("last result is not found");

                return;
            }


            if (cmd.Has("save"))
            {
                try
                {
                    var ds = ShellHistory.LastDataSet();
                    if (ds == null || ds.Tables.Count == 0)
                    {
                        cerr.WriteLine("last result is null");
                        return;
                    }

                    if (file == null)
                        file = ds.Tables[0].TableName + ".xml";

                    file.WriteDataSet(ds);
                    cout.WriteLine($"last result saved into {file}");
                }
                catch (Exception ex)
                {
                    cerr.WriteLine(ex.Message);
                }

                return;
            }


            if (cmd.Has("load"))
            {
                try
                {
                    var ds = file.ReadDataSet();
                    if (ds == null)
                        return;

                    ShellHistory.SetLastResult(ds);
                    cout.WriteLine($"{typeof(DataSet).FullName} file \"{file}\" loaded");
                }
                catch (Exception ex)
                {
                    cerr.WriteLine($"invalid data set file: {file}, {ex.Message}");
                }

                return;
            }


            if (cmd.Has("datalake"))
            {
                //display data lake
                try
                {
                    if (Path.GetExtension(file) == string.Empty)
                        file = Path.ChangeExtension(file, ".json");

                    string json = File.ReadAllText(file);
                    DataLake lake = new DataLake();
                    lake.ReadJson(json);
                    if (lake == null)
                        return;

                    string output = cmd.OutputPath();
                    if (output == null)
                    {
                        foreach (var kvp in lake)
                        {
                            cout.WriteLine($"\"{kvp.Key}\"");
                            DataSet ds = kvp.Value;
                            foreach (DataTable dt in ds.Tables)
                            {
                                cout.WriteLine($"[{dt.TableName}]");
                                dt.ToConsole();
                            }

                            cout.WriteLine();
                        }
                    }
                    else
                    {
                        string directory = Path.GetDirectoryName(output);
                        if (directory != string.Empty)
                        {
                            if (!Directory.Exists(directory))
                                Directory.CreateDirectory(directory);
                        }

                        using (var writer = new StreamWriter(output))
                        {
                            foreach (var kvp in lake)
                            {
                                writer.WriteLine($"\"{kvp.Key}\"");
                                DataSet ds = kvp.Value;
                                foreach (DataTable dt in ds.Tables)
                                {
                                    writer.WriteLine($"[{dt.TableName}]");
                                    new OutputDataTable(dt, writer, vertical: false).Output();
                                    writer.WriteLine("<{0} row{1}>", dt.Rows.Count, dt.Rows.Count > 1 ? "s" : "");
                                }

                                writer.WriteLine();
                            }
                        }

                        cout.WriteLine($"saved into \"{output}\"");
                    }
                }
                catch (Exception ex)
                {
                    cerr.WriteLine($"invalid data lake file:{file}, {ex.Message}");
                }

                return;
            }

            //display data set
            DataSet old = ShellHistory.LastDataSet();
            try
            {
                var ds = file.ReadDataSet();
                if (ds == null)
                    return;

                foreach (DataTable dt in ds.Tables)
                {
                    cout.WriteLine($"[{dt.TableName}]");
                    dt.ToConsole();
                }
            }
            catch (Exception ex)
            {
                cerr.WriteLine($"invalid data set file:{file}, {ex.Message}");
                return;
            }
            ShellHistory.SetLastResult(old);

            return;
        }

        public void find(ApplicationCommand cmd, string match)
        {
            if (cmd.HasHelp)
            {
                cout.WriteLine("find command searches name of database, schema, table, view, or column");
                cout.WriteLine("example:");
                cout.WriteLine("  find *ID*           : search any string contains ID");
                cout.WriteLine("  find *na?e          : search string ends with na?e");
                return;
            }

            if (match == null)
            {
                cerr.WriteLine("find pattern is undefined");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            if (pt.Item is ServerName)
            {
                ServerName sname = (ServerName)pt.Item;
                Tools.FindName(sname.Provider, sname.GetDatabaseNames(), match);
            }
            else if (pt.Item is DatabaseName)
            {
                DatabaseName dname = (DatabaseName)pt.Item;
                Tools.FindName(dname.Provider, new DatabaseName[] { dname }, match);
            }

        }

    }
}

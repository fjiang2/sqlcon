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
using System.Data.Common;
using Sys.Stdio;
using Sys.Data.Resource;
using Tie;

namespace SqlCon
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
                    Cerr.WriteLine("invalid path");
                    return false;
                }
            }

            return true;
        }

        private static bool IsReadonly(TableName tname)
        {
            bool ro = tname.DatabaseName.ServerName.Provider.IsReadOnly;
            if (ro)
                Cout.WriteLine("it is read-only table");

            return ro;
        }

        private static bool IsReadonly(DatabaseName dname)
        {
            bool ro = dname.ServerName.Provider.IsReadOnly;
            if (ro)
                Cout.WriteLine("it is read-only database");

            return ro;
        }

        private static bool IsReadonly(ServerName sname)
        {
            bool ro = sname.Provider.IsReadOnly;
            if (ro)
                Cout.WriteLine("it is read-only database server");

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
                Cerr.WriteLine($"invalid path: {path}");
        }

        public bool chdir(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("Change current database directory");
                Cout.WriteLine("command cd or chdir");
                Cout.WriteLine("cd [path]              : change database directory");
                Cout.WriteLine("cd \\                   : change to root directory");
                Cout.WriteLine("cd ..                  : change to the parent directory");
                Cout.WriteLine("cd ...                 : change to the grand parent directory");
                Cout.WriteLine("cd ~                  : change to default database (initial-catalog)");
                Cout.WriteLine("cd ~~                   : change to home directory");
                return true;
            }

            if (cmd.wildcard != null)
            {
                Cerr.WriteLine("invalid path");
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
                Cout.WriteLine("command dir or ls");
                Cout.WriteLine("dir [path]     : display current directory");
                Cout.WriteLine("options:");
                Cout.WriteLine("   /def        : display table structure");
                Cout.WriteLine("   /pk         : display table primary keys");
                Cout.WriteLine("   /fk         : display table foreign keys");
                Cout.WriteLine("   /ik         : display table identity keys");
                Cout.WriteLine("   /dep        : display table dependencies");
                Cout.WriteLine("   /ind        : display table index/indices");
                Cout.WriteLine("   /sto        : display table storage");
                Cout.WriteLine("   /refresh    : refresh table structure");
                Cout.WriteLine("   /let:var    : save output to variable \"var\"");
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
                Cout.WriteLine("set assignment                      : update value by current table or locator");
                Cout.WriteLine("set col1=val1, col2= val2           : update column by current table or locator");
                Cout.WriteLine("set col[n1]=val1, col[n2]=val2      : update by row-id, n1,n2 is row-id");
                Cout.WriteLine("    --use command type /r to display row-id");
                return;
            }

            if (string.IsNullOrEmpty(cmd.Args))
            {
                Cerr.WriteLine("argument cannot be empty");
                return;
            }

            var pt = mgr.current;
            if (!(pt.Item is Locator) && !(pt.Item is TableName))
            {
                Cerr.WriteLine("table is not selected");
                return;
            }

            Locator locator = mgr.GetCombinedLocator(pt);
            TableName tname = mgr.GetCurrentPath<TableName>();

            SqlBuilder builder = new SqlBuilder().UPDATE(tname).SET(cmd.Args);
            if (locator != null)
            {
                builder.WHERE(locator);
            }
            else if (mgr.Tout != null && mgr.Tout.TableName == tname && mgr.Tout.HasPhysloc)
            {
                try
                {
                    var x = ParsePhysLocStatement(mgr.Tout.Table, cmd.Args);
                    if (x != null)
                        builder = x;
                }
                catch (TieException)
                {
                    Cerr.WriteLine("invalid set assigment");
                    return;
                }
                catch (Exception ex2)
                {
                    Cerr.WriteLine(ex2.Message);
                    return;
                }
            }

            try
            {
                int count = builder.SqlCmd.ExecuteNonQuery();
                Cout.WriteLine("{0} of row(s) affected", count);
            }
            catch (Exception ex)
            {
                Cerr.WriteLine(ex.Message);
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
                Cout.WriteLine("command del or erase: drop tables or delete data rows");
                Cout.WriteLine("del tablename               : drop table");
                Cout.WriteLine("del [sql where clause]      : delete current table filtered rows");
                Cout.WriteLine("example:");
                Cout.WriteLine(@"local> del Northwind\Products       : drop table [Products]");
                Cout.WriteLine(@"local\Northwind\Products> del       : delete all rows of table [Products]");
                Cout.WriteLine(@"local\Northwind\Products> del col1=1 and col2='match' : del rows matched on columns:c1 or c2");
                return;
            }

            var pt = mgr.current;
            if (!(pt.Item is Locator) && !(pt.Item is TableName))
            {
                TableName[] T = null;
                if (cmd.Arg1 != null)
                {
                    PathName path = new PathName(cmd.Arg1);
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
                                    Cerr.WriteLine("invalid path");
                                    return;
                                }
                            }
                        }
                        else
                        {
                            Cerr.WriteLine("database is unavailable");
                            return;
                        }
                    }
                    else
                    {
                        Cerr.WriteLine("invalid path");
                        return;
                    }
                }

                if (T != null && T.Length > 0)
                {
                    if (!Cin.YesOrNo($"are you sure to drop {T.Length} tables (y/n)?"))
                        return;

                    try
                    {
                        var sqlcmd = new SqlCmd(T[0].Provider, string.Empty);
                        sqlcmd.ExecuteNonQueryTransaction(T.Select(row => string.Format("DROP TABLE {0}", row)));
                        string text = string.Join<TableName>("\n", T);
                        Cerr.WriteLine($"completed to drop table(s):\n{text}");
                    }
                    catch (Exception ex)
                    {
                        Cerr.WriteLine(ex.Message);
                    }
                }
                else
                    Cerr.WriteLine("table is not selected");

                return;
            }


            TableName tname = null;
            Locator locator = null;
            if (pt.Item is Locator)
            {
                locator = mgr.GetCombinedLocator(pt);
                tname = mgr.GetCurrentPath<TableName>();
                if (!string.IsNullOrEmpty(cmd.Args))
                    locator.And(new Locator(cmd.Args));
            }

            if (pt.Item is TableName)
            {
                tname = (TableName)pt.Item;
                if (!string.IsNullOrEmpty(cmd.Args))
                    locator = new Locator(cmd.Args);
            }

            if (locator == null)
                Cout.Write("are you sure to delete all rows (y/n)?");
            else
                Cout.Write("are you sure to delete (y/n)?");

            if (Cin.ReadKey() != ConsoleKey.Y)
                return;

            Cout.WriteLine();

            try
            {
                int count;
                if (locator == null)
                    count = new SqlBuilder().DELETE(tname).SqlCmd.ExecuteNonQuery();
                else
                    count = new SqlBuilder().DELETE(tname).WHERE(locator).SqlCmd.ExecuteNonQuery();

                Cout.WriteLine("{0} of row(s) affected", count);
            }
            catch (Exception ex)
            {
                Cerr.WriteLine(ex.Message);
            }
        }


        public void mkdir(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("command md or mkdir");
                Cout.WriteLine("md [sql where clause]           : filter current table rows");
                Cout.WriteLine("options:");
                Cout.WriteLine("   /name:directory              : filter name");
                Cout.WriteLine("example:");
                Cout.WriteLine("md col1=1                       : filter rows matched on columns:c1");
                Cout.WriteLine("md \"col1=1 and col2='match'\"    : filter rows matched on columns:c1 or c2");
                Cout.WriteLine("md \"age > 60\" /name:senior      : filter rows matched age>60 and display as senior");
                return;
            }

            TreeNode<IDataPath> pt = mgr.current;

            if (!(pt.Item is TableName) && !(pt.Item is Locator))
            {
                Cerr.WriteLine("must add filter underneath table or locator");
                return;
            }

            if (string.IsNullOrEmpty(cmd.Args))
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
                Cout.WriteLine("command rd or rmdir");
                Cout.WriteLine("rm [filter name] : remove locators/filters");
                Cout.WriteLine("rm #1 : remove the locator node#");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            pt = pt.Parent;

            if (!(pt.Item is TableName))
            {
                Cerr.WriteLine("cannot remove filter underneath non-Table");
                return;
            }


            var nodes = pt.Nodes.Where(node => node.Item is Locator && (node.Item as Locator).Path == cmd.Path1.name).ToArray();
            if (nodes.Count() > 0)
            {
                if (!Cin.YesOrNo("are you sure to delete (y/n)?"))
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
                        if (!Cin.YesOrNo("are you sure to delete (y/n)?"))
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
                Cout.WriteLine("display current data, or search pattern");
                Cout.WriteLine("type [path]|[pattern]|[where]  : display current data, or search pattern");
                Cout.WriteLine("options:");
                Cout.WriteLine("   /top:n              : display top n records");
                Cout.WriteLine("   /all                : display all records");
                Cout.WriteLine("   /t                  : display table in vertical grid");
                Cout.WriteLine("   /r                  : display row-id");
                Cout.WriteLine("   /json               : generate json data");
                Cout.WriteLine("   /dup                : list duplicated rows, e.g. type /dup /col:c1,c2");
                Cout.WriteLine("   /col:c1,c2,..       : display columns, or search on columns");
                Cout.WriteLine("   /edit               : edit mode");
                Cout.WriteLine("example:");
                Cout.WriteLine("type match*s /col:c1,c2 : display rows matched on columns:c1 or c2");
                Cout.WriteLine("type id=20             : display rows where id=20");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            if (!mgr.TypeFile(pt, cmd))
                Cerr.WriteLine("invalid arguments");
        }




        public void copy(ApplicationCommand cmd, CompareSideType sideType)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("use \"/pk:table1=pk1+pk2,table=pk1\" if primary key doesn't exist");
                if (sideType == CompareSideType.copy)
                {
                    Cout.WriteLine("copy schema or records from table1 to table2, support table name wildcards");
                    Cout.WriteLine("copy table1 [table2] [/s]");
                }
                else if (sideType == CompareSideType.sync)
                {
                    Cout.WriteLine("synchronize schema or records from table1 to table2");
                    Cout.WriteLine("sync table1 [table2] [/s] : sync table1' records to table2");
                }
                else if (sideType == CompareSideType.compare)
                {
                    Cout.WriteLine("compare schema or records from table1 to table2");
                    Cout.WriteLine("comp table1 [table2] [/s] : sync table1' records to table2");
                }
                Cout.WriteLine("support table name wildcards");
                Cout.WriteLine("[/s]                       : table schema, default table records");
                return;
            }

            CancelableWork.CanCancel(cts =>
            {
                PathBothSide both = new PathBothSide(mgr, cmd);
                var dname2 = mgr.GetPathFrom<DatabaseName>(both.ps2.Node);
                if (dname2 == null)
                {
                    Cout.WriteLine("invalid destination path");
                    return;
                }

                if (both.ps1.MatchedTables == null || both.ps1.MatchedTables.Length == 0)
                {
                    Cout.WriteLine("no table found");
                    return;
                }

                foreach (var tname1 in both.ps1.MatchedTables)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    TableName tname2 = mgr.GetPathFrom<TableName>(both.ps2.Node);
                    if (tname2 == null)
                    {
                        tname2 = new TableName(dname2, tname1.SchemaName, tname1.Name);
                    }

                    var adapter = new CompareAdapter(cmd, both.ps1.side, both.ps2.side);
                    //stdio.WriteLine("start to {0} from {1} to {2}", sideType, tname1, tname2);
                    var sql = adapter.CompareTable(cmd.IsSchema ? ActionType.CompareSchema : ActionType.CompareData,
                        sideType, tname1, tname2, cmd.PK, cmd.Columns);

                    if (sideType == CompareSideType.compare)
                    {
                        if (sql == string.Empty)
                        {
                            Cout.WriteLine("source {0} and destination {1} are identical, or table is not found", tname1, tname2);
                        }
                        continue;
                    }

                    if (sql == string.Empty)
                    {
                        Cout.WriteLine("nothing changes made on destination {0}", tname2);
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
                                    Cout.WriteLine("{0} row(s) changed at destination {1}", count, tname2);
                                else
                                    Cout.WriteLine("command(s) completed successfully at destination {1}", count, tname2);
                            }
                            else
                                Cout.WriteLine("table {0} created at destination", tname2);
                        }
                        catch (Exception ex)
                        {
                            Cerr.WriteLine(ex.Message);
                            return;
                        }
                    }
                } // loop for

                return;
            });
        }

        public void compare(ApplicationCommand cmd, IApplicationConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("compare table schema or records");
                Cout.WriteLine("compare path1 [path2]  : compare data");
                Cout.WriteLine("compare [/s]           : compare schema");
                Cout.WriteLine("compare [/e]           : find common existing table names");
                Cout.WriteLine("compare [/count]       : compare number of rows");
                Cout.WriteLine("        [/pk]          : if primary key doesn't exist");
                Cout.WriteLine("                         for example /pk:table1=pk1+pk2,table=pk1");
                Cout.WriteLine();
                return;
            }

            PathBothSide both = new PathBothSide(mgr, cmd);
            string fileName = cmd.OutputFile(cfg.OutputFile);
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
            Cout.WriteLine($"result in \"{fileName}\"");
        }

        public void rename(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("rename column name, table name, modify column");
                Cout.WriteLine();
                Cout.WriteLine("ren [database] new-database        : rename database or current database to newdatabase");
                Cout.WriteLine("ren [table] new-table              : rename table current table to newtable");
                Cout.WriteLine("ren [table.]column table.newcolumn : rename column on current table to newcolumn");
                Cout.WriteLine();
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            string newName = cmd.Arg1 ?? cmd.Arg2;
            if (pt.Item is TableName && newName != null)
            {
                TableName tname = (TableName)pt.Item;
                string line = $"EXEC sp_rename '{tname}', '{newName}'";
                Cout.WriteLine(line);
                //cout.WriteLine($"completely to rename table name from {tname} to {newName}");
                return;
            }

            Cerr.WriteLine("invalid argument");
            return;
        }

        public void attrib(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("command attrib: update column property");
                Cout.WriteLine("add primary key, foreign key or identity key");
                Cout.WriteLine("columns:");
                Cout.WriteLine("  attrib [table] +c:col1=varchar(2)+null : add column or alter column");
                Cout.WriteLine("  attrib [table] +c:col1=varchar(10)     : add column or alter column");
                Cout.WriteLine("  attrib [table] -c:col1                 : remove column");
                Cout.WriteLine("primary keys:");
                Cout.WriteLine("  attrib [table] +p:col1,col2            : add primary key");
                Cout.WriteLine("  attrib [table] +p:col1,col2            : remove primary key");
                Cout.WriteLine("foreign keys:");
                Cout.WriteLine("  attrib [table] +f:col1=table2[.col2]   : add foreign key");
                Cout.WriteLine("  attrib [table] -f:col1                 : remove foreign key");
                Cout.WriteLine("identiy key:");
                Cout.WriteLine("  attrib [table] +i:col1                 : add identity");
                Cout.WriteLine("  attrib [table] -i:col1                 : remove identity");
                Cout.WriteLine("refine columns:");
                Cout.WriteLine("  attrib [table] /refine                 : refine column type and nullable");
                Cout.WriteLine("  attrib [table] /refine  /commit        : refine and save changes");
                Cout.WriteLine("  refine option:");
                Cout.WriteLine("    /not-null                            : change to NOT NULL");
                Cout.WriteLine("    /int                                 : convert to int");
                Cout.WriteLine("    /bit                                 : convert to bit");
                Cout.WriteLine("    /string                              : shrink string(NVARCHAR,VARCHAR,NCHAR,CHAR)");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;


            if (!(pt.Item is TableName))
            {
                Cerr.WriteLine("table is not selected");
                return;
            }


            if (cmd.Options.Has("+c"))
            {
                TableName tname = (TableName)pt.Item;
                string expr = cmd.Options.GetValue("+c");
                string[] items = expr.Split(new string[] { "=", "+" }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length != 2 && items.Length != 3)
                {
                    Cerr.WriteLine($"invalid expression:{expr}, correct is col1=type or col1=type+null");
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

            if (cmd.Options.Has("-c"))
            {
                TableName tname = (TableName)pt.Item;
                string column = cmd.Options.GetValue("-c");
                string SQL = $"ALTER TABLE [{tname.Name}] DROP COLUMN {column}";
                ExecuteNonQuery(tname.Provider, SQL);
                return;
            }

            if (cmd.Options.Has("+f"))
            {
                TableName fkName = (TableName)pt.Item;
                string expr = cmd.Options.GetValue("+f");
                string[] items = expr.Split('=');

                if (items.Length != 2)
                {
                    Cerr.WriteLine($"invalid foreign key expression:{expr}, correct is col1=pktable.col2");
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
                    Cerr.WriteLine($"invalid foreign key expression:{expr}, correct is col1=pktable.col2");
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
                    Cerr.WriteLine($"fails in generating foreign key constraint name, {ex.Message}");
                    return;
                }

                //check fkColumn, pkColumn is valid
                string SQL = $"ALTER TABLE [{fkName.Name}] ADD CONSTRAINT [{constraintName}] FOREIGN KEY([{fkColumn}]) REFERENCES [{pkName}]([{pkColumn}])";
                ExecuteNonQuery(fkName.Provider, SQL);
                return;
            }

            if (cmd.Options.Has("+p"))
            {
                TableName tname = (TableName)pt.Item;
                string expr = cmd.Options.GetValue("+p");
                string SQL = $"ALTER TABLE [{tname.Name}] ADD PRIMARY KEY(expr)";
                ExecuteNonQuery(tname.Provider, SQL);
                return;
            }

            if (cmd.Options.Has("+i"))
            {
                TableName tname = (TableName)pt.Item;
                string column = cmd.Options.GetValue("+i");
                string SQL = @"
ALTER TABLE {0} ADD {1} INT IDENTITY(1, 1)
ALTER TABLE {0} DROP COLUMN {2}
sp_rename '{1}', '{2}', 'COLUMN'";
                string.Format(SQL, tname.Name, $"_{column}_", column);
                ExecuteNonQuery(tname.Provider, SQL);
                return;
            }

            if (cmd.Has("refine"))
            {
                TableName tname = (TableName)pt.Item;
                TableSchemaRefinement refinement = new TableSchemaRefinement(tname);
                SchemaRefineOption option = new SchemaRefineOption
                {
                    ChangeNotNull = cmd.Has("not-null"),
                    ConvertInteger = cmd.Has("int"),
                    ConvertBoolean = cmd.Has("bit"),
                    ShrinkString = cmd.Has("string"),
                };

                string SQL = refinement.Refine(option);
                if (!string.IsNullOrEmpty(SQL))
                {
                    string fileName = cmd.OutputFile(cmd.Configuration.OutputFile);
                    using (var writer = fileName.CreateStreamWriter(cmd.Append))
                    {
                        writer.WriteLine(SQL);
                        Cout.WriteLine(SQL);
                        Cout.WriteLine($"table schema refinement for {tname.ShortName} at {fileName}");
                    }
                }
                else
                {
                    Cout.WriteLine($"no table schema refinement needed for {tname.ShortName}");
                }

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
                Cerr.WriteLine(ex.Message);
            }

            return -1;
        }

        public void let(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("let assignment              : variable assign statement ");
                Cout.WriteLine("let key=value               : update column by current table or locator");
                Cout.WriteLine("examples:");
                Cout.WriteLine("let Host=\"127.0.0.1\"      : value of variable Host is \"127.0.0.1\"");
                Cout.WriteLine("let a=12                    : value of variable a is 12");
                return;
            }

            if (string.IsNullOrEmpty(cmd.Args))
            {
                Cerr.WriteLine("assignment cannot be empty");
                return;
            }

            try
            {
                Script.Execute($"{cmd.Args};", Context.DS);
            }
            catch (Exception ex)
            {
                Cerr.WriteLine($"execute error: {ex.Message}");
            }
        }

        public void let1(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("let assignment              : update key-value table row, key-value table must be defined on the sqlcon.cfg or user.cfg");
                Cout.WriteLine("let key=value               : update column by current table or locator");
                Cout.WriteLine("example:");
                Cout.WriteLine("let Smtp.Host=\"127.0.0.1\" : update key-value row, it's equivalent to UPDATE table SET [Value]='\"127.0.0.1\"' WHERE [Key]='Smtp.Host'");
                return;
            }

            if (string.IsNullOrEmpty(cmd.Args))
            {
                Cerr.WriteLine("argument cannot be empty");
                return;
            }

            var pt = mgr.current;
            if (!(pt.Item is Locator) && !(pt.Item is TableName))
            {
                Cerr.WriteLine("table is not selected");
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
                Cerr.WriteLine("current table is not key-value tables");
                return;
            }

            string[] kvp = cmd.Args.Split('=');

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
                Cerr.WriteLine("invalid assignment");
                return;
            }

            Locator locator = new Locator(setting.KeyName.ColumnName() == key);
            SqlBuilder builder = new SqlBuilder().SELECT().COLUMNS(setting.ValueName.ColumnName()).FROM(tname).WHERE(locator);
            var L = new SqlCmd(builder).FillDataColumn<string>(0);
            if (L.Count() == 0)
            {
                Cerr.WriteLine($"undefined key: {key}");
                return;
            }

            if (kvp.Length == 1)
            {
                Cerr.WriteLine($"{key} = {L.First()}");
                return;
            }

            builder = new SqlBuilder()
                .UPDATE(tname)
                .SET(setting.ValueName.ColumnName() == value)
                .WHERE(locator);

            try
            {
                int count = builder.SqlCmd.ExecuteNonQuery();
                Cout.WriteLine("{0} of row(s) affected", count);
            }
            catch (Exception ex)
            {
                Cerr.WriteLine(ex.Message);
            }
        }

        public void clean(ApplicationCommand cmd, IApplicationConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("clean duplicated rows");
                Cout.WriteLine("clean [path]|[pattern]|  : clean current database or table, or search pattern");
                Cout.WriteLine("options:");
                Cout.WriteLine("   /col:c1,c2,..         : clean columns, compare column c1, c2, ...");
                Cout.WriteLine("   /d                    : commit cleaning duplicated rows on database server, otherwise display # of duplicated rows");
                Cout.WriteLine("example:");
                Cout.WriteLine("clean match*s /col:c1,c2 : clean duplicated rows by comparing columns:c1 and c2");
                Cout.WriteLine("clean                    : clean by comparing entire row");
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
                    Cout.WriteLine("completed to clean {0} #rows at {1}", count, tname);
                }
                else
                {
                    int count = dup.DuplicatedRowCount();
                    if (count == 0)
                        Cout.WriteLine("no duplicated rows at {0}", tname);
                    else
                        Cout.WriteLine("{0} duplicated row(s) at {1}", count, tname);
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
                            Cout.WriteLine("start to clean {0}", tn);
                            var dup = new DuplicatedTable(tn, cmd.Columns);
                            int count = dup.Clean();
                            Cout.WriteLine("cleaned {0} #rows", count);
                        }
                        else
                        {
                            Cout.WriteLine("start to query {0}", tn);
                            var dup = new DuplicatedTable(tn, cmd.Columns);
                            int count = dup.DuplicatedRowCount();
                            if (count == 0)
                                Cout.WriteLine("distinct rows");
                            else
                                Cout.WriteLine("{0} duplicated row(s)", count, tn);
                        }

                    }


                });

                return;
            }

            Cerr.WriteLine("select database or table first");
        }

        public void load(ApplicationCommand cmd, IApplicationConfiguration cfg, ShellContext context)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("load data file");
                Cout.WriteLine("option:");
                Cout.WriteLine("   /fmt:xml,ds   : load System.Data.DataSet xml file as last result");
                Cout.WriteLine("   /fmt:xml,dt   : load System.Data.DataTable xml file as last result");
                Cout.WriteLine("   /fmt:txt      : load text file and load into current table");
                Cout.WriteLine("   /fmt:csv      : load .csv data into current table");
                Cout.WriteLine("      [/col:c1,c2,...] csv columns mapping");
                Cout.WriteLine("   /fmt:cfg      : load .cfg data into current config table");
                Cout.WriteLine("      [/key:column] column of key on config table");
                Cout.WriteLine("      [/value:column] column of value config table");
                Cout.WriteLine("      [/col:c1=v1,c2=v2,...] default values for not null columns");
                Cout.WriteLine("e.g. load c:\\conf.cfg /fmt:cfg /key:Key /value:Value /col:[Inactive]=0");
                return;
            }

            string file = cmd.Arg1;
            if (file == null)
            {
                Cerr.WriteLine("file name not specified");
                return;
            }

            if (!File.Exists(file))
            {
                Cerr.WriteLine($"cannot find the file \"{file}\"");
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
                        Cout.WriteLine($"{typeof(DataSet).FullName} xml file \"{file}\" has been loaded");
                    }
                    catch (Exception ex)
                    {
                        Cerr.WriteLine($"invalid {typeof(DataSet).FullName} xml file, {ex.Message}");
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
                        Cerr.WriteLine($"invalid {typeof(DataTable).FullName} xml file, {ex.Message}");
                        return;
                    }
                    Cout.WriteLine($"{typeof(DataTable).FullName} xml file \"{file}\" has been loaded");
                    break;

                case "txt":
                    break;

                case "cfg":
                case "csv":
                    TableName tname = mgr.GetCurrentPath<TableName>();
                    if (tname == null)
                    {
                        Cerr.WriteLine("cannot find the table to load data");
                        return;
                    }

                    int count = 0;
                    var importer = new Loader(cmd);
                    if (fmt == "csv")
                        count = importer.LoadCsv(file, tname, cmd.Columns);
                    else if (fmt == "cfg")
                        count = importer.LoadCfg(file, tname);

                    Cout.WriteLine($"{count} row(s) loaded");
                    break;

                case "tie":
                    new TieClassBuilder(cmd).Done();
                    break;

                default:
                    Cerr.WriteLine("invalid command");
                    break;
            }
        }

        public void export(ApplicationCommand cmd, IApplicationConfiguration cfg, ShellContext context)
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
                Cerr.WriteLine("select server, database or table first");
        }

        public void import(ApplicationCommand cmd, IApplicationConfiguration cfg, ShellContext context)
        {
            if (cmd.HasHelp)
            {
                Importer.Help();
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            if (pt.Item is TableName || pt.Item is Locator || pt.Item is DatabaseName || pt.Item is ServerName)
            {
                var importer = new Importer(mgr, pt, cmd, cfg);
                importer.Run();
            }
            else
                Cerr.WriteLine("select server, database or table first");
        }

        public void mount(ApplicationCommand cmd, IConnectionConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("mount database server");
                Cout.WriteLine("mount alias=server_name   : alias must start with letter");
                Cout.WriteLine("options:");
                Cout.WriteLine("   /db:database           : initial catalog, default is 'master'");
                Cout.WriteLine("   /u:username            : user id, default is 'sa'");
                Cout.WriteLine("   /p:password            : password, default is empty, use Windows Security when /u /p not setup");
                Cout.WriteLine("   /pvd:provider          : default is SQL Server client");
                Cout.WriteLine("        sqldb               SQL Server, default provider");
                Cout.WriteLine("        sqloledb            ODBC Database Server");
                Cout.WriteLine("        sqlce               Sql Server Compact database");
                Cout.WriteLine("        sqlite              SQLite v3 database");
                Cout.WriteLine("        file/db/xml         sqlcon Database Schema, default provider for xml file");
                Cout.WriteLine("        file/dataset/json   System.Data.DataSet");
                Cout.WriteLine("        file/dataset/xml    System.Data.DataSet");
                Cout.WriteLine("        file/datalake/json  Dictionary<string, System.Data.DataSet>");
                Cout.WriteLine("        file/datalake/xml   Dictionary<string, System.Data.DataSet>");
                Cout.WriteLine("        file/assembly       .Net assembly dll");
                Cout.WriteLine("        file/c#             C# data contract classes");
                Cout.WriteLine("        riadb               Remote Invoke Agent");
                Cout.WriteLine("   /namespace:xxx           wildcard of namespace name filter on assembly");
                Cout.WriteLine("   /class:xxxx              wildcard of class name filter on assembly");
                Cout.WriteLine("example:");
                Cout.WriteLine("  mount ip100=192.168.0.100\\sqlexpress /u:sa /p:p@ss");
                Cout.WriteLine("  mount web=http://192.168.0.100/db/northwind.xml /u:sa /p:p@ss");
                Cout.WriteLine("  mount xml=file://c:\\db\\northwind.xml");
                Cout.WriteLine("  mount cs=file://c:\\db\\northwind.cs /pvd:file/c#");
                Cout.WriteLine("  mount dll=file://c:\\db\\any.dll /pvd:file/assembly /namespace:Sys* /class:Employee*");
                Cout.WriteLine("  mount sdf=c:\\db\\db1.sdf /pvd:sqlce");
                Cout.WriteLine("  mount db=c:\\db\\db2.db /pvd:sqlite");
                return;
            }

            if (cmd.Arg1 == null)
            {
                Cerr.WriteLine("invalid arguments");
                return;
            }

            var items = cmd.Arg1.Split('=');
            if (items.Length != 2)
            {
                Cerr.WriteLine("invalid arguments, correct format is alias=server_name");
                return;
            }
            string serverName = items[0].Trim();
            string dataSource = items[1].Trim();

            DbConnectionStringBuilder conn = new DbConnectionStringBuilder();
            string pvd = cmd.GetValue("pvd");
            if (pvd != null)
            {
                if (pvd != "sqloledb" && pvd != "xmlfile" && !pvd.StartsWith("file/") && pvd != "sqlce" && pvd != "sqlite")
                {
                    Cerr.WriteLine($"provider={pvd} is not supported");
                    return;
                }

                conn["provider"] = pvd;
            }
            else
            {
                if (dataSource.StartsWith("file://") || dataSource.StartsWith("http://") || dataSource.StartsWith("ftp://"))
                    conn["provider"] = "file/db/xml";   // it == xmlfile
            }


            conn["data source"] = dataSource;

            if (pvd == "sqlce")
            {
                conn["Max Buffer Size"] = 1024;
                conn["Persist Security Info"] = false;
            }
            else if (pvd == "sqlite")
            {
                conn["Version"] = 3;
                conn["DateTimeFormat"] = "Ticks";
                conn["Pooling"] = true;
                conn["Max Pool Size"] = 100;
            }
            else
            {
                string db = cmd.GetValue("db");
                if (db != null)
                    conn["initial catalog"] = db;
                else
                    conn["initial catalog"] = "master";

                string userId = cmd.GetValue("u");
                string password = cmd.GetValue("p");

                if (userId == null && password == null)
                {
                    conn["integrated security"] = "SSPI";
                    conn["packet size"] = 4096;
                }
                else
                {
                    if (userId != null)
                        conn["User Id"] = userId;
                    else
                        conn["User Id"] = "sa";

                    if (password != null)
                        conn["Password"] = password;
                    else
                        conn["Password"] = "";
                }
            }


            append("namespace");
            append("class");

            void append(string key)
            {
                string value = cmd.GetValue(key);
                if (value != null)
                    conn[key] = value;
            }

            string connectionString = conn.ConnectionString;

            ConnectionProvider provider = ConnectionProviderManager.Register(serverName, connectionString);
            if (!provider.CheckConnection())
            {
                Cerr.WriteLine("database is offline or wrong parameter");
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
                Cout.WriteLine("unmount database server");
                Cout.WriteLine("unmount alias             : alias must start with letter");
                Cout.WriteLine("example:");
                Cout.WriteLine("  umount ip100");
                return;
            }

            if (cmd.Arg1 == null)
            {
                Cerr.WriteLine("invalid arguments");
                return;
            }

            var items = cmd.Arg1.Split('=');
            string serverName = cmd.Arg1;

            var result = cfg.Providers.FirstOrDefault(row => row.ServerName.Path == serverName);
            if (result != null)
            {
                cfg.Providers.Remove(result);

                var node = mgr.RootNode.Nodes.FirstOrDefault(row => row.Item.Path == serverName);
                if (node != null)
                    mgr.RootNode.Nodes.Remove(node);

                Cout.WriteLine($"umount server \"{serverName}\" done");
            }
            else
            {
                Cerr.WriteLine($"server \"{serverName}\" doesn't exist");
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
                Cerr.WriteLine("select table first");
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
                Cerr.WriteLine($"{message} path not exist: {path}");
        }

        public void xcopy(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("xcopy large size records, support table/database name wildcards");
                Cout.WriteLine("   table must have same structure");
                Cout.WriteLine("xcopy database1 [database2]");
                Cout.WriteLine("xcopy table1 [table2]");
                Cout.WriteLine("       /col:c1[=d1],c2[=d2],...         copy selected columns (mapping)");
                Cout.WriteLine("       /s                               compare table schema");
                Cout.WriteLine("note that: to xcopy selected records of table, mkdir locator first, example:");
                Cout.WriteLine(@"  \local\NorthWind\Products> md ProductId<200 /name:p200");
                Cout.WriteLine(@"  \local\NorthWind\Products> xcopy p200 \local\db");
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
                        tname2 = new TableName(dname2, tname1.SchemaName, tname1.Name);

                    if (cmd.IsSchema)
                    {
                        string result = Compare.TableSchemaDifference(CompareSideType.compare, tname1, tname2);
                        if (!string.IsNullOrEmpty(result))
                        {
                            Cerr.WriteLine("destination table is not compatible or doesn't exist");
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
                        tableReader = new TableReader(tname1, locator);
                    }
                    else
                    {
                        tableReader = new TableReader(tname1);
                    }

                    long cnt = tableReader.Count;
                    int count = Tools.ForceLongToInteger(cnt);

                    Cout.Write($"copying {tname1.Name} ");
                    using (var progress = new ProgressBar { Count = count })
                    {
                        TableBulkCopy bulkCopy = new TableBulkCopy(tableReader);
                        try
                        {
                            bulkCopy.CopyTo(tname2, maps.ToArray(), cts, progress);
                        }
                        catch (Exception ex)
                        {
                            Cerr.WriteLine(ex.Message);
                        }

                        if (cts.IsCancellationRequested)
                            progress.Report(count);
                    }

                    if (!cts.IsCancellationRequested)
                        Cout.WriteLine($", Done on rows({cnt}).");
                }
            });
        }

        public void execute(ApplicationCommand cmd, IApplicationConfiguration cfg, Side theSide)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("execute sql script file");
                Cout.WriteLine("execute file (.sql)");
                Cout.WriteLine("options:");
                Cout.WriteLine("   /batch-size:count          : maximum number of statements in SQL bulk command");
                Cout.WriteLine("   /verbose                   : display details");
                Cout.WriteLine("examples:");
                Cout.WriteLine("  execute northwind.sql       : execute single sql script file");
                return;
            }

            string inputfile;
            if (cmd.Arg1 != null)
                inputfile = cfg.WorkingDirectory.GetFullPath(cmd.Arg1, ".sql");
            else
            {
                Cerr.WriteLine("input undefined");
                return;
            }

            int batchSize = cmd.GetInt32("batch-size", 1);
            bool verbose = cmd.Has("verbose");
            if (theSide.ExecuteScript(inputfile, batchSize, verbose))
                ErrorCode = CommandState.OK;
            else
                ErrorCode = CommandState.SQL_FAILS;
        }

        public void edit(ApplicationCommand cmd, IApplicationConfiguration cfg, IConnectionConfiguration connection, Side theSide)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("edit, view and execute sql script");
                Cout.WriteLine("edit                          : create new file and edit");
                Cout.WriteLine("edit [file]                   : edit file, it is read-only if file is hyperlink");
                Cout.WriteLine("options:");
                Cout.WriteLine("   /usr                       : FTP user name");
                Cout.WriteLine("   /pwd                       : FTP password");
                Cout.WriteLine("examples:");
                Cout.WriteLine("  edit c:\\db\\northwind.sql");
                Cout.WriteLine("  edit file://datconn/northwind.sql");
                Cout.WriteLine("  edit http://www.datconn.com/demos/northwind.sql");
                Cout.WriteLine("  edit ftp://www.datconn.com/demos/northwind.sql /usr:user /pwd:password");
                return;
            }

            FileLink fileLink = null;
            if (cmd.Arg1 != null)
            {
                string inputfile = cmd.Arg1;

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
                            Cerr.WriteLine($"file {fileLink} doesn't exist");
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
                    Cerr.WriteLine(ex.Message);
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
                Cerr.WriteLine(ex.Message);
                return;
            }
#else
			cerr.WriteLine("doesn't support editor");
#endif			
        }

        public void open(ApplicationCommand cmd, IApplicationConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("open file in the editor or open directory in the Explorer");
                Cout.WriteLine("open file or directory ");
                Cout.WriteLine("options:");
                Cout.WriteLine("   log              : open log file");
                Cout.WriteLine("   working          : open working directory");
                Cout.WriteLine("   last             : open GUI viewer to see the last data table retrieved");
                Cout.WriteLine("   output           : open output file");
                Cout.WriteLine("   config [/s]      : open user configure file, /s open system configurate");
                Cout.WriteLine("   dpo              : open table class output directory");
                Cout.WriteLine("   dc|dc1|dc2       : open data contract class output directory");
                Cout.WriteLine("   l2s              : open Linq to SQL class output directory");
                Cout.WriteLine("   release          : open release notes");
                Cout.WriteLine("   file-name.sqc    : open batch file");
                Cout.WriteLine("   file-name.sql    : open SQL script file");
                Cout.WriteLine("   file-name.sqt    : open Tie file");

                return;
            }

            string path;

            switch (cmd.Arg1)
            {
                case "output":
                    StdIO.OpenEditor(cfg.OutputFile);
                    break;

                case "log":
                    StdIO.OpenEditor(Context.GetValue<string>("log"));
                    break;

                case "config":
                    if (cmd.IsSchema)
                        StdIO.OpenEditor("sqlcon.cfg");
                    else
                        StdIO.OpenEditor(ConfigurationEnvironment.Path.Personal);
                    break;

                case "release":
                    StdIO.OpenEditor("ReleaseNotes.txt");
                    break;

                case "working":
                    path = cfg.WorkingDirectory.CurrentDirectory;
                    OpenDirectory(path, "working directory");
                    break;

                case "dpo":
                    path = cfg.GetValue<string>(ConfigKey._GENERATOR_DPO_PATH, $"{ConfigurationEnvironment.MyDocuments}\\DataModel\\Dpo");
                    OpenDirectory(path, "dpo class");
                    break;

                case "dc":
                case "dc1":
                case "dc2":
                    path = cfg.GetValue<string>(ConfigKey._GENERATOR_DC_PATH, $"{ConfigurationEnvironment.MyDocuments}\\DataModel\\DataContracts");
                    OpenDirectory(path, "data contract class");
                    break;

                case "l2s":
                    path = cfg.GetValue<string>(ConfigKey._GENERATOR_L2S_PATH, $"{ConfigurationEnvironment.MyDocuments}\\DataModel\\L2s");
                    OpenDirectory(path, "data Linq to SQL class");
                    break;

                case "last":
                    OpenEditor();
                    break;

                default:
                    if (open(cmd.Arg1))
                        return;

                    Cerr.WriteLine("invalid arguments");
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
                        StdIO.OpenEditor(_path);
                        return true;
                    }
                }

                return false;
            }
        }

        public void save(ApplicationCommand cmd, IApplicationConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("save [file]");
                Cout.WriteLine("options:");
                Cout.WriteLine("  /output       : copy sql script ouput to clipboard for Windows only");
                Cout.WriteLine("  /string       : ");
                Cout.WriteLine("example:");
                Cout.WriteLine("  save /output");
                return;
            }

            if (cmd.Has("output"))
            {
                if (!File.Exists(cfg.OutputFile))
                {
                    Cerr.WriteLine($"no output file found : {cfg.OutputFile}");
                    return;
                }
                using (var reader = new StreamReader(cfg.OutputFile))
                {
                    string data = reader.ReadToEnd();
#if WINDOWS					
                    System.Windows.Clipboard.SetText(data);
                    Cout.WriteLine("copied to clipboard");
#endif
                }
            }
            else if (cmd.Has("string"))
            {
                var pt = mgr.current;
                if (pt.Item is DatabaseName)
                {
                    string table_name = cmd.GetValue("table-name") ?? "Table";
                    string schema_name = cmd.GetValue("schema-name") ?? SchemaName.dbo;
                    string root = cmd.GetValue("directory") ?? ".";
                    DatabaseName dname = (DatabaseName)pt.Item;
                    TableName tname = new TableName(dname, schema_name, table_name);

                    StringDumper dumper = new StringDumper(tname);

                    string SqlFileName = cmd.OutputFile(cfg.OutputFile);
                    using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
                    {
                        dumper.Save(writer);
                    }

                    Cout.WriteLine($"SQL script generated on \"{SqlFileName}\"");
                    return;
                }
            }
            else
            {
                Cerr.WriteLine("invalid arguments");
            }
        }

        public void echo(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("Displays messages, or turns command-echoing on or off");
                Cout.WriteLine("  echo [on | off]");
                Cout.WriteLine("  echo [message]");
                Cout.WriteLine("Type echo without parameters to display the current echo setting.");
                return;
            }

            string text = cmd.Args;
            if (string.IsNullOrEmpty(text))
            {
                string status = "on";
                if (!Cout.echo)
                    status = "off";

                Cout.WriteLine($"echo is {status}");
                return;
            }

            switch (text)
            {
                case "on":
                    Cout.echo = true;
                    break;

                case "off":
                    Cout.echo = false;
                    break;

                default:
                    Cout.WriteLine(text);
                    break;
            }

            return;
        }


        public void check(ApplicationCommand cmd, Side theSide)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("check data correctness");
                Cout.WriteLine("check [path]                   : check data on current table");
                Cout.WriteLine("options:");
                Cout.WriteLine("   /syntax                       : check key-value pair syntax");
                Cout.WriteLine("   /key:c1                     : column name of key variable");
                Cout.WriteLine("   /value:c2                   : column name of value expression");
                Cout.WriteLine("examples:");
                Cout.WriteLine("  check  dbo.config /syntax /key:Key /value:Value");
                return;
            }

            if (!Navigate(cmd.Path1))
                return;

            if (!(pt.Item is TableName))
            {
                Cerr.WriteLine("table is not selected");
                return;
            }

            TableName tname = pt.Item as TableName;

            if (cmd.Has("syntax"))
            {
                string colKey = cmd.GetValue("key") ?? "Key";
                string colValue = cmd.GetValue("value") ?? "Value";

                SqlBuilder builder = new SqlBuilder().SELECT().COLUMNS(new string[] { colKey, colValue }).FROM(tname);
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
                        Cerr.WriteLine($"invalid key={kvp.Key}");
                    }

                    try
                    {
                        VAL val = Script.Evaluate(kvp.Value, DS);
                    }
                    catch (Exception ex)
                    {
                        Cerr.WriteLine($"invalid value={kvp.Value} on key={kvp.Key}, {ex.Message}");
                    }
                }

                Cout.WriteLine($"{L.Count()} items checking completed");
                return;
            }


            Cerr.WriteLine($"invalid command");
            return;
        }


        public void last(ApplicationCommand cmd, IApplicationConfiguration cfg)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("last command display, load or save last dataset");
                Cout.WriteLine("last [path]                :");
                Cout.WriteLine("options:");
                Cout.WriteLine("  /load                    : load C#, json or xml file to last dataset");
                Cout.WriteLine("  /save                    : save last dataset to sql, json or xml file");
                Cout.WriteLine("  /datalake                : format of json file is data lake");
                Cout.WriteLine("example:");
                Cout.WriteLine("  last                     : display last dataset");
                Cout.WriteLine("  last /pk:table1=pk1+pk2  : setup primary keys");
                Cout.WriteLine("  last products.cs         : display dataset file in c# format");
                Cout.WriteLine("  last products.xml        : display dataset file in xml format");
                Cout.WriteLine("  last products.json       : display dataset file in json format");
                Cout.WriteLine("  last lake.json  /datalake: display data lake file");
                Cout.WriteLine("  last products.xml /save  : save last dataset to a xml file");
                Cout.WriteLine("  last /save               : use table name as file name and save");
                Cout.WriteLine("  last products.json /save : save last dataset to a json file");
                Cout.WriteLine("  last products.sql /save  : save last dataset to a sql file");
                Cout.WriteLine("  last products.cs  /load  : load c# file to last dataset");
                Cout.WriteLine("  last products.xml /load  : load xml file to last dataset");
                Cout.WriteLine("  last products.json /load : load json file to last dataset");
                return;
            }

            IDictionary<string, string[]> parimaryKeys = cmd.PK;
            if (parimaryKeys.Count != 0)
            {
                DataSet ds = ShellHistory.LastDataSet();
                foreach (var kvp in parimaryKeys)
                {
                    string tableName = kvp.Key;
                    string[] pkKeys = kvp.Value;

                    DataTable dt = ds.Tables.OfType<DataTable>().FirstOrDefault(x => x.TableName == tableName);
                    if (dt != null)
                    {
                        try
                        {
                            var pkColumns = dt.Columns.OfType<DataColumn>().Where(x => pkKeys.Contains(x.ColumnName)).ToArray();
                            dt.PrimaryKey = pkColumns;
                            if (dt.PrimaryKey.Length != pkKeys.Length)
                            {
                                var L1 = pkKeys.Except(dt.Columns.OfType<DataColumn>().Select(x => x.ColumnName));
                                Cerr.WriteLine($"Cannot find columns last table: {L1.ToSimpleString()}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Cerr.WriteLine($"invalid primary keys: {pkKeys.ToSimpleString()}, {ex.Message}");
                        }
                    }
                    else
                    {
                        Cerr.WriteLine($"invalid table name: {tableName}");
                    }

                }
                return;
            }

            string file = cmd.Arg1;
            if (file == null && !cmd.Has("save"))
            {
                DataSet ds = ShellHistory.LastDataSet();
                if (ds != null)
                {
                    foreach (DataTable dt in ds.Tables)
                    {
                        Cout.WriteLine($"[{dt.TableName}]");
                        dt.ToConsole();
                    }
                }
                else
                    Cout.WriteLine("last result is not found");

                return;
            }


            if (cmd.Has("save"))
            {
                try
                {
                    var ds = ShellHistory.LastDataSet();
                    if (ds == null || ds.Tables.Count == 0)
                    {
                        Cerr.WriteLine("last result is null");
                        return;
                    }

                    if (file == null)
                        file = ds.Tables[0].TableName + ".xml";

                    file.WriteDataSet(ds);
                    Cout.WriteLine($"last result saved into {file}");
                }
                catch (Exception ex)
                {
                    Cerr.WriteLine(ex.Message);
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
                    Cout.WriteLine($"{typeof(DataSet).FullName} file \"{file}\" loaded");
                }
                catch (Exception ex)
                {
                    Cerr.WriteLine($"invalid data set file: {file}, {ex.Message}");
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
                            Cout.WriteLine($"\"{kvp.Key}\"");
                            DataSet ds = kvp.Value;
                            foreach (DataTable dt in ds.Tables)
                            {
                                Cout.WriteLine($"[{dt.TableName}]");
                                dt.ToConsole();
                            }

                            Cout.WriteLine();
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

                        Cout.WriteLine($"saved into \"{output}\"");
                    }
                }
                catch (Exception ex)
                {
                    Cerr.WriteLine($"invalid data lake file:{file}, {ex.Message}");
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
                    Cout.WriteLine($"[{dt.TableName}]");
                    dt.ToConsole();
                }
            }
            catch (Exception ex)
            {
                Cerr.WriteLine($"invalid data set file:{file}, {ex.Message}");
                return;
            }
            ShellHistory.SetLastResult(old);

            return;
        }

        public void find(ApplicationCommand cmd, string match)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("find command searches name of database, schema, table, view, or column");
                Cout.WriteLine("example:");
                Cout.WriteLine("  find *ID*           : search any string contains ID");
                Cout.WriteLine("  find *na?e          : search string ends with na?e");
                return;
            }

            if (match == null)
            {
                Cerr.WriteLine("find pattern is undefined");
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

        public bool call(ApplicationCommand cmd)
        {
            if (cmd.HasHelp)
            {
                Cout.WriteLine("call command script file");
                Cout.WriteLine("call [path]                :");
                Cout.WriteLine("options:");
                Cout.WriteLine("  /dump               : dump variables memory to output file");
                Cout.WriteLine("  /out                : define output file or directory");
                Cout.WriteLine("example:");
                Cout.WriteLine("  call script.sqt     : run script");
                Cout.WriteLine("  call script         : run script, default extension is .sqt");
                return true;
            }

            if (cmd.Arg1 == null)
            {
                Cerr.WriteLine($"missing file name");
                return true;
            }

            string path = cmd.Configuration.WorkingDirectory.GetFullPath(cmd.Arg1, ".sqt");
            if (!File.Exists(path))
            {
                Cerr.WriteLine($"cannot find the file: \"{path}\"");
                return true;
            }

            bool dump = cmd.Has("dump");
            try
            {
                Memory DS = Context.DS;
                if (dump)
                    DS = new Memory();

                string code = File.ReadAllText(path);
                Script.Execute(code, DS);

                if (dump)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (VAR var in DS.Names)
                    {
                        VAL val = DS[var];
                        try
                        {
                            builder.AppendLine($"{var} = {val.ToExJson()};").AppendLine();
                        }
                        catch (Exception ex)
                        {
                            builder.AppendLine($"error on the variable \"{var}\", {ex.AllMessages()}");
                        }
                    }

                    string _path = cmd.OutputFile("dump.txt", createDirectoryIfNotExists: false);
                    _path = cmd.Configuration.WorkingDirectory.GetFullPath(_path);
                    File.WriteAllText(_path, builder.ToString());
                    Cout.WriteLine($"Memory dumps to \"{_path}\"");
                }
            }
            catch (Exception ex)
            {
                Cerr.WriteLine($"execute error: {ex.Message}");
                return false;   //NextStep.ERROR;
            }

            return true; // NextStep.COMPLETED;
        }


    }
}


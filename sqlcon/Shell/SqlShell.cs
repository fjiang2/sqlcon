using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys.Data;
using Sys.Data.Comparison;
using Sys;
using Tie;

namespace sqlcon
{


    class SqlShell : ShellContext
    {
        static readonly string[] _SQL = new string[] { "use", "select", "update", "delete", "insert", "exec", "create", "alter", "drop" };
        public SqlShell(Configuration cfg)
            : base(cfg)
        {
        }

        /// <summary>
        /// read command line from console and run command
        /// </summary>
        public void DoConsole()
        {

            string line = null;

            L1:
            stdio.Write("{0}> ", mgr);
            L2:
            line = stdio.ReadLine();

            if (Console.IsOutputRedirected)
                Console.WriteLine(line);

            //ctrl-c captured
            if (line == null)
                return;

            switch (Run(line))
            {
                case NextStep.NEXT:
                case NextStep.COMPLETED:
                case NextStep.ERROR:
                    goto L1;

                case NextStep.CONTINUE:
                    goto L2;

                case NextStep.EXIT:
                    return;

            }
        }

        /// <summary>
        /// process command batch file
        /// </summary>
        /// <param name="lines"></param>
        public void DoBatch(IEnumerable<string> lines)
        {
            NextStep next = NextStep.NEXT;
            foreach (string line in lines)
            {
                if (next == NextStep.EXIT)
                    return;

                if (next == NextStep.COMPLETED || next == NextStep.NEXT)
                {
                    stdio.Write("{0}> ", mgr);
                }
                else if (next == NextStep.ERROR)
                {
                    if (!stdio.YesOrNo($"continue to run \"{line}\" (y/n)?"))
                    {
                        stdio.ErrorFormat("interupted.");
                        return;
                    }
                }

                stdio.WriteLine(line);
                next = Run(line);
            }
        }

        private bool multipleLineMode = false;
        private StringBuilder multipleLineBuilder = new StringBuilder();

        public NextStep Run(string line)
        {

            if (!multipleLineMode)
            {

                if (line == "exit")
                    return NextStep.EXIT;

                switch (line)
                {
                    case "help":
                    case "?":
                        Help();
                        multipleLineBuilder.Clear();
                        return NextStep.COMPLETED;

                    case "cls":
                        Console.Clear();
                        return NextStep.COMPLETED;

                    default:
                        {
                            var _result = TrySingleLineCommand(line);
                            if (_result == NextStep.COMPLETED)
                            {
                                stdio.WriteLine();
                                return NextStep.COMPLETED;
                            }
                            else if (_result == NextStep.ERROR)
                                return NextStep.ERROR;

                        }
                        break;
                }
            }

            if (line != "" && line != ";")
                multipleLineBuilder.AppendLine(line);

            if (line.EndsWith(";"))
            {
                string text = multipleLineBuilder.ToString().Trim();
                multipleLineBuilder.Clear();

                if (text.EndsWith(";"))
                    text = text.Substring(0, text.Length - 1);

                try
                {
                    var result = DoMultipleLineCommand(text);
                    multipleLineMode = false;
                    stdio.WriteLine();
                    return result;
                }
                catch (Exception ex)
                {
                    stdio.WriteLine(ex.Message);
                    return NextStep.ERROR;
                }
            }
            else if (multipleLineBuilder.ToString() != "")
            {
                multipleLineMode = true;
                stdio.Write("...");
                return NextStep.CONTINUE;
            }

            return NextStep.NEXT;
        }

        private NextStep TrySingleLineCommand(string text)
        {

#if DEBUG
            return DoSingleLineCommand(text);
#else
            try
            {
                return DoSingleLineCommand(text);
            }
            catch (SqlException ex1)
            {
                stdio.ErrorFormat("{0}:{1}", "SQL", ex1.Message);
            }
            catch (Exception ex2)
            {
                stdio.ErrorFormat(ex2.Message);
            }

            return NextStep.ERROR;
#endif
        }


        private NextStep DoSingleLineCommand(string text)
        {
            text = text.Trim();
            if (text == string.Empty)
                return NextStep.CONTINUE;


            Command cmd = new Command(text, cfg);
            if (cmd.badcommand)
                return NextStep.ERROR;

            switch (cmd.Action)
            {
                case "set":
                    commandee.set(cmd);
                    return NextStep.COMPLETED;

                case "let":
                    commandee.let(cmd);
                    return NextStep.COMPLETED;

                case "md":
                case "mkdir":
                    commandee.mkdir(cmd);
                    return NextStep.COMPLETED;

                case "rd":
                case "rmdir":
                    commandee.rmdir(cmd);
                    return NextStep.COMPLETED;
            }


            switch (cmd.Action)
            {
                case "ls":
                case "dir":
                    commandee.dir(cmd);
                    return NextStep.COMPLETED;

                case "cd":
                case "chdir":
                    if (cmd.arg1 != null || cmd.HasHelp)
                        chdir(cmd);
                    else
                        stdio.WriteLine(mgr.ToString());
                    return NextStep.COMPLETED;

                case "type":
                    commandee.type(cmd);
                    return NextStep.COMPLETED;

                case "del":
                case "erase":
                    commandee.del(cmd);
                    return NextStep.COMPLETED;

                case "ren":
                case "rename":
                    commandee.rename(cmd);
                    return NextStep.COMPLETED;

                case "attrib":
                    commandee.attrib(cmd);
                    return NextStep.COMPLETED;

                case "echo":
                    commandee.echo(cmd);
                    return NextStep.COMPLETED;

                case "rem":
                    return NextStep.COMPLETED;

                case "ver":
                    stdio.WriteLine("sqlcon [Version {0}]", SysExtension.ApplicationVerison);
                    return NextStep.COMPLETED;

                case "show":
                    if (cmd.arg1 != null)
                        Show(cmd.arg1.ToLower(), cmd.arg2);
                    else
                        stdio.ErrorFormat("invalid argument");
                    return NextStep.COMPLETED;

                case "find":
                    if (cmd.arg1 != null)
                        theSide.FindName(cmd.arg1);
                    else
                        stdio.ErrorFormat("find object undefined");
                    return NextStep.COMPLETED;

                case "save":
                    commandee.save(cmd, cfg);
                    return NextStep.COMPLETED;

                case "execute":
                    commandee.execute(cmd, theSide);
                    if (commandee.ErrorCode == CommandState.OK)
                        return NextStep.COMPLETED;
                    else
                        return NextStep.ERROR;

                case "open":
                    commandee.open(cmd, cfg);
                    return NextStep.COMPLETED;

                case "compare":
                    {
                        PathBothSide both = new PathBothSide(mgr, cmd);

                        using (var writer = cfg.OutputFile.NewStreamWriter())
                        {
                            ActionType type;
                            if (cmd.IsSchema)
                                type = ActionType.CompareSchema;
                            else
                                type = ActionType.CompareData;

                            if (both.Invalid)
                            {
                                return NextStep.COMPLETED;
                            }

                            var adapter = new CompareAdapter(both.ps1.side, both.ps2.side);
                            var sql = adapter.Run(type, both.ps1.MatchedTables, both.ps2.MatchedTables, cfg, cmd.Columns);
                            writer.Write(sql);
                        }
                        stdio.WriteLine("completed");
                        return NextStep.COMPLETED;
                    }

                case "copy":
                    commandee.copy(cmd, CompareSideType.copy);
                    return NextStep.COMPLETED;

                case "sync":
                    commandee.copy(cmd, CompareSideType.sync);
                    return NextStep.COMPLETED;

                case "comp":
                    commandee.copy(cmd, CompareSideType.compare);
                    return NextStep.COMPLETED;

                case "xcopy":
                    commandee.xcopy(cmd);
                    return NextStep.COMPLETED;

                case "lcd":
                    if (cmd.arg1 != null)
                        cfg.WorkingDirectory.ChangeDirectory(cmd.arg1);
                    else
                        stdio.WriteLine(cfg.WorkingDirectory.CurrentDirectory);
                    return NextStep.COMPLETED;

                case "ldir":
                    cfg.WorkingDirectory.ShowCurrentDirectory(cmd.arg1);
                    return NextStep.COMPLETED;

                case "run":
                    if (cmd.arg1 != null)
                    {
                        new Batch(cfg, cmd.arg1).Call(cmd.arguments);
                    }
                    return NextStep.COMPLETED;

                case "call":
                    if (cmd.arg1 != null)
                    {
                        string path = cfg.WorkingDirectory.GetFullPath(cmd.arg1, ".sqt");
                        if (!System.IO.File.Exists(path))
                        {
                            stdio.Error($"cannot find the file: {path}");
                        }
                        else
                        {
                            try
                            {
                                string code = System.IO.File.ReadAllText(path);
                                Script.Execute(code, Context.DS);
                            }
                            catch (Exception ex)
                            {
                                stdio.ErrorFormat("execute error: {0}", ex.Message);
                                return NextStep.ERROR;
                            }
                        }
                    }
                    return NextStep.COMPLETED;

                case "export":
                    commandee.export(cmd, cfg, this);
                    return NextStep.COMPLETED;

                case "import":
                    commandee.import(cmd, cfg, this);
                    return NextStep.COMPLETED;

                case "clean":
                    commandee.clean(cmd, cfg);
                    return NextStep.COMPLETED;

                case "mount":
                    commandee.mount(cmd, cfg);
                    return NextStep.COMPLETED;

                case "umount":
                    commandee.umount(cmd, cfg);
                    return NextStep.COMPLETED;

                case "edit":
                    commandee.edit(cmd, theSide);
                    return NextStep.COMPLETED;

                case "last":
                    {
                        DataTable dt = ShellHistory.LastTable();
                        if (dt != null)
                            dt.ToConsole();
                        else
                            stdio.WriteLine("last result not found");
                    }
                    return NextStep.COMPLETED;

                default:
                    if (!_SQL.Contains(cmd.Action.ToLower()))
                    {
                        stdio.Error("invalid command");
                        return NextStep.COMPLETED;
                    }
                    break;
            }

            return NextStep.NEXT;
        }




        private void chdir(Command cmd)
        {
            if (commandee.chdir(cmd))
            {
                var dname = mgr.GetCurrentPath<DatabaseName>();
                if (dname != null)
                {
                    if (theSide == null)
                        theSide = new Side(dname.Provider);
                    else
                        theSide.UpdateDatabase(dname.Provider);
                }
                else
                {
                    var sname = mgr.GetCurrentPath<ServerName>();
                    if (sname != null)
                    {
                        if (theSide == null)
                            theSide = new Side(dname.Provider);
                        else
                            theSide.UpdateDatabase(sname.Provider);
                    }
                }
            }
        }

        private static string showConnection(ConnectionProvider cs)
        {
            return string.Format("S={0} db={1} U={2} P={3}", cs.DataSource, cs.InitialCatalog, cs.UserId, cs.Password);
        }

        private NextStep DoMultipleLineCommand(string text)
        {
            text = text.Trim();
            if (text == string.Empty)
                return NextStep.NEXT;

            string[] A = text.Split(' ', '\r');
            string cmd = null;
            string arg1 = null;
            string arg2 = null;

            int n = A.Length;

            if (n > 0)
                cmd = A[0].ToLower();

            if (n > 1)
                arg1 = A[1].Trim();

            if (n > 2)
                arg2 = A[2].Trim();

            switch (cmd)
            {
                case "use":
                case "select":
                    if (!Context.GetValue<bool>(Context.DATAREADER))
                    {
                        DataSet ds = new SqlCmd(theSide.Provider, text).FillDataSet();
                        if (ds != null)
                        {
                            foreach (DataTable dt in ds.Tables)
                                dt.ToConsole();
                        }
                    }
                    else
                    {
                        new SqlCmd(theSide.Provider, text).Read(reader => reader.ToConsole(Context.GetValue<int>(Context.MAXROWS, 100)));
                    }
                    break;

                case "update":
                case "delete":
                case "insert":
                case "exec":
                case "create":
                case "alter":
                case "drop":
                    try
                    {
                        int count = new SqlCmd(theSide.Provider, text).ExecuteNonQuery();
                        if (count > 0)
                            stdio.WriteLine("{0} of row(s) affected", count);
                        else if (count == 0)
                            stdio.WriteLine("nothing affected");
                        else
                            stdio.WriteLine("command(s) completed successfully");
                    }
                    catch (Exception ex)
                    {
                        stdio.ErrorFormat(ex.Message);
                        return NextStep.ERROR;
                    }
                    break;

                default:
                    stdio.ErrorFormat("invalid command");
                    return NextStep.ERROR;
            }

            return NextStep.COMPLETED;
        }

        private void Show(string arg1, string arg2)
        {
            var dname = theSide.DatabaseName;
            TableName[] vnames;

            switch (arg1)
            {
                case "pk":
                    {
                        var PKS = dname.TableWithPrimaryKey();
                        int count = 0;
                        foreach (var tname in PKS)
                        {
                            count++;
                            stdio.WriteLine("{0,5} {1}", $"[{count}]", tname);
                        }
                        stdio.WriteLine("total <{0}> tables with primary keys", count);
                    }
                    break;

                case "npk":
                    {
                        var tnames = dname.GetTableNames();
                        var PKS = dname.TableWithPrimaryKey();
                        int count = 0;
                        foreach (var tname in tnames)
                        {
                            if (PKS.FirstOrDefault(row => row.Equals(tname)) == null)
                            {
                                count++;
                                stdio.WriteLine("{0,5} {1}", $"[{count}]", tname);
                            }
                        }
                        stdio.WriteLine("total <{0}> tables without primary keys", count);
                    }
                    break;

                case "vw":
                    vnames = new MatchedDatabase(dname, arg2, null).DefaultViewNames;
                    foreach (var vname in vnames)
                    {
                        DataTable dt = null;
                        dt = vname.ViewSchema();
                        if (dt.Rows.Count > 0)
                        {
                            stdio.WriteLine("<{0}>", vname.ShortName);
                            dt.ToConsole();
                        }
                        else
                            stdio.WriteLine("not found at <{0}>", vname.ShortName);
                    }
                    break;

                case "view":
                    vnames = new MatchedDatabase(dname, arg2, null).DefaultViewNames;
                    vnames.Select(tname => new { Schema = tname.SchemaName, View = tname.Name })
                        .ToConsole();
                    break;

                case "proc":
                    dname.AllProc().ToConsole();
                    break;

                case "index":
                    dname.AllIndices().ToConsole();
                    break;

                case "connection":
                    {
                        var list = cfg.GetValue("servers");
                        if (list.Defined)
                        {
                            list.Select(kvp => new { Alias = (string)kvp[0].HostValue, Connection = kvp[1].ToString() })
                            .ToConsole();
                        }
                        else
                            stdio.ErrorFormat("connection string not found");
                    }
                    break;

                case "current":
                    stdio.WriteLine("current: {0}({1})", theSide.Provider.Name, showConnection(theSide.Provider));
                    break;

                case "var":
                    Context.ToConsole();
                    break;
                default:
                    stdio.ErrorFormat("invalid argument");
                    break;
            }
        }


        private static void Help()
        {
            stdio.WriteLine("Path points to server, database,tables, data rows");
            stdio.WriteLine(@"      \server\database\table\filter\filter\....");
            stdio.WriteLine("Notes: table names support wildcard matching, e.g. Prod*,Pro?ucts");
            stdio.WriteLine("exit                    : quit application");
            stdio.WriteLine("help                    : this help");
            stdio.WriteLine("?                       : this help");
            stdio.WriteLine("cls                     : clears the screen");
            stdio.WriteLine("dir,ls /?               : see more info");
            stdio.WriteLine("cd,chdir /?             : see more info");
            stdio.WriteLine("lcd [path]              : change or display current directory");
            stdio.WriteLine("ldir [path]             : display files");
            stdio.WriteLine("md,mkdir /?             : see more info");
            stdio.WriteLine("rd,rmdir /?             : see more info");
            stdio.WriteLine("type /?                 : see more info");
            stdio.WriteLine("set /?                  : see more info");
            stdio.WriteLine("del,erase /?            : see more info");
            stdio.WriteLine("ren,rename /?           : see more info");
            stdio.WriteLine("attrib /?               : see more info");
            stdio.WriteLine("copy /?                 : see more info");
            stdio.WriteLine("comp /?                 : see more info");
            stdio.WriteLine("xcopy /?                : see more info");
            stdio.WriteLine("echo /?                 : see more info");
            stdio.WriteLine("run [drive:][path]file  : run a batch program (.sqc)");
            stdio.WriteLine("call [drive:][path]file : call Tie program (.sqt)");
            stdio.WriteLine("rem                     : records comments/remarks");
            stdio.WriteLine("ver                     : display version");
            stdio.WriteLine("compare path1 [path2]   : compare table scheam or data");
            stdio.WriteLine("          /s            : compare schema otherwise compare data");
            stdio.WriteLine("          /col:c1,c2    : skip columns defined during comparing");
            stdio.WriteLine("export /?               : see more info");
            stdio.WriteLine("import /?               : see more info");
            stdio.WriteLine("clean /?                : see more info");
            stdio.WriteLine("mount /?                : see more info");
            stdio.WriteLine("umount /?               : see more info");
            stdio.WriteLine("open /?                 : see more info");
            stdio.WriteLine("save /?                 : see more info");
            stdio.WriteLine("execute /?              : see more info");
            stdio.WriteLine("edit /?                 : see more info");
            stdio.WriteLine("last                    : display last result");
            stdio.WriteLine();
            stdio.WriteLine("<Commands>");
            stdio.WriteLine("<find> pattern          : find table name or column name");
            stdio.WriteLine("<show view>             : show all views");
            stdio.WriteLine("<show proc>             : show all stored proc and func");
            stdio.WriteLine("<show index>            : show all indices");
            stdio.WriteLine("<show vw> viewnames     : show view structure");
            stdio.WriteLine("<show pk>               : show all tables with primary keys");
            stdio.WriteLine("<show npk>              : show all tables without primary keys");
            stdio.WriteLine("<show connection>       : show connection-string list");
            stdio.WriteLine("<show current>          : show current active connection-string");
            stdio.WriteLine("<show var>              : show variable list");
            stdio.WriteLine("<sync table1 table2>    : synchronize, make table2 is the same as table1");
            stdio.WriteLine();
            stdio.WriteLine("type [;] to execute following SQL script or functions");
            stdio.WriteLine("<SQL>");
            stdio.WriteLine("select ... from table where ...");
            stdio.WriteLine("update table set ... where ...");
            stdio.WriteLine("delete from table where...");
            stdio.WriteLine("create table ...");
            stdio.WriteLine("drop table ...");
            stdio.WriteLine("alter ...");
            stdio.WriteLine("exec ...");
            stdio.WriteLine("<Variables>");
            stdio.WriteLine("  maxrows               : max number of row shown on select query");
            stdio.WriteLine("  DataReader            : true: use SqlDataReader; false: use Fill DataSet");
            stdio.WriteLine();
            stdio.WriteLine("evalute expression or execuate statement if none of above ");
            stdio.WriteLine("10+3; results 13");
            stdio.WriteLine();
        }


    }
}

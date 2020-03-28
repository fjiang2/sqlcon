using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Sys;
using Sys.Data;
using Sys.Data.Comparison;
using Tie;
using Sys.Stdio;

namespace sqlcon
{
    partial class Shell : ShellContext, IShell
    {

        public Shell(IConfiguration cfg)
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
            cout.Write($"{mgr}> ");
        L2:
            line = cin.ReadLine();

            if (Console.IsOutputRedirected)
                Console.WriteLine(line);

            //ctrl-c captured
            if (line == null)
                goto L1;

            if (FlowControl.IsFlowStatement(line))
            {
                cerr.WriteLine($"use \"{line}\" on batch script file only");
                goto L1;
            }

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
        public void DoBatch(string[] lines)
        {
            FlowControl flow = new FlowControl(lines);
            NextStep next = flow.Execute(Run);
            if (next == NextStep.EXIT)
                cout.WriteLine(ConsoleColor.Green, "completed.");

            cout.Write($"{mgr}> ");
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
                                cout.WriteLine();
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
                    multipleLineMode = false;
                    var result = DoMultipleLineCommand(text);
                    cout.WriteLine();
                    return result;
                }
                catch (System.Data.SqlClient.SqlException ex1)
                {
                    cerr.WriteLine($"SQL:{ex1.AllMessages()}");
                }
                catch (Exception ex)
                {
                    cout.WriteLine(ex.Message);
                    return NextStep.ERROR;
                }

            }
            else if (multipleLineBuilder.ToString() != "")
            {
                multipleLineMode = true;
                cout.Write("...");
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
            catch (System.Data.SqlClient.SqlException ex1)
            {
                cerr.WriteLine($"SQL:{ex1.AllMessages()}");
            }
            catch (Exception ex2)
            {
                cerr.WriteLine(ex2.Message);
            }

            return NextStep.ERROR;
#endif
        }


        private NextStep DoSingleLineCommand(string line)
        {
            line = line.Trim();
            if (line == string.Empty)
                return NextStep.CONTINUE;

            ApplicationCommand cmd = new ApplicationCommand(cfg, line);
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
                        cout.WriteLine(mgr.ToString());
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
                    cout.WriteLine("sqlcon [Version {0}]", SysExtension.ApplicationVerison);
                    return NextStep.COMPLETED;

                case "show":
                    if (cmd.arg1 != null)
                        Show(cmd.arg1.ToLower(), cmd.arg2);
                    else
                        cerr.WriteLine("invalid argument");
                    return NextStep.COMPLETED;

                case "find":
                    commandee.find(cmd, cmd.arg1);
                    return NextStep.COMPLETED;

                case "save":
                    commandee.save(cmd, cfg);
                    return NextStep.COMPLETED;

                case "execute":
                    commandee.execute(cmd, cfg, theSide);
                    if (commandee.ErrorCode == CommandState.OK)
                        return NextStep.COMPLETED;
                    else
                        return NextStep.ERROR;

                case "open":
                    commandee.open(cmd, cfg);
                    return NextStep.COMPLETED;

                case "compare":
                    {
                        commandee.compare(cmd, cfg);
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
                        cout.WriteLine(cfg.WorkingDirectory.CurrentDirectory);
                    return NextStep.COMPLETED;

                case "ldir":
                    cfg.WorkingDirectory.ShowCurrentDirectory(cmd.arg1);
                    return NextStep.COMPLETED;

                case "ltype":
                    if (cmd.arg1 != null)
                    {
                        string[] lines = cfg.WorkingDirectory.ReadAllLines(cmd.arg1);
                        if (lines != null)
                        {
                            foreach (var _line in lines)
                                cout.WriteLine(_line);
                        }
                    }
                    else
                        cout.WriteLine("invalid arguments");
                    return NextStep.COMPLETED;

                case "run":
                    if (cmd.arg1 != null)
                    {
                        new Batch(cfg, cmd.arg1).Call(this, cmd.arguments);
                    }
                    return NextStep.COMPLETED;

                case "call":
                    if (cmd.arg1 != null)
                    {
                        string path = cfg.WorkingDirectory.GetFullPath(cmd.arg1, ".sqt");
                        if (!File.Exists(path))
                        {
                            cerr.WriteLine($"cannot find the file: {path}");
                        }
                        else
                        {
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

                                    string _path = cmd.OutputFile("dump.txt");
                                    _path = cfg.WorkingDirectory.GetFullPath(_path);
                                    File.WriteAllText(_path, builder.ToString());
                                    cout.WriteLine($"Memory dumps to \"{_path}\"");
                                }
                            }
                            catch (Exception ex)
                            {
                                cerr.WriteLine($"execute error: {ex.Message}");
                                return NextStep.ERROR;
                            }
                        }
                    }
                    else
                    {
                        cerr.WriteLine($"missing file name");
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
                    commandee.mount(cmd, connection);
                    return NextStep.COMPLETED;

                case "umount":
                    commandee.umount(cmd, connection);
                    return NextStep.COMPLETED;

                case "edit":
                    commandee.edit(cmd, cfg, connection, theSide);
                    return NextStep.COMPLETED;

                case "last":
                    commandee.last(cmd, cfg);
                    return NextStep.COMPLETED;

                case "chk":
                case "check":
                    commandee.check(cmd, theSide);
                    return NextStep.COMPLETED;

                default:
                    if (!_SQL.Contains(cmd.Action.ToUpper()))
                    {
                        cerr.WriteLine("invalid command");
                        return NextStep.COMPLETED;
                    }
                    break;
            }

            return NextStep.NEXT;
        }

        static readonly string[] _SQL = new string[] { "ALTER", "CREATE", "DELETE", "DROP", "EXEC", "INSERT", "SELECT", "UPDATE", "USE" };


        private void chdir(ApplicationCommand cmd)
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
                            cout.WriteLine("{0} of row(s) affected", count);
                        else if (count == 0)
                            cout.WriteLine("nothing affected");
                        else
                            cout.WriteLine("command(s) completed successfully");
                    }
                    catch (Exception ex)
                    {
                        cerr.WriteLine(ex.Message);
                        return NextStep.ERROR;
                    }
                    break;

                default:
                    cerr.WriteLine("invalid command");
                    break;
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
                            cout.WriteLine("{0,5} {1}", $"[{count}]", tname);
                        }
                        cout.WriteLine("total <{0}> tables with primary keys", count);
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
                                cout.WriteLine("{0,5} {1}", $"[{count}]", tname);
                            }
                        }
                        cout.WriteLine("total <{0}> tables without primary keys", count);
                    }
                    break;

                case "vw":
                    vnames = new MatchedDatabase(dname, arg2).ViewNames();
                    foreach (var vname in vnames)
                    {
                        DataTable dt = null;
                        dt = vname.ViewSchema();
                        if (dt.Rows.Count > 0)
                        {
                            cout.WriteLine("<{0}>", vname.ShortName);
                            dt.ToConsole();
                        }
                        else
                            cout.WriteLine("not found at <{0}>", vname.ShortName);
                    }
                    break;

                case "view":
                    vnames = new MatchedDatabase(dname, arg2).ViewNames();
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
                        var L = connection.Providers.OrderBy(x => x.ServerName.Path);
                        if (L.Count() > 0)
                        {
                            L.Select(pvd => new { Alias = pvd.ServerName.Path, Connection = pvd.ToSimpleString() })
                            .ToConsole();
                        }
                        else
                            cerr.WriteLine("connection string not found");
                    }
                    break;

                case "current":
                    cout.WriteLine("current: {0}({1})", theSide.Provider.Name, showConnection(theSide.Provider));
                    break;

                case "var":
                    {
                        ((VAL)Context.DS)
                            .Where(row => row[1].VALTYPE != VALTYPE.nullcon && row[1].VALTYPE != VALTYPE.voidcon && !row[0].Str.StartsWith("$"))
                            .Select(row => new { Variable = (string)row[0], Value = row[1] })
                            .ToConsole();
                    }
                    break;
                default:
                    cerr.WriteLine("invalid argument");
                    break;
            }
        }



    }
}

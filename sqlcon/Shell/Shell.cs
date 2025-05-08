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

namespace SqlCon
{
    partial class Shell : ShellContext, IShell
    {

        public Shell(IApplicationConfiguration cfg)
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
            Cout.Write($"{mgr}> ");
        L2:
            line = Cin.ReadLine();

            if (Console.IsOutputRedirected)
                Console.WriteLine(line);

            //ctrl-c captured
            if (line == null)
                goto L1;

            if (FlowControl.IsFlowStatement(line))
            {
                Cerr.WriteLine($"use \"{line}\" on batch script file only");
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
                Cout.WriteLine(ConsoleColor.Green, "completed.");

            Cout.Write($"{mgr}> ");
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
                                Cout.WriteLine();
                                return NextStep.COMPLETED;
                            }
                            else if (_result == NextStep.ERROR)
                                return NextStep.ERROR;

                        }
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(line) && line != ";")
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
                    Cout.WriteLine();
                    return result;
                }
                catch (System.Data.SqlClient.SqlException ex1)
                {
                    Cerr.WriteLine($"SQL:{ex1.AllMessages()}");
                }
                catch (Exception ex)
                {
                    Cout.WriteLine(ex.Message);
                    return NextStep.ERROR;
                }

            }
            else if (multipleLineBuilder.ToString() != "")
            {
                multipleLineMode = true;
                Cout.Write("...");
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
                Cerr.WriteLine($"SQL:{ex1.AllMessages()}");
            }
            catch (Exception ex2)
            {
                Cerr.WriteLine(ex2.Message);
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
            if (cmd.Badcommand)
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
                    if (cmd.Arg1 != null || cmd.HasHelp)
                        chdir(cmd);
                    else
                        Cout.WriteLine(mgr.ToString());
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
                    Cout.WriteLine("sqlcon [Version {0}]", Helper.ApplicationVerison);
                    return NextStep.COMPLETED;

                case "show":
                    if (cmd.Arg1 != null)
                        Show(cmd.Arg1.ToLower(), cmd.Arg2);
                    else
                        Cerr.WriteLine("invalid argument");
                    return NextStep.COMPLETED;

                case "find":
                    commandee.find(cmd, cmd.Arg1);
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
                    if (cmd.Arg1 != null)
                        cfg.WorkingDirectory.ChangeDirectory(cmd.Arg1);
                    else
                        Cout.WriteLine(cfg.WorkingDirectory.CurrentDirectory);
                    return NextStep.COMPLETED;

                case "ldir":
                    cfg.WorkingDirectory.ShowCurrentDirectory(cmd.Arg1);
                    return NextStep.COMPLETED;

                case "ltype":
                    if (cmd.Arg1 != null)
                    {
                        string[] lines = cfg.WorkingDirectory.ReadAllLines(cmd.Arg1);
                        if (lines != null)
                        {
                            foreach (var _line in lines)
                                Cout.WriteLine(_line);
                        }
                    }
                    else
                        Cout.WriteLine("invalid arguments");
                    return NextStep.COMPLETED;

                case "path":
                    if (cmd.Arg1 == null)
                    {
                        Cout.WriteLine(cfg.Path);
                    }
                    else
                    {
                        Context.SetValue("path", cmd.Arg1);
                    }
                    return NextStep.COMPLETED;

                case "run":
                    if (cmd.Arg1 != null)
                    {
                        new Batch(cfg, cmd.Arg1).Call(this, cmd.Arguments);
                    }
                    return NextStep.COMPLETED;

                case "call":
                    if (!commandee.call(cmd))
                        return NextStep.ERROR;
                    else
                        return NextStep.COMPLETED;

                case "import":
                    commandee.import(cmd, cfg, this);
                    return NextStep.COMPLETED;

                case "export":
                    commandee.export(cmd, cfg, this);
                    return NextStep.COMPLETED;

                case "load":
                    commandee.load(cmd, cfg, this);
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
                        Cerr.WriteLine("invalid command");
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
                        theSide = new Side(dname);
                    else
                        theSide.UpdateDatabase(dname);
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
                        new SqlCmd(theSide.Provider, text).Read(reader => reader.ToConsole(cfg.MaxRows));
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
                            Cout.WriteLine("{0} of row(s) affected", count);
                        else if (count == 0)
                            Cout.WriteLine("nothing affected");
                        else
                            Cout.WriteLine("command(s) completed successfully");
                    }
                    catch (Exception ex)
                    {
                        Cerr.WriteLine(ex.Message);
                        return NextStep.ERROR;
                    }
                    break;

                default:
                    Cerr.WriteLine("invalid command");
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
                            Cout.WriteLine("{0,5} {1}", $"[{count}]", tname);
                        }
                        Cout.WriteLine("total <{0}> tables with primary keys", count);
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
                                Cout.WriteLine("{0,5} {1}", $"[{count}]", tname);
                            }
                        }
                        Cout.WriteLine("total <{0}> tables without primary keys", count);
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
                            Cout.WriteLine("<{0}>", vname.ShortName);
                            dt.ToConsole();
                        }
                        else
                            Cout.WriteLine("not found at <{0}>", vname.ShortName);
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
                            Cerr.WriteLine("connection string not found");
                    }
                    break;

                case "current":
                    Cout.WriteLine("current: {0}({1})", theSide.Provider.Name, showConnection(theSide.Provider));
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
                    Cerr.WriteLine("invalid argument");
                    break;
            }
        }



    }
}

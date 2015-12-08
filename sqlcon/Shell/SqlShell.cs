using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data;
using Sys.Data.Comparison;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.IO;
using Sys;
using Tie;

namespace sqlcon
{
    class SqlShell :ShellContext
    {
        public SqlShell(Configuration cfg)
            :base(cfg)
        {
        }

        public void DoCommand()
        {
            StringBuilder builder = new StringBuilder();
            string line = null;
            bool multipleLineMode = false;

            while (true)
            {
                L1:
                stdio.Write("{0}> ", mgr);
                L2:
                line = stdio.ReadLine();

                //ctrl-c captured
                if (line == null)
                    return;


                if (!multipleLineMode)
                {

                    if (line == "exit")
                        break;

                    switch (line)
                    {
                        case "help":
                        case "?":
                            Help();
                            builder.Clear();
                            goto L1;

                        case "cls":
                            Console.Clear();
                            goto L1;

                        default:
                            if (TrySingleLineCommand(line))
                            {
                                stdio.WriteLine();
                                goto L1;
                            }
                            break;
                    }
                }

                if (line != "" && line != ";")
                    builder.AppendLine(line);

                if (line.EndsWith(";"))
                {
                    string text = builder.ToString().Trim();
                    builder.Clear();

                    if (text.EndsWith(";"))
                        text = text.Substring(0, text.Length - 1);

                    try
                    {
                        DoMultipleLineCommand(text);
                        multipleLineMode = false;
                        stdio.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        stdio.WriteLine(ex.Message);
                    }
                }
                else if (builder.ToString() != "")
                {
                    multipleLineMode = true;
                    stdio.Write("...");
                    goto L2;
                }
            }
        }

        private bool TrySingleLineCommand(string text)
        {
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

            return true;
        }
        private bool DoSingleLineCommand(string text)
        {
            text = text.Trim();
            if (text == string.Empty)
                return false;


            Command cmd = new Command(text, cfg);
            switch (cmd.Action)
            {
                case "set":
                    commandee.set(cmd);
                    return true;

                case "let":
                    commandee.let(cmd);
                    return true;

                case "md":
                case "mkdir":
                    commandee.mkdir(cmd);
                    return true;

                case "rd":
                case "rmdir":
                    commandee.rmdir(cmd);
                    return true;
            }


            switch (cmd.Action)
            {
                case "ls":
                case "dir":
                    commandee.dir(cmd);
                    return true;

                case "cd":
                case "chdir":
                    if (cmd.arg1 != null || cmd.HasHelp)
                        chdir(cmd);
                    else
                        stdio.WriteLine(mgr.ToString());
                    return true;

                case "type":
                    commandee.type(cmd);
                    return true;

                case "del":
                case "erase":
                    commandee.del(cmd);
                    return true;

                case "ren":
                case "rename":
                    commandee.rename(cmd);
                    return true;

                case "echo":
                    stdio.WriteLine(text);
                    return true;

                case "rem":
                    return true;

                case "ver":
                    stdio.WriteLine("sqlcon [Version {0}]", System.Reflection.Assembly.GetEntryAssembly().GetName().Version);
                    return true;

                case "show":
                    if (cmd.arg1 != null)
                        Show(cmd.arg1.ToLower(), cmd.arg2);
                    else
                        stdio.ErrorFormat("invalid argument");
                    return true;

                case "find":
                    if (cmd.arg1 != null)
                        theSide.FindName(cmd.arg1);
                    else
                        stdio.ErrorFormat("find object undefined");
                    return true;

                case "xcopy":
                    if (cmd.arg1 == "output")
                    {
                        if (!File.Exists(this.cfg.OutputFile))
                        {
                            stdio.ErrorFormat("no output file found : {0}", this.cfg.OutputFile);
                            break;
                        }
                        using (var reader = new StreamReader(this.cfg.OutputFile))
                        {
                            string data = reader.ReadToEnd();
                            System.Windows.Clipboard.SetText(data);
                            stdio.WriteLine("copied to clipboard");
                        }
                    }
                    return true;

                case "execute":
                    {
                        string inputfile = cfg.InputFile;
                        if (cmd.arg1 != null)
                            inputfile = cmd.arg1;

                        if (cmd.IsSchema)
                        {
                            string tag = inputfile;
                            string[] files = cfg.GetValue<string[]>(tag);
                            if (files == null)
                            {
                                stdio.ErrorFormat("no varible string[] {0} found on config file: {0}", tag);
                                return true;
                            }

                            foreach (var file in files)
                            {
                                if (!theSide.ExecuteScript(file))
                                {
                                    if (!stdio.YesOrNo("are you sure to continue(y/n)?"))
                                    {
                                        stdio.ErrorFormat("interupted on {0}", file);
                                        return true;
                                    }
                                }
                            }
                        }
                        else
                            theSide.ExecuteScript(inputfile);
                    }

                    return true;

                case "open":
                    switch (cmd.arg1)
                    {
                        case "input":
                            stdio.OpenEditor(cfg.InputFile);
                            break;

                        case "output":
                            stdio.OpenEditor(cfg.OutputFile);
                            break;

                        case "schema":
                            stdio.OpenEditor(cfg.SchemaFile);
                            break;

                        case "log":
                            stdio.OpenEditor(Context.GetValue<string>("log"));
                            break;
                    }

                    return true;


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
                                return true;
                            }

                            var adapter = new CompareAdapter(both.ps1.side, both.ps2.side);
                            var sql = adapter.Run(type, both.ps1.MatchedTables, both.ps2.MatchedTables, cfg, cmd.Columns);
                            writer.Write(sql);
                        }
                        stdio.WriteLine("completed");
                        return true;
                    }

                case "copy":
                    commandee.xcopy(cmd, CompareSideType.copy);
                    return true;

                case "sync":
                    commandee.xcopy(cmd, CompareSideType.sync);
                    return true;

                case "comp":
                    commandee.xcopy(cmd, CompareSideType.compare);
                    return true;

                //example: run func(id=20)
                case "run":
                    {
                        VAL result = Context.Evaluate(cmd.args);
                        if (result.IsNull)
                            stdio.ErrorFormat("undefined query function");
                        else if (result.IsInt)
                        {
                            //show error code
                        }
                        else
                        {
                            if (!result.IsList && result.Size != 2)
                            {
                                stdio.ErrorFormat("invalid format, run query like >run query(id=1)");
                                return true;
                            }

                            DataSet ds = new SqlCmd(theSide.Provider, (string)result[0], result[1]).FillDataSet();
                            if (ds != null)
                            {
                                foreach (DataTable dt in ds.Tables)
                                    dt.ToConsole();
                            }
                            else
                                stdio.ErrorFormat("cannot retrieve data from server");
                        }
                    }
                    return true;

                case "export":
                    var exporter = new Exporter(this);
                    return exporter.ExportSqlScript(cmd);

                default:
                    break;
            }

            return false;
        }

       

    
        private void chdir(Command cmd)
        {
            if (commandee.chdir(cmd))
            {
                var dname = mgr.GetCurrentPath<DatabaseName>();
                if (dname != null)
                    theSide.UpdateDatabase(dname.Provider);
                else
                {
                    var sname = mgr.GetCurrentPath<ServerName>();
                    if (sname != null)
                        theSide.UpdateDatabase(sname.Provider);
                }
            }
        }

        private static string showConnection(ConnectionProvider cs)
        {
            return string.Format("S={0} db={1} U={2} P={3}", cs.DataSource, cs.InitialCatalog, cs.UserId, cs.Password);
        }

        private void DoMultipleLineCommand(string text)
        {
            text = text.Trim();
            if (text == string.Empty)
                return;

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
                        new SqlCmd(theSide.Provider, text).Execute(reader => reader.ToConsole(Context.GetValue<int>(Context.MAXROWS, 100)));
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
                        if (count >0)
                            stdio.WriteLine("{0} of row(s) affected", count);
                        else if(count == 0)
                            stdio.WriteLine("nothing affected");
                        else
                            stdio.WriteLine("command(s) completed successfully");
                    }
                    catch (Exception ex)
                    {
                        stdio.ErrorFormat(ex.Message);
                    }
                    break;

                default:
                    if (char.IsDigit(cmd[0]))
                    {
                        stdio.ErrorFormat("invalid command");
                        break;
                    }
                    else
                    {
                        if (text.EndsWith(";"))
                            Tie.Script.Execute(text, Context.DS);
                        else
                        {
                            var val = Tie.Script.Evaluate(text, Context.DS);
                            stdio.WriteLine(string.Format("{0} results {1}", text, val));
                        }
                    }

                    break;
            }
        }


        private void Show(string arg1, string arg2)
        {
            TableName[] vnames;

            switch (arg1)
            {
                case "vw":
                    vnames = new MatchedDatabase(theSide.DatabaseName, arg2, null).DefaultViewNames;
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
                    vnames = new MatchedDatabase(theSide.DatabaseName, arg2, null).DefaultViewNames;
                    vnames.Select(tname => new { Schema = tname.SchemaName, View = tname.Name })
                        .ToConsole();
                    break;

                case "proc":
                    theSide.DatabaseName.AllProc().ToConsole();
                    break;

                case "index":
                    theSide.DatabaseName.AllIndices().ToConsole();
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
            stdio.WriteLine("      \\server\\database\\table\\filter\\filter\\....");
            stdio.WriteLine("Notes: table names support wildcard matching, e.g. Prod*,Pro?ucts");
            stdio.WriteLine("exit                    : quit application");
            stdio.WriteLine("help                    : this help");
            stdio.WriteLine("?                       : this help");
            stdio.WriteLine("cls                     : clears the screen");
            stdio.WriteLine("dir,ls /?               : see more info");
            stdio.WriteLine("cd,chdir /?             : see more info");
            stdio.WriteLine("md,mkdir /?             : see more info");
            stdio.WriteLine("rd,rmdir /?             : see more info");
            stdio.WriteLine("type /?                 : see more info");
            stdio.WriteLine("set /?                  : see more info");
            stdio.WriteLine("del,erase /?            : see more info");
            stdio.WriteLine("ren,rename /?           : see more info");
            stdio.WriteLine("copy /?                 : see more info");
            stdio.WriteLine("comp /?                 : see more info");
            stdio.WriteLine("echo                    : display message");
            stdio.WriteLine("rem                     : records comments/remarks");
            stdio.WriteLine("ver                     : display version");
            stdio.WriteLine("compare path1 [path2]   : compare table scheam or data");
            stdio.WriteLine("          /s            : compare schema otherwise compare data");
            stdio.WriteLine("          /col:c1,c2    : skip columns defined during comparing");
            stdio.WriteLine();
            stdio.WriteLine("<Commands>");
            stdio.WriteLine("<find> pattern          : find table name or column name");
            stdio.WriteLine("<show view>             : show all views");
            stdio.WriteLine("<show proc>             : show all stored proc and func");
            stdio.WriteLine("<show index>            : show all indices");
            stdio.WriteLine("<show vw> viewnames     : show view structure");
            stdio.WriteLine("<show connection>       : show connection-string list");
            stdio.WriteLine("<show current>          : show current active connection-string");
            stdio.WriteLine("<show var>              : show variable list");
            stdio.WriteLine("<run> query(..)         : run predefined query. e.g. run query(var1=val1,...);");
            stdio.WriteLine("<sync table1 table2>    : synchronize, make table2 is the same as table1");
            stdio.WriteLine("<xcopy output>          : copy sql script ouput to clipboard");
            stdio.WriteLine("<open log>              : open log file");
            stdio.WriteLine("<open input>            : open input file");
            stdio.WriteLine("<open output>           : open output file");
            stdio.WriteLine("<open schema>           : open schema file");
            stdio.WriteLine("<export insert>         : export INSERT INTO script on current table/database");
            stdio.WriteLine("<export create>         : export CREATE TABLE script on current table/database");
            stdio.WriteLine("<export schema>         : export database schema xml file");
            stdio.WriteLine("<export class>          : export C# class");
            stdio.WriteLine("<execute inputfile>     : execute sql script file");
            stdio.WriteLine("<execute variable /s>   : execute script file list defined on the configuration file");
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
        }
    }
}

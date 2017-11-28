﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys;
using Sys.Data;
using Sys.Data.Comparison;
using Sys.Data.Manager;

namespace sqlcon
{
    class Exporter
    {
        private PathManager mgr;
        private Command cmd;
        private Configuration cfg;


        private TableName tname;
        private DatabaseName dname;
        private ServerName sname;

        XmlDbFile xml;
        public Exporter(PathManager mgr, TreeNode<IDataPath> pt, Command cmd)
        {
            this.mgr = mgr;
            this.cmd = cmd;
            this.cfg = cmd.Configuration;

            this.xml = new XmlDbFile { XmlDbFolder = cfg.XmlDbDirectory };

            if (pt.Item is Locator)
            {
                this.tname = mgr.GetPathFrom<TableName>(pt);
                this.dname = tname.DatabaseName;
                this.sname = dname.ServerName;
            }
            else if (pt.Item is TableName)
            {
                this.tname = (TableName)pt.Item;
                this.dname = tname.DatabaseName;
                this.sname = dname.ServerName;
            }
            else if (pt.Item is DatabaseName)
            {
                this.tname = null;
                this.dname = (DatabaseName)pt.Item;
                this.sname = dname.ServerName;
            }
            else if (pt.Item is ServerName)
            {
                this.tname = null;
                this.dname = null;
                this.sname = (ServerName)pt.Item;
            }

        }
        private string fileName
        {
            get
            {
                string output = cmd.GetValue("out");
                if (!string.IsNullOrEmpty(output))
                {
                    string directory = Path.GetDirectoryName(output);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    return output;
                }

                return cfg.OutputFile;
            }
        }

        private TableName[] getTableNames(Command cmd)
        {
            TableName[] tnames;
            if (cmd.wildcard != null)
            {
                var md = new MatchedDatabase(dname, cmd.wildcard, null);
                tnames = md.MatchedTableNames;
                if (tnames.Length == 0)
                {
                    cerr.WriteLine("warning: no table is matched");
                    return new TableName[] { };
                }
            }
            else
            {
                tnames = dname.GetTableNames();
            }

            return tnames;
        }


        public void ExportScud(SqlScriptType type)
        {
            if (tname != null)
            {
                using (var writer = fileName.NewStreamWriter())
                {
                    string sql = Compare.GenerateTemplate(new TableSchema(tname), type);
                    cout.WriteLine(sql);
                    writer.WriteLine(sql);
                }
            }
            else
            {
                cerr.WriteLine("warning: table is not selected");
            }
        }


        public void ExportCreate()
        {
            if (tname != null)
            {
                cout.WriteLine("start to generate CREATE TABLE script: {0}", dname);
                using (var writer = fileName.NewStreamWriter())
                {
                    writer.WriteLine(tname.GenerateCluase(true));
                }
                cout.WriteLine("completed to generate script on file: {0}", fileName);
                return;
            }


            if (dname != null)
            {
                if (cmd.wildcard != null)
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, cfg.exportIncludedTables);
                    TableName[] tnames = md.MatchedTableNames;
                    if (tnames.Length > 0)
                    {
                        using (var writer = fileName.NewStreamWriter())
                        {
                            foreach (var tname in tnames)
                            {
                                cout.WriteLine("start to generate CREATE TABLE script: {0} ", tname);
                                writer.WriteLine(tname.GenerateCluase(true));
                            }
                        }
                    }
                    else
                    {
                        cerr.WriteLine("warning: no table is matched");
                        return;
                    }
                }
                else
                {
                    cout.WriteLine("start to generate CREATE TABLE script: {0}", dname);
                    using (var writer = fileName.NewStreamWriter())
                    {
                        writer.WriteLine(dname.GenerateClause());
                    }
                }

                cout.WriteLine("completed to generate script on file: {0}", fileName);
                return;
            }

            cerr.WriteLine("warning: table or database is not seleted");
        }

        public void ExportInsert()
        {
            if (tname != null)
            {
                if (cmd.IsSchema)
                {
                    using (var writer = fileName.NewStreamWriter())
                    {
                        string sql = Compare.GenerateTemplate(new TableSchema(tname), SqlScriptType.INSERT);
                        cout.WriteLine(sql);
                        writer.WriteLine(sql);
                    }
                }
                else
                {
                    var node = mgr.GetCurrentNode<Locator>();
                    int count;

                    using (var writer = fileName.NewStreamWriter())
                    {
                        if (node != null)
                        {
                            cout.WriteLine("start to generate {0} INSERT script to file: {1}", tname, fileName);
                            Locator locator = mgr.GetCombinedLocator(node);
                            count = Compare.GenerateRows(writer, new TableSchema(tname), locator, cmd.HasIfExists);
                            cout.WriteLine("insert clauses (SELECT * FROM {0} WHERE {1}) generated to {2}", tname, locator, fileName);
                        }
                        else
                        {
                            count = Compare.GenerateRows(writer, new TableSchema(tname), null, cmd.HasIfExists);
                            cout.WriteLine("insert clauses (SELECT * FROM {0}) generated to {1}", tname, fileName);
                        }
                    }
                }
            }
            else if (dname != null)
            {
                cout.WriteLine("start to generate {0} script to file: {1}", dname, fileName);
                using (var writer = fileName.NewStreamWriter())
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, cfg.exportIncludedTables);
                    TableName[] tnames = md.MatchedTableNames;
                    CancelableWork.CanCancel(cts =>
                    {
                        foreach (var tn in tnames)
                        {
                            if (cts.IsCancellationRequested)
                                return;

                            if (!cfg.exportIncludedTables.IsMatch(tn.ShortName))
                            {
                                int count = new SqlCmd(tn.Provider, string.Format("SELECT COUNT(*) FROM {0}", tn)).FillObject<int>();
                                if (count > cfg.Export_Max_Count)
                                {
                                    if (!cin.YesOrNo($"are you sure to export {count} rows on {tn.ShortName} (y/n)?"))
                                    {
                                        cout.WriteLine("\n{0,10} skipped", tn.ShortName);
                                        continue;
                                    }
                                }

                                count = Compare.GenerateRows(writer, new TableSchema(tn), null, cmd.HasIfExists);
                                cout.WriteLine("{0,10} row(s) generated on {1}", count, tn.ShortName);
                            }
                            else
                                cout.WriteLine("{0,10} skipped", tn.ShortName);
                        }
                        cout.WriteLine("completed");

                    });
                }
            }
            else
                cerr.WriteLine("warning: table or database is not selected");
        }

        public void ExportSchema()
        {
            if (dname != null)
            {
                cout.WriteLine("start to generate database schema {0}", dname);
                var file = xml.WriteSchema(dname);
                cout.WriteLine("completed {0}", file);
            }
            else if (sname != null)
            {
                if (sname != null)
                {
                    cout.WriteLine("start to generate server schema {0}", sname);
                    var file = xml.WriteSchema(sname);
                    cout.WriteLine("completed {0}", file);
                }
                else
                    cerr.WriteLine("warning: server or database is not selected");
            }
        }

        public void ExportData()
        {
            if (tname != null)
            {
                cout.WriteLine("start to generate {0} data file", tname);
                var dt = new TableReader(tname).Table;
                var file = xml.Write(tname, dt);
                cout.WriteLine("completed {0} =>{1}", tname.ShortName, file);
            }

            else if (dname != null)
            {
                cout.WriteLine("start to generate {0}", dname);
                var mt = new MatchedDatabase(dname, cmd.wildcard, cfg.exportIncludedTables);
                CancelableWork.CanCancel(cts =>
                {
                    foreach (var tname in mt.MatchedTableNames)
                    {
                        if (cts.IsCancellationRequested)
                            return;


                        cout.WriteLine("start to generate {0}", tname);
                        var dt = new SqlBuilder().SELECT.TOP(cmd.Top).COLUMNS().FROM(tname).SqlCmd.FillDataTable();
                        var file = xml.Write(tname, dt);
                        cout.WriteLine("completed {0} => {1}", tname.ShortName, file);
                    }
                    return;
                }
               );

                if (cmd.Top == 0)
                    cout.WriteLine("completed");
                else
                    cout.WriteLine("completed to export TOP {0} row(s) for each table", cmd.Top);
            }
            else
            {
                cerr.WriteLine("warning: table or database is not seleted");
            }
        }

        public void ExportClass()
        {
            DpoOption option = new DpoOption();

            option.NameSpace = cfg.GetValue<string>("dpo.ns", "Sys.DataModel.Dpo");
            option.OutputPath = cfg.GetValue<string>("dpo.path", $"{Configuration.MyDocuments}\\DataModel\\Dpo");
            option.Level = cfg.GetValue<Level>("dpo.level", Level.Application);
            option.HasProvider = cfg.GetValue<bool>("dpo.hasProvider", false);
            option.HasTableAttribute = cfg.GetValue<bool>("dpo.hasTableAttr", true);
            option.HasColumnAttribute = cfg.GetValue<bool>("dpo.hasColumnAttr", true);
            option.IsPack = cfg.GetValue<bool>("dpo.isPack", true);
            option.CodeSorted = cmd.Has("sort");

            option.ClassNameSuffix = cfg.GetValue<string>("dpo.suffix", Setting.DPO_CLASS_SUFFIX_CLASS_NAME);
            option.ClassNameRule =
                name => name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower() + option.ClassNameSuffix;

            if (tname != null)
            {
                var clss = new DpoGenerator(tname) { Option = option };
                clss.CreateClass();
                cout.WriteLine("generated class {0} at {1}", tname.ShortName, option.OutputPath);
            }
            else if (dname != null)
            {
                cout.WriteLine("start to generate database {0} class to directory: {1}", dname, option.OutputPath);
                CancelableWork.CanCancel(cts =>
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, cfg.exportIncludedTables);
                    TableName[] tnames = md.MatchedTableNames;
                    foreach (var tn in tnames)
                    {
                        if (cts.IsCancellationRequested)
                            return;


                        try
                        {
                            var clss = new DpoGenerator(tn) { Option = option };
                            clss.CreateClass();
                            cout.WriteLine("generated class for {0} at {1}", tn.ShortName, option.OutputPath);
                        }
                        catch (Exception ex)
                        {
                            cerr.WriteLine($"failed to generate class {tn.ShortName}, {ex.Message}");
                        }
                    }

                    cout.WriteLine("completed");
                    return;
                });
            }
            else
            {
                cerr.WriteLine("warning: database is not selected");
            }

        }

        public void ExportCsvFile()
        {
            string path = cfg.GetValue<string>("csv.path", $"{Configuration.MyDocuments}\\csv");

            string file;
            Func<TableName, string> fullName = tname => $"{path}\\{sname.Path}\\{dname.Name}\\{tname.ShortName}.csv";

            if (tname != null)
            {
                cout.WriteLine("start to generate {0} csv file", tname);
                file = fullName(tname);
                var dt = new SqlBuilder().SELECT.COLUMNS(cmd.Columns).FROM(tname).SqlCmd.FillDataTable();
                using (var writer = file.NewStreamWriter())
                {
                    CsvFile.Write(dt, writer, true);
                }
                cout.WriteLine("completed {0} => {1}", tname.ShortName, file);
            }
            else if (dname != null)
            {
                cout.WriteLine("start to generate {0} csv to directory: {1}", dname, path);
                CancelableWork.CanCancel(cts =>
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, cfg.exportIncludedTables);
                    TableName[] tnames = md.MatchedTableNames;
                    foreach (var tn in tnames)
                    {
                        if (cts.IsCancellationRequested)
                            return;

                        try
                        {
                            file = fullName(tn);
                            var dt = new TableReader(tn).Table;
                            using (var writer = file.NewStreamWriter())
                            {
                                CsvFile.Write(dt, writer, true);
                            }
                            cout.WriteLine("generated for {0} at {1}", tn.ShortName, path);
                        }
                        catch (Exception ex)
                        {
                            cerr.WriteLine($"failed to generate {tn.ShortName}, {ex.Message}");
                        }
                    }

                    cout.WriteLine("completed");
                    return;
                });
            }
            else
            {
                cerr.WriteLine("warning: table or database is not seleted");
            }
        }

        public void ExportDataContract(int version)
        {
            string path = cmd.GetValue("out") ?? cfg.GetValue<string>("dc.path", $"{Configuration.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns") ?? cfg.GetValue<string>("dc.ns", "Sys.DataModel.DataContract");
            string clss = cmd.GetValue("class");

            DataSet ds = ShellHistory.LastDataSet();
            List<DataTable> list = new List<DataTable>();

            if (ds != null)
            {
                string[] items = new string[] { };
                if (clss != null)
                    items = clss.Split(',');

                int i = 0;
                foreach (DataTable dt in ds.Tables)
                {
                    list.Add(dt);
                    if (i < items.Length)
                        dt.TableName = items[i];

                    i++;
                }
            }
            else if (tname != null)
            {
                var dt = new SqlCmd(tname.Provider, $"SELECT TOP 1 * FROM {tname.FormalName}").FillDataTable();
                dt.TableName = tname.Name;
                list.Add(dt);
            }
            else if (dname != null)
            {
                path = path + "\\" + dname.Name;
                TableName[] tnames = getTableNames(cmd);
                foreach (var tn in tnames)
                {
                    var dt = new SqlCmd(tn.Provider, $"SELECT TOP 1 * FROM {tn.FormalName}").FillDataTable();
                    dt.TableName = tn.Name;
                    list.Add(dt);
                }
            }
            else
            {
                cerr.WriteLine("data table cannot find, use command type or select first");
                return;
            }

            foreach (DataTable dt in list)
            {
                ExportDataContractClass(path, version, dt, ns, className: dt.TableName);
            }
        }


        private void ExportDataContractClass(string path, int version, DataTable dt, string ns, string className)
        {

            string mtd = cmd.GetValue("method");
            string[] keys = cmd.Columns;

            if (version == 1)
            {
                var builder = new DataContractClassBuilder(cmd, dt)
                {
                    ns = ns,
                    cname = className,
                    mtd = mtd,
                    keys = keys
                };

                string file = builder.WriteFile(path);
                cout.WriteLine("code generated on {0}", file);
            }
            else
            {
                var builder = new DataContract2ClassBuilder(cmd, dt)
                {
                    ns = ns,
                    cname = className,
                    mtd = mtd
                };

                string file = builder.WriteFile(path);
                cout.WriteLine("code generated on {0}", file);
            }
        }

        public void ExportEntityClass()
        {
            if (dname == null)
            {
                cerr.WriteLine("select a database first");
                return;
            }

            string path = cmd.GetValue("out") ?? cfg.GetValue<string>("dc.path", $"{Configuration.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns") ?? cfg.GetValue<string>("dc.ns", "Sys.DataModel.DataContracts");

            if (tname != null)
            {
                cout.WriteLine("start to generate {0} entity framework class file", tname);
                var builder = new EntityClassBuilder(cmd, tname)
                {
                    ns = ns
                };

                string file = builder.WriteFile(path);
                cout.WriteLine("completed {0} => {1}", tname.ShortName, file);
            }
            else if (dname != null)
            {
                cout.WriteLine("start to generate {0} entity framework class to directory: {1}", dname, path);
                CancelableWork.CanCancel(cts =>
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, null); //cfg.exportExcludedTables);
                    TableName[] tnames = md.MatchedTableNames;
                    foreach (var tn in tnames)
                    {
                        if (cts.IsCancellationRequested)
                            return;

                        try
                        {
                            var builder = new EntityClassBuilder(cmd, tn)
                            {
                                ns = ns
                            };

                            string file = builder.WriteFile(path);
                            cout.WriteLine("generated for {0} at {1}", tn.ShortName, path);
                        }
                        catch (Exception ex)
                        {
                            cerr.WriteLine($"failed to generate {tn.ShortName}, {ex.Message}");
                        }
                    }

                    cout.WriteLine("completed");
                    return;
                });
            }
            else
            {
                cerr.WriteLine("warning: table or database is not seleted");
            }

        }



        public void ExportLinq2SQLClass()
        {
            string path = cfg.GetValue<string>("l2s.path", $"{Configuration.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns") ?? cfg.GetValue<string>("l2s.ns", "Sys.DataModel.L2s");
            Dictionary<TableName, TableSchema> schemas = new Dictionary<TableName, TableSchema>();

            if (tname != null)
            {
                var builder = new Linq2SQLClassBuilder(cmd, tname, schemas)
                {
                    ns = ns
                };
                string file = builder.WriteFile(path);
                cout.WriteLine("code generated on {0}", file);
            }
            else if (dname != null)
            {

                TableName[] tnames = getTableNames(cmd);
                foreach (var tname in tnames)
                {
                    var builder = new Linq2SQLClassBuilder(cmd, tname, schemas)
                    {
                        ns = ns
                    };

                    string file = builder.WriteFile(path);
                    cout.WriteLine("code generated on {0}", file);
                }

                return;
            }
            else
            {
                cerr.WriteLine("warning: table or database is not seleted");
            }
        }


        private DataTable LastOrCurrentTable()
        {
            var dt = ShellHistory.LastOrCurrentTable(tname);
            if (dt == null)
            {
                cerr.WriteLine("display data table first by sql clause or command [type]");
                return null;
            }

            if (dt.Rows.Count == 0)
            {
                cerr.WriteLine("no rows found");
                return null;
            }

            return dt;
        }

        public void ExportJson()
        {
            var dt = LastOrCurrentTable();
            if (dt == null)
                return;

            cout.WriteLine(TableOut.ToJson(dt));
        }

        /// <summary>
        /// create C# data class from data table
        /// </summary>
        /// <param name="cmd"></param>
        public void ExportCSharpData()
        {
            var dt = LastOrCurrentTable();
            if (dt == null)
                return;

            var builder = new DataClassBuilder(cmd, dt);
            builder.ExportCSharpData();
        }

        public void ExportConfigurationClass()
        {
            var dt = LastOrCurrentTable();

            //not .cfg file
            if (cmd.GetValue("in") == null)
            {
                if (dt == null)
                    return;
            }

            var builder = new ConfClassBuilder(cmd, dt);
            builder.ExportCSharpData();
        }

        public void ExportConfigurationFile()
        {
            var dt = LastOrCurrentTable();

            //not .cfg file
            if (cmd.GetValue("in") == null)
            {
                if (dt == null)
                    return;
            }

            var builder = new ConfClassBuilder(cmd, dt);
            string _type = cmd.GetValue("type") ?? "f";     //f:flat, h:hierarchial
            builder.ExportTie(_type == "f");
        }

        public static void Help()
        {
            cout.WriteLine("export data, schema, class, and template on current selected server/db/table");
            cout.WriteLine("option of SQL generation:");
            cout.WriteLine("   /insert  : export INSERT INTO script on current table/database");
            cout.WriteLine("   [/if]    : option /if generate if exists row then UPDATE else INSERT");
            cout.WriteLine("   /create  : generate CREATE TABLE script on current table/database");
            cout.WriteLine("   /select  : generate SELECT FROM WHERE template");
            cout.WriteLine("   /update  : generate UPDATE SET WHERE template");
            cout.WriteLine("   /save    : generate IF EXISTS UPDATE ELSE INSERT template");
            cout.WriteLine("   /delete  : generate DELETE FROM WHERE template, delete rows with foreign keys constraints");
            cout.WriteLine("option of data generation:");
            cout.WriteLine("   /schema  : generate database schema xml file");
            cout.WriteLine("   /data    : generate database/table data xml file");
            cout.WriteLine("   /csv     : generate table csv file");
            cout.WriteLine("   /json    : generate json from last result");
            cout.WriteLine("option of code generation:");
            cout.WriteLine("   /dpo     : generate C# table class");
            cout.WriteLine("   /l2s     : generate C# Linq to SQL class");
            cout.WriteLine("   /dc1     : generate C# data contract class and extension class from last result");
            cout.WriteLine("      [/readonly]: contract class for reading only");
            cout.WriteLine("   /dc2     : generate C# data contract class from last result");
            cout.WriteLine("      [/method:name] default convert method is defined on the .cfg");
            cout.WriteLine("      [/col:pk1,pk2] default primary key is the first column");
            cout.WriteLine("   /entity  : generate C# method copy/compare/clone for Entity framework");
            cout.WriteLine("      [/base:type] define base class or interface, use ~ to represent generic class itself, delimited by ;");
            cout.WriteLine("   /c#      : generate C# data from last result");
            cout.WriteLine("      [/type:dict|list|enum] data type, default is list");
            cout.WriteLine("   /conf    : generate Config C# class");
            cout.WriteLine("      [/type:k|d|f|p|F|P] C# class type, default is kdP");
            cout.WriteLine("          k : generate class of const key");
            cout.WriteLine("          d : generate class of default value");
            cout.WriteLine("          P : generate class of static property");
            cout.WriteLine("          F : generate class of static field");
            cout.WriteLine("          p : generate class of hierarchial property");
            cout.WriteLine("          f : generate class of hierarchial field");
            cout.WriteLine("      [/method:name] GetValue method name, default is \"GetValue<>\"");
            cout.WriteLine("      [/kc:name] class name of const key");
            cout.WriteLine("      [/dc:name] class name of default value");
            cout.WriteLine("   /cfg    : generate config file");
            cout.WriteLine("      [/type:f|h] script type");
            cout.WriteLine("          h : generate TIE hierarchial config script file");
            cout.WriteLine("          f : generate TIE config script file");
            cout.WriteLine("common options /conf and /cfg");
            cout.WriteLine("      [/in:path] input path(.cfg)");
            cout.WriteLine("      [/key:column] column of key on config table");
            cout.WriteLine("      [/default:column] column of default value config table");
            cout.WriteLine("common options for code generation");
            cout.WriteLine("      [/ns:name] default name space is defined on the .cfg");
            cout.WriteLine("      [/class:name] default class name is defined on the .cfg");
            cout.WriteLine("      [/using:assembly] allow the use of types in a namespace, delimited by ;");
            cout.WriteLine("      [/out:path] output directory or file name (.cs)");
        }

        public void Run()
        {
            if (cmd.Has("insert"))
                ExportInsert();
            else if (cmd.Has("create"))
                ExportCreate();
            else if (cmd.Has("select"))
                ExportScud(SqlScriptType.SELECT);
            else if (cmd.Has("delete"))
                ExportScud(SqlScriptType.DELETE);
            else if (cmd.Has("update"))
                ExportScud(SqlScriptType.UPDATE);
            else if (cmd.Has("save"))
                ExportScud(SqlScriptType.INSERT_OR_UPDATE);
            else if (cmd.Has("schema"))
                ExportSchema();
            else if (cmd.Has("data"))
                ExportData();
            else if (cmd.Has("dpo"))
                ExportClass();
            else if (cmd.Has("csv"))
                ExportCsvFile();
            else if (cmd.Has("dc1"))
                ExportDataContract(1);
            else if (cmd.Has("dc2"))
                ExportDataContract(2);
            else if (cmd.Has("entity"))
                ExportEntityClass();
            else if (cmd.Has("l2s"))
                ExportLinq2SQLClass();
            else if (cmd.Has("json"))
                ExportJson();
            else if (cmd.ToCSharp)
                ExportCSharpData();
            else if (cmd.Has("conf"))
                ExportConfigurationClass();
            else if (cmd.Has("cfg"))
                ExportConfigurationFile();
            else
                cerr.WriteLine("invalid command options");
        }
    }
}

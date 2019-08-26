﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys;
using Sys.Data;
using Sys.Data.Comparison;
using Sys.Data.Manager;
using Sys.Stdio;

namespace sqlcon
{
    class Exporter
    {
        private PathManager mgr;
        private ApplicationCommand cmd;
        private Configuration cfg;


        private TableName tname;
        private DatabaseName dname;
        private ServerName sname;

        XmlDbCreator xmlDbFile;
        public Exporter(PathManager mgr, TreeNode<IDataPath> pt, ApplicationCommand cmd)
        {
            this.mgr = mgr;
            this.cmd = cmd;
            this.cfg = cmd.Configuration;

            this.xmlDbFile = new XmlDbCreator
            {
                XmlDbFolder = cfg.XmlDbDirectory
            };

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
        private string fileName => cmd.OutputFile();

        private TableName[] getTableNames(ApplicationCommand cmd)
        {
            TableName[] tnames;
            if (cmd.wildcard != null)
            {
                var md = new MatchedDatabase(dname, cmd);
                tnames = md.Results();
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

        private static DataTable FillTable(TableName tname)
        {
            var dt = new SqlCmd(tname.Provider, $"SELECT TOP 1 * FROM {tname.FormalName}").FillDataTable();
            dt.TableName = tname.Name;
            var schema = new TableSchema(tname);
            dt.PrimaryKeys(schema.PrimaryKeys.Keys);
            foreach (IColumn column in schema.Columns)
            {
                dt.Columns[column.ColumnName].AllowDBNull = column.Nullable;
            }

            if (dt.Rows.Count > 0)
                dt.Rows[0].Delete();
            dt.AcceptChanges();

            return dt;
        }

        public void ExportScud(SqlScriptType type)
        {
            if (tname != null)
            {
                using (var writer = fileName.CreateStreamWriter(cmd.Append))
                {
                    string sql = Compare.GenerateTemplate(new TableSchema(tname), type, cmd.HasIfExists);
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
                using (var writer = fileName.CreateStreamWriter(cmd.Append))
                {
                    writer.WriteLine(tname.GenerateIfDropClause());
                    writer.WriteLine(tname.GenerateClause());
                }
                cout.WriteLine("completed to generate script on file: {0}", fileName);
                return;
            }


            if (dname != null)
            {
                if (cmd.wildcard != null)
                {
                    var md = new MatchedDatabase(dname, cmd);
                    TableName[] tnames = md.Results();
                    if (tnames.Length > 0)
                    {
                        Stack<string> stack = new Stack<string>();
                        Queue<string> queue = new Queue<string>();
                        foreach (var tname in tnames)
                        {
                            cout.WriteLine("start to generate CREATE TABLE script: {0} ", tname);
                            stack.Push(tname.GenerateIfDropClause());
                            queue.Enqueue(tname.GenerateClause());
                        }

                        using (var writer = fileName.CreateStreamWriter(cmd.Append))
                        {
                            while (stack.Count > 0)
                                writer.WriteLine(stack.Pop());

                            while (queue.Count > 0)
                                writer.WriteLine(queue.Dequeue());
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
                    using (var writer = fileName.CreateStreamWriter(cmd.Append))
                    {
                        writer.WriteLine(dname.GenerateClause());
                    }
                }

                cout.WriteLine("completed to generate script on file: {0}", fileName);
                return;
            }

            cerr.WriteLine("warning: table or database is not seleted");
        }

        public void ExportInsertOrUpdateData(SqlScriptType type)
        {
            if (tname != null)
            {
                var node = mgr.GetCurrentNode<Locator>();
                int count;

                using (var writer = fileName.CreateStreamWriter(cmd.Append))
                {
                    if (node != null)
                    {
                        cout.WriteLine("start to generate {0} INSERT script to file: {1}", tname, fileName);
                        Locator locator = mgr.GetCombinedLocator(node);
                        count = Compare.GenerateRows(type, writer, new TableSchema(tname), locator, cmd.HasIfExists);
                        cout.WriteLine($"{type} clauses (SELECT * FROM {tname} WHERE {locator}) generated to \"{fileName}\"");
                    }
                    else
                    {
                        count = Compare.GenerateRows(type, writer, new TableSchema(tname), null, cmd.HasIfExists);
                        cout.WriteLine($"{type} clauses (SELECT * FROM {tname}) generated to \"{fileName}\"");
                    }
                }
            }
            else if (dname != null)
            {
                cout.WriteLine("start to generate {0} script to file: {1}", dname, fileName);
                using (var writer = fileName.CreateStreamWriter(cmd.Append))
                {
                    var md = new MatchedDatabase(dname, cmd);
                    TableName[] tnames = md.Results();
                    CancelableWork.CanCancel(cts =>
                    {
                        foreach (var tn in tnames)
                        {
                            if (cts.IsCancellationRequested)
                                return;

                            int count = new SqlCmd(tn.Provider, string.Format("SELECT COUNT(*) FROM {0}", tn)).FillObject<int>();
                            if (count > cfg.Export_Max_Count)
                            {
                                if (!cin.YesOrNo($"are you sure to export {count} rows on {tn.ShortName} (y/n)?"))
                                {
                                    cout.WriteLine("\n{0,10} skipped", tn.ShortName);
                                    continue;
                                }
                            }

                            count = Compare.GenerateRows(type, writer, new TableSchema(tn), null, cmd.HasIfExists);
                            cout.WriteLine($"{count,10} row(s) generated on {tn.ShortName}");
                        }

                        cout.WriteLine($"completed to generate {type} clauses to \"{fileName}\"");

                    });
                }
            }
            else
                cerr.WriteLine("warning: table or database is not selected");
        }

        public void ExportSchema()
        {
            string directory = cmd.OutputDirectory();
            if (directory != null)
                xmlDbFile.XmlDbFolder = directory;

            if (dname != null)
            {
                cout.WriteLine("start to generate database schema {0}", dname);
                var file = xmlDbFile.WriteSchema(dname);
                cout.WriteLine("completed {0}", file);
            }
            else if (sname != null)
            {
                if (sname != null)
                {
                    cout.WriteLine("start to generate server schema {0}", sname);
                    var file = xmlDbFile.WriteSchema(sname);
                    cout.WriteLine("completed {0}", file);
                }
                else
                    cerr.WriteLine("warning: server or database is not selected");
            }
        }

        public void ExportData()
        {
            string directory = cmd.OutputDirectory();
            if (directory != null)
                xmlDbFile.XmlDbFolder = directory;

            if (tname != null)
            {
                cout.WriteLine("start to generate {0} data file", tname);
                var dt = new TableReader(tname).Table;
                var file = xmlDbFile.WriteData(tname, dt);
                cout.WriteLine("completed {0} =>{1}", tname.ShortName, file);
            }

            else if (dname != null)
            {
                cout.WriteLine("start to generate {0}", dname);
                var mt = new MatchedDatabase(dname, cmd);
                CancelableWork.CanCancel(cts =>
                {
                    foreach (var tname in mt.Results())
                    {
                        if (cts.IsCancellationRequested)
                            return;


                        cout.WriteLine("start to generate {0}", tname);
                        var dt = new SqlBuilder().SELECT.TOP(cmd.Top).COLUMNS().FROM(tname).SqlCmd.FillDataTable();
                        var file = xmlDbFile.WriteData(tname, dt);
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
            DpoOption option = new DpoOption
            {
                NameSpace = cfg.GetValue<string>(ConfigKey._GENERATOR_DPO_NS, "Sys.DataModel.Dpo"),
                OutputPath = cmd.OutputPath(ConfigKey._GENERATOR_DPO_PATH, $"{Configuration.MyDocuments}\\DataModel\\Dpo"),
                Level = cfg.GetValue<Level>(ConfigKey._GENERATOR_DPO_LEVEL, Level.Application),
                HasProvider = cfg.GetValue<bool>(ConfigKey._GENERATOR_DPO_HASPROVIDER, false),
                HasTableAttribute = cfg.GetValue<bool>(ConfigKey._GENERATOR_DPO_HASTABLEATTR, true),
                HasColumnAttribute = cfg.GetValue<bool>(ConfigKey._GENERATOR_DPO_HASCOLUMNATTR, true),
                IsPack = cfg.GetValue<bool>(ConfigKey._GENERATOR_DPO_ISPACK, true),
                CodeSorted = cmd.Has("sort"),

                ClassNameSuffix = cfg.GetValue<string>(ConfigKey._GENERATOR_DPO_SUFFIX, Setting.DPO_CLASS_SUFFIX_CLASS_NAME)
            };
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
                    var md = new MatchedDatabase(dname, cmd);
                    TableName[] tnames = md.Results();
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
            string path = this.cmd.OutputPath(ConfigKey._GENERATOR_CSV_PATH, $"{Configuration.MyDocuments}\\csv");

            string file;
            string fullName(TableName tname) => $"{path}\\{sname.Path}\\{dname.Name}\\{tname.ShortName}.csv";

            if (tname != null)
            {
                cout.WriteLine("start to generate {0} csv file", tname);
                file = this.cmd.OutputFileName();
                if (file == null)
                    file = fullName(tname);

                var dt = new SqlBuilder().SELECT.COLUMNS(cmd.Columns).FROM(tname).SqlCmd.FillDataTable();
                using (var writer = file.CreateStreamWriter(cmd.Append))
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
                    var md = new MatchedDatabase(dname, cmd);
                    TableName[] tnames = md.Results();
                    foreach (var tn in tnames)
                    {
                        if (cts.IsCancellationRequested)
                            return;

                        try
                        {
                            file = fullName(tn);
                            var dt = new TableReader(tn).Table;
                            using (var writer = file.CreateStreamWriter(cmd.Append))
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
            string path = cmd.OutputPath(ConfigKey._GENERATOR_DC_PATH, $"{Configuration.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns", ConfigKey._GENERATOR_DC_NS, "Sys.DataModel.DataContract");
            string clss = cmd.GetValue("class");
            bool last = cmd.Has("last");

            List<DataTable> list = new List<DataTable>();

            if (last)
            {
                DataSet ds = ShellHistory.LastDataSet();

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
            }
            else if (tname != null)
            {
                var dt = FillTable(tname);
                list.Add(dt);
            }
            else if (dname != null)
            {
                TableName[] tnames = getTableNames(cmd);
                foreach (var tn in tnames)
                {
                    var dt = FillTable(tn);
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
            bool allowDbNull = cmd.Has("NULL");
            string mtd = cmd.GetValue("method");
            string[] keys = cmd.Columns;

            if (version == 0)
            {
                var builder = new DataContractClassBuilder(cmd, dt, allowDbNull)
                {
                    ns = ns,
                    cname = className,
                    mtd = mtd
                };

                string file = builder.WriteFile(path);
                cout.WriteLine("code generated on {0}", file);
            }
            else if (version == 1)
            {
                var builder = new DataContract1ClassBuilder(cmd, dt, allowDbNull)
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
                var builder = new DataContract2ClassBuilder(cmd, dt, allowDbNull)
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

            string path = cmd.OutputPath(ConfigKey._GENERATOR_DC_PATH, $"{Configuration.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns", ConfigKey._GENERATOR_DC_NS, "Sys.DataModel.DataContracts");

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
                    var md = new MatchedDatabase(dname, cmd); //cfg.exportExcludedTables);
                    TableName[] tnames = md.Results();
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
            string path = cmd.OutputPath(ConfigKey._GENERATOR_L2S_PATH, $"{Configuration.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns", ConfigKey._GENERATOR_L2S_NS, "Sys.DataModel.L2s");
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

        private DataSet LastOrCurrentDataSet()
        {
            var ds = ShellHistory.LastOrCurrentTable(tname);
            if (ds == null || ds.Tables.Count == 0)
            {
                return null;
            }

            return ds;
        }

        private DataTable LastOrCurrentTable()
        {
            var ds = LastOrCurrentDataSet();
            if (ds == null || ds.Tables.Count == 0)
                return null;

            return ds.Tables[0];
        }

        public void ExportJson()
        {
            DataSet ds = LastOrCurrentDataSet();
            if (ds == null)
                return;

            cout.WriteLine(ds.WriteJson());
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
            if (cmd.InputPath() == null)
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
            if (cmd.InputPath() == null)
            {
                if (dt == null)
                    return;
            }

            var builder = new ConfClassBuilder(cmd, dt);
            string _type = cmd.GetValue("type") ?? "f";     //f:flat, h:hierarchial
            builder.ExportTie(_type == "f");
        }

        public void ExportDataSetXml()
        {
            if (dname == null)
            {
                cerr.WriteLine("select a database first");
                return;
            }

            string path = cmd.OutputPath(ConfigKey._GENERATOR_DS_PATH, $"{Configuration.MyDocuments}\\ds");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (tname != null)
            {
                cout.WriteLine($"start to generate data file: {tname}");
                var dt = new TableReader(tname).Table;
                dt.TableName = tname.ShortName;

                string file = Path.Combine(path, $"{tname.ShortName}.xml");
                DataSet ds = dt.DataSet;
                ds.DataSetName = dname.Name;

                ds.WriteXml(file, XmlWriteMode.WriteSchema);
                cout.WriteLine($"completed {tname} => {file}");
            }
            else if (dname != null)
            {
                cout.WriteLine($"start to generate data file to directory: {dname}");
                CancelableWork.CanCancel(cts =>
                {
                    var md = new MatchedDatabase(dname, cmd); //cfg.exportExcludedTables);
                    TableName[] tnames = md.Results();
                    DataSet ds = new DataSet
                    {
                        DataSetName = dname.Name,
                    };

                    foreach (var tn in tnames)
                    {
                        if (cts.IsCancellationRequested)
                            return;

                        try
                        {
                            var dt = new TableReader(tn).Table.Copy();
                            dt.TableName = tn.ShortName;
                            ds.Tables.Add(dt);
                            cout.WriteLine($"generated for {tn.ShortName}");
                        }
                        catch (Exception ex)
                        {
                            cerr.WriteLine($"failed to generate {tn.ShortName}, {ex.Message}");
                        }
                    }

                    string file = Path.Combine(path, $"{dname.Name}.xml");
                    ds.WriteXml(file, XmlWriteMode.WriteSchema);
                    cout.WriteLine($"completed generated: {file}");
                    return;
                });
            }
            else
            {
                cerr.WriteLine("warning: table or database is not seleted");
            }

        }

        public static void Help()
        {
            cout.WriteLine("export data, schema, class, and template on current selected server/db/table");
            cout.WriteLine("option:");
            cout.WriteLine("   /out:xxx : output path or file name");
            cout.WriteLine("option of SQL generation:");
            cout.WriteLine("   /INSERT  : export INSERT INTO script on current table/database");
            cout.WriteLine("   /UPDATE  : export UPDATE SETscript on current table/database");
            cout.WriteLine("   /SAVE    : export IF NOT EXISTS INSERT ELSE UPDATE script on current table/database");
            cout.WriteLine("   [/if]    : option /if generate if exists row then UPDATE else INSERT; or check existence of table when drop table");
            cout.WriteLine("   /create  : generate CREATE TABLE script on current table/database");
            cout.WriteLine("   /select  : generate SELECT FROM WHERE template");
            cout.WriteLine("   /insert  : generate INSERT INTO template");
            cout.WriteLine("   /update  : generate UPDATE SET WHERE template");
            cout.WriteLine("   /save    : generate IF EXISTS UPDATE ELSE INSERT template");
            cout.WriteLine("   /delete  : generate DELETE FROM WHERE template, delete rows with foreign keys constraints");
            cout.WriteLine("   /drop    : generate DROP TABLE template, drop tables with foreign keys constraints");
            cout.WriteLine("option of data generation:");
            cout.WriteLine("   /schema  : generate database schema xml file");
            cout.WriteLine("   /data    : generate database/table data xml file");
            cout.WriteLine("      [/include]: include table names with wildcard");
            cout.WriteLine("   /csv     : generate table csv file");
            cout.WriteLine("   /ds      : generate data set xml file");
            cout.WriteLine("   /json    : generate json from last result");
            cout.WriteLine("option of code generation:");
            cout.WriteLine("   /dpo     : generate C# table class");
            cout.WriteLine("   /l2s     : generate C# Linq to SQL class");
            cout.WriteLine("   /dc      : generate C# data contract class");
            cout.WriteLine("   /dc1     : generate C# data contract class and extension class");
            cout.WriteLine("   /dc2     : generate C# data contract class and extension class");
            cout.WriteLine("      option of data contract /[dc|dc1|dc2] :");
            cout.WriteLine("      [/readonly]: contract class for reading only");
            cout.WriteLine("      [/last]: generate C# data contract from last result");
            cout.WriteLine("      [/method:name] default convert method is defined on the .cfg");
            cout.WriteLine("      [/NULL] allow column type be nullable");
            cout.WriteLine("      [/col:pk1,pk2] default primary key is the first column");
            cout.WriteLine("   /entity  : generate C# method copy/compare/clone for Entity framework");
            cout.WriteLine("      [/base:type] define base class or interface, use ~ to represent generic class itself, delimited by ;");
            cout.WriteLine("      [/field:constMap] create const fields for name of columns");
            cout.WriteLine("      [/method:Map,Copy,Equals,Clone,GetHashCode,ToString] create Copy,Equals,Clone,GetHashCode, and ToString method");
            cout.WriteLine("   /c#      : generate C# data from last result");
            cout.WriteLine("      [/type:dict|list|array|enum] data type, default is list");
            cout.WriteLine("      [/code-column:col1=usertype1;col2=usertyp2] define user type for columns");
            cout.WriteLine("   /conf    : generate Config C# class");
            cout.WriteLine("      [/type:k|d|f|p|F|P] C# class type, default is kdP");
            cout.WriteLine("          k : generate class of const key");
            cout.WriteLine("          d : generate class of default value");
            cout.WriteLine("          P : generate class of static property");
            cout.WriteLine("          F : generate class of static field");
            cout.WriteLine("          p : generate class of hierarchial property");
            cout.WriteLine("          f : generate class of hierarchial field");
            cout.WriteLine("          t : generate data contract classes");
            cout.WriteLine("          j : geneerat data classes from JSON");
            cout.WriteLine("      [/method:name] GetValue method name, default is \"GetValue<>\"");
            cout.WriteLine("      [/key:column] column key, required");
            cout.WriteLine("      [/default:column] column default value, required");
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
            if (cmd.Has("INSERT"))
                ExportInsertOrUpdateData(SqlScriptType.INSERT);
            else if (cmd.Has("UPDATE"))
                ExportInsertOrUpdateData(SqlScriptType.UPDATE);
            else if (cmd.Has("SAVE"))
                ExportInsertOrUpdateData(SqlScriptType.INSERT_OR_UPDATE);
            else if (cmd.Has("create"))
                ExportCreate();
            else if (cmd.Has("select"))
                ExportScud(SqlScriptType.SELECT);
            else if (cmd.Has("insert"))
                ExportScud(SqlScriptType.INSERT);
            else if (cmd.Has("delete"))
                ExportScud(SqlScriptType.DELETE);
            else if (cmd.Has("drop"))
                ExportScud(SqlScriptType.DROP);
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
            else if (cmd.Has("dc"))
                ExportDataContract(0);
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
            else if (cmd.Has("c#"))
                ExportCSharpData();
            else if (cmd.Has("conf"))
                ExportConfigurationClass();
            else if (cmd.Has("cfg"))
                ExportConfigurationFile();
            else if (cmd.Has("ds"))
                ExportDataSetXml();
            else
                cerr.WriteLine("invalid command options");
        }
    }
}

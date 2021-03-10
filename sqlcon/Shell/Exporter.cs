using System;
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
using Sys.Data.Resource;
using Sys.Stdio;

namespace sqlcon
{
    class Exporter
    {
        private PathManager mgr;
        private ApplicationCommand cmd;
        private IApplicationConfiguration cfg;


        private TableName tname;
        private DatabaseName dname;
        private ServerName sname;

        XmlDbCreator xmlDbFile;
        public Exporter(PathManager mgr, TreeNode<IDataPath> pt, ApplicationCommand cmd, IApplicationConfiguration cfg)
        {
            this.mgr = mgr;
            this.cmd = cmd;
            this.cfg = cfg;

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
        private string SqlFileName => cmd.OutputFile(cfg.OutputFile);
        private string FileName(string defaultOutputFile) => cmd.OutputFile(defaultOutputFile);

        private TableName[] getTableNames(ApplicationCommand cmd)
        {
            TableName[] tnames;
            if (cmd.wildcard != null)
            {
                var md = new MatchedDatabase(dname, cmd);
                tnames = md.TableNames();
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
            dt.SetSchemaAndTableName(tname);
            
            var schema = new TableSchema(tname);
            dt.PrimaryKeys(schema.PrimaryKeys.Keys);
            foreach (IColumn column in schema.Columns)
            {
                DataColumn col = dt.Columns[column.ColumnName];
                col.AllowDBNull = column.Nullable;
                col.AutoIncrement = column.IsIdentity;
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
                using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
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
                using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
                {
                    writer.WriteLine(tname.GenerateIfDropClause());
                    writer.WriteLine(tname.GenerateCreateTableClause(appendGO: true));
                }
                cout.WriteLine("completed to generate script on file: {0}", SqlFileName);
                return;
            }


            if (dname != null)
            {
                if (cmd.wildcard != null)
                {
                    var md = new MatchedDatabase(dname, cmd);
                    TableName[] tnames = md.TableNames();
                    if (tnames.Length > 0)
                    {
                        Stack<string> stack = new Stack<string>();
                        Queue<string> queue = new Queue<string>();
                        foreach (var tname in tnames)
                        {
                            cout.WriteLine("start to generate CREATE TABLE script: {0} ", tname);
                            stack.Push(tname.GenerateIfDropClause());
                            queue.Enqueue(tname.GenerateCreateTableClause(appendGO: true));
                        }

                        using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
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
                    using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
                    {
                        writer.WriteLine(dname.GenerateClause());
                    }
                }

                cout.WriteLine("completed to generate script on file: {0}", SqlFileName);
                return;
            }

            cerr.WriteLine("warning: table or database is not seleted");
        }

        public void ExportInsertOrUpdateData(SqlScriptType type)
        {
            var option = new SqlScriptGenerationOption
            {
                HasIfExists = cmd.HasIfExists,
                InsertWithoutColumns = cmd.Has("without-columns"),
            };

            if (tname != null)
            {
                var node = mgr.GetCurrentNode<Locator>();
                int count;

                using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
                {
                    if (node != null)
                    {
                        cout.WriteLine("start to generate {0} INSERT script to file: {1}", tname, SqlFileName);
                        Locator locator = mgr.GetCombinedLocator(node);
                        count = Compare.GenerateRows(type, writer, new TableSchema(tname), locator, option);
                        cout.WriteLine($"{type} clauses (SELECT * FROM {tname} WHERE {locator}) generated to \"{SqlFileName}\"");
                    }
                    else
                    {
                        count = Compare.GenerateRows(type, writer, new TableSchema(tname), null, option);
                        cout.WriteLine($"{type} clauses (SELECT * FROM {tname}) generated to \"{SqlFileName}\"");
                    }
                }
            }
            else if (dname != null)
            {
                cout.WriteLine("start to generate {0} script to file: {1}", dname, SqlFileName);
                using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
                {
                    var md = new MatchedDatabase(dname, cmd);
                    TableName[] tnames = md.TableNames();
                    CancelableWork.CanCancel(cts =>
                    {
                        foreach (var tn in tnames)
                        {
                            if (cts.IsCancellationRequested)
                                return;

                            int count = new SqlCmd(tn.Provider, string.Format("SELECT COUNT(*) FROM {0}", tn)).FillObject<int>();
                            if (count > cfg.MaxRows)
                            {
                                if (!cin.YesOrNo($"are you sure to export {count} rows on {tn.ShortName} (y/n)?"))
                                {
                                    cout.WriteLine("\n{0,10} skipped", tn.ShortName);
                                    continue;
                                }
                            }

                            count = Compare.GenerateRows(type, writer, new TableSchema(tn), null, option);
                            cout.WriteLine($"{count,10} row(s) generated on {tn.ShortName}");
                        }

                        cout.WriteLine($"completed to generate {type} clauses to \"{SqlFileName}\"");

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
                    foreach (var tname in mt.TableNames())
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
                OutputPath = cmd.OutputPath(ConfigKey._GENERATOR_DPO_PATH, $"{ConfigurationEnvironment.MyDocuments}\\DataModel\\Dpo"),
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
                    TableName[] tnames = md.TableNames();
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
            string path = this.cmd.OutputPath(ConfigKey._GENERATOR_CSV_PATH, $"{ConfigurationEnvironment.MyDocuments}\\csv");

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
                    TableName[] tnames = md.TableNames();
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

        class TableNameData
        {
            public TableName Name { get; set; }
            public DataTable Data { get; set; }
        }

        public void ExportDataContract(int version)
        {
            bool last = cmd.Has("last");

            List<TableNameData> list = new List<TableNameData>();

            if (last)
            {
                DataSet ds = ShellHistory.LastDataSet();

                if (ds != null)
                {
                    string[] items = new string[] { };
                    string clss = cmd.GetValue("class");
                    if (clss != null)
                        items = clss.Split(',');

                    int i = 0;
                    foreach (DataTable dt in ds.Tables)
                    {
                        list.Add(new TableNameData { Data = dt });
                        if (i < items.Length)
                            dt.TableName = items[i];

                        i++;
                    }
                }
            }
            else if (tname != null)
            {
                var dt = FillTable(tname);
                list.Add(new TableNameData { Name = tname, Data = dt });
            }
            else if (dname != null)
            {
                TableName[] tnames = getTableNames(cmd);
                foreach (var tn in tnames)
                {
                    var dt = FillTable(tn);
                    list.Add(new TableNameData { Name = tn, Data = dt });
                }
            }
            else
            {
                cerr.WriteLine("data table cannot find, use command type or select first");
                return;
            }

            foreach (TableNameData dt in list)
            {
                ExportDataContractClass(version, dt);
            }
        }


        private void ExportDataContractClass(int version, TableNameData tnd)
        {
            DataTable dt = tnd.Data;
            bool allowDbNull = cmd.Has("NULL");
            string[] keys = cmd.Columns;

            DataColumn[] pk = dt.PrimaryKey;
            if (pk == null || pk.Length == 0)
            {
                pk = dt.PrimaryKeys(keys);

                if (pk.Length == 0)
                {
                    dt.PrimaryKey = new DataColumn[] { dt.Columns[0] };
                    cout.WriteLine($"no primary key found on Table: \"{dt.TableName}\"");
                }

                dt.PrimaryKey = pk;
            }


            TheClassBuilder gen = null;
            if (version == 0)
                gen = new DataContractClassBuilder(cmd, tnd.Name, dt, allowDbNull);
            else if (version == 1)
                gen = new DataContract1ClassBuilder(cmd, tnd.Name, dt, allowDbNull);
            else
                gen = new DataContract2ClassBuilder(cmd, tnd.Name, dt, allowDbNull);

            if (gen != null)
            {
                string path = cmd.OutputPath(ConfigKey._GENERATOR_DC_PATH, $"{ConfigurationEnvironment.MyDocuments}\\dc");
                string ns = cmd.GetValue("ns", ConfigKey._GENERATOR_DC_NS, "Sys.DataModel.DataContract");
                string mtd = cmd.GetValue("method");

                gen.SetNamespace(ns);
                gen.SetClassName(dt.TableName);
                gen.SetMethod(mtd);
                string file = gen.WriteFile(path);
                cout.WriteLine("code generated on {0}", file);
            }

            TableSchemaCache.Clear();
        }

        public void ExportEntityClass()
        {
            if (dname == null)
            {
                cerr.WriteLine("select a database first");
                return;
            }

            string path = cmd.OutputPath(ConfigKey._GENERATOR_DC_PATH, $"{ConfigurationEnvironment.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns", ConfigKey._GENERATOR_DC_NS, "Sys.DataModel.DataContracts");

            if (tname != null)
            {
                cout.WriteLine("start to generate {0} entity framework class file", tname);
                var builder = new EntityClassBuilder(cmd, tname)
                {
                };
                builder.SetNamespace(ns);
                if (!builder.IsAssocication)
                {
                    string file = builder.WriteFile(path);
                    cout.WriteLine("completed {0} => {1}", tname.ShortName, file);
                }
            }
            else if (dname != null)
            {
                cout.WriteLine("start to generate {0} entity framework class to directory: {1}", dname, path);
                CancelableWork.CanCancel(cts =>
                {
                    var md = new MatchedDatabase(dname, cmd); //cfg.exportExcludedTables);
                    TableName[] tnames = md.TableNames();
                    foreach (var tn in tnames)
                    {
                        if (cts.IsCancellationRequested)
                            return;

                        try
                        {
                            var builder = new EntityClassBuilder(cmd, tn);
                            builder.SetNamespace(ns);
                            if (!builder.IsAssocication)
                            {
                                string file = builder.WriteFile(path);
                                cout.WriteLine("generated for {0} at {1}", tn.ShortName, path);
                            }
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
            string path = cmd.OutputPath(ConfigKey._GENERATOR_L2S_PATH, $"{ConfigurationEnvironment.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns", ConfigKey._GENERATOR_L2S_NS, "Sys.DataModel.L2s");

            if (tname != null)
            {
                var builder = new Linq2SQLClassBuilder(cmd, tname)
                {
                };
                builder.SetNamespace(ns);

                string file = builder.WriteFile(path);
                cout.WriteLine("code generated on {0}", file);
            }
            else if (dname != null)
            {

                TableName[] tnames = getTableNames(cmd);
                foreach (var tname in tnames)
                {
                    var builder = new Linq2SQLClassBuilder(cmd, tname)
                    {
                    };
                    builder.SetNamespace(ns);

                    string file = builder.WriteFile(path);
                    cout.WriteLine("code generated on {0}", file);
                }
            }
            else
            {
                cerr.WriteLine("warning: table or database is not seleted");
            }

            TableSchemaCache.Clear();
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

            string ds_name = cmd.GetValue("ds-name");
            string[] dt_names = cmd.GetStringArray("dt-names");

            if (ds_name != null)
                ds.DataSetName = ds_name;

            if (dt_names != null)
            {
                int min = Math.Min(ds.Tables.Count, dt_names.Length);
                for (int i = 0; i < min; i++)
                    ds.Tables[i].TableName = dt_names[i];
            }

            JsonStyle style = cmd.GetEnum("style", JsonStyle.Normal);

            if (ds.Tables.Count == 1)
            {
                var dt = ds.Tables[0];
                string file = FileName($"{dt.TableName}.json");
                using (var writer = file.CreateStreamWriter(cmd.Append))
                {
                    bool excludeTableName = cmd.Has("exclude-table");
                    writer.WriteLine(dt.WriteJson(style, excludeTableName));
                    cout.WriteLine($"completed to generate json on file: \"{file}\"");
                }
            }
            else
            {
                string file = FileName($"{ds.DataSetName}.json");
                using (var writer = file.CreateStreamWriter(cmd.Append))
                {
                    writer.WriteLine(ds.WriteJson(style));
                    cout.WriteLine($"completed to generate json on file: \"{file}\"");
                }
            }

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

            string path = cmd.OutputPath(ConfigKey._GENERATOR_DS_PATH, $"{ConfigurationEnvironment.MyDocuments}\\ds");
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
                    TableName[] tnames = md.TableNames();
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


        private void ExportResourceData()
        {
            var dt = LastOrCurrentTable();

            ResourceFormat format = cmd.GetEnum("format", ResourceFormat.resx);
            string language = cmd.GetValue("language") ?? "en";
            string directory = cmd.OutputDirectory() ?? ".";
            string name_column = cmd.GetValue("name-column");
            string value_column = cmd.GetValue("value-column") ?? name_column;
            bool append = cmd.Has("append");

            if (string.IsNullOrEmpty(name_column))
            {
                cerr.WriteLine("name-column is undefined");
                return;
            }

            if (!dt.Columns.Contains(name_column))
            {
                cerr.WriteLine($"name-column doesn't exist: {name_column}");
                return;
            }

            if (!dt.Columns.Contains(value_column))
            {
                cerr.WriteLine($"value-column doesn't exist: {value_column}");
                return;
            }

            Locale locale = new Locale
            {
                Format = format,
                Append = append,
            };

            //load entries from database
            locale.LoadEntries(dt, name_column, value_column);
            var file = locale.GetResourceFile(language, directory);
            int count = locale.Update(file);
            string _append = append ? "appended" : "updated";

            cout.WriteLine($"{count} of entries {_append} on \"{file}\"");
        }

        public static void Help()
        {
            cout.WriteLine("export data, schema, class, and template on current selected server/db/table");
            cout.WriteLine("option:");
            cout.WriteLine("   /out:xxx : output path or file name");
            cout.WriteLine("option of SQL generation:");
            cout.WriteLine("   /INSERT  : export data in INSERT INTO script on current table/database");
            cout.WriteLine("   /UPDATE  : export data in UPDATE SET script on current table/database");
            cout.WriteLine("   /SAVE    : export data in IF NOT EXISTS INSERT ELSE UPDATE script on current table/database");
            cout.WriteLine("   [/if]    : option /if generate if exists row then UPDATE else INSERT; or check existence of table when drop table");
            cout.WriteLine("   /create  : generate CREATE TABLE script on current table/database");
            cout.WriteLine("   /select  : generate template SELECT FROM WHERE");
            cout.WriteLine("   /insert  : generate template INSERT INTO");
            cout.WriteLine("   /update  : generate template UPDATE SET WHERE");
            cout.WriteLine("   /save    : generate template IF EXISTS UPDATE ELSE INSERT");
            cout.WriteLine("   /delete  : generate template DELETE FROM WHERE, delete rows with foreign keys constraints");
            cout.WriteLine("   /drop    : generate template DROP TABLE, drop tables with foreign keys constraints");
            cout.WriteLine("option of data generation:");
            cout.WriteLine("   /schema  : generate database schema xml file");
            cout.WriteLine("   /data    : generate database/table data xml file");
            cout.WriteLine("      [/include]: include table names with wildcard");
            cout.WriteLine("   /csv     : generate table csv file");
            cout.WriteLine("   /ds      : generate data set xml file");
            cout.WriteLine("   /json    : generate json from last result");
            cout.WriteLine("      [/ds-name:]     : data set name");
            cout.WriteLine("      [/dt-names:  ]  : data table name list");
            cout.WriteLine("      [/style:]       : json style: normal|extended|coded");
            cout.WriteLine("      [/exclude-table]: exclude table name in json");
            cout.WriteLine("   /resource: generate i18n resource file from last result");
            cout.WriteLine("      [/format:]      : resource format: resx|xlf|json, default:resx");
            cout.WriteLine("      [/name-column:] : name column");
            cout.WriteLine("      [/value-column:]: value column");
            cout.WriteLine("      [/language:]    : language: en|es|..., default:en");
            cout.WriteLine("      [/out:]         : resource file directory, default: current working directory");
            cout.WriteLine("      [/append]       : update or append to resource file");
            cout.WriteLine("option of code generation:");
            cout.WriteLine("   /dpo     : generate C# table class");
            cout.WriteLine("   /l2s     : generate C# Linq to SQL class");
            cout.WriteLine("      [/code-style]: orginal|pascal|camel");
            cout.WriteLine("   /dc      : generate C# data contract class");
            cout.WriteLine("   /dc1     : generate C# data contract class and extension class");
            cout.WriteLine("      /fk   : create foreign key constraint");
            cout.WriteLine("      /assoc: create association classes");
            cout.WriteLine("      [/methods:NewObject,FillObject,UpdateRow,CreateTable,ToDataTable,ToDictionary,FromDictionary,CopyTo,CompareTo,ToSimpleString]");
            cout.WriteLine("   /dc2     : generate C# data contract class and extension class");
            cout.WriteLine("      option of data contract /[dc|dc1|dc2] :");
            cout.WriteLine("      [/readonly]: contract class for reading only");
            cout.WriteLine("      [/last]: generate C# data contract from last result");
            cout.WriteLine("      [/method:name] default convert method is defined on the .cfg");
            cout.WriteLine("      [/methods:NewObject,FillObject,UpdateRow,Equals,CopyTo,CreateTable,ToString]");
            cout.WriteLine("      [/NULL] allow column type be nullable");
            cout.WriteLine("      [/col:pk1,pk2] default primary key is the first column");
            cout.WriteLine("   /entity  : generate C# method copy/compare/clone for Entity framework");
            cout.WriteLine("      [/base:type] define base class or interface, use ~ to represent generic class itself, delimited by ;");
            cout.WriteLine("      [/field:constMap] create const fields for name of columns");
            cout.WriteLine("      [/methods:Map,Copy,Equals,Clone,GetHashCode,ToString] create Copy,Equals,Clone,GetHashCode, and ToString method");
            cout.WriteLine("   /c#      : generate C# data from last result");
            cout.WriteLine("      [/type:dict|list|array|enum|const] data type, default is list");
            cout.WriteLine("      [/code-column:col1=usertype1;col2=usertyp2] define user type for columns");
            cout.WriteLine("      [/field:col1,col2] const filed name");
            cout.WriteLine("      [/value:col1,col2] const filed value");
            cout.WriteLine("      [/dataclass] data-class name, default is DbReadOnly");
            cout.WriteLine("      [/dataonly] create data only");
            cout.WriteLine("      [/classonly] create class only");
            cout.WriteLine("   /conf    : generate Config C# class");
            cout.WriteLine("      [/type:k|d|f|p|F|P] C# class type, default is kdP");
            cout.WriteLine("          k : generate class of const key");
            cout.WriteLine("          d : generate class of default value");
            cout.WriteLine("          P : generate class of static property");
            cout.WriteLine("          F : generate class of static field");
            cout.WriteLine("          M : generate class of static method");
            cout.WriteLine("          p : generate class of hierarchial property");
            cout.WriteLine("          f : generate class of hierarchial field");
            cout.WriteLine("          m : generate class of hierarchial method");
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
            else if (cmd.Has("resource"))
                ExportResourceData();
            else
                cerr.WriteLine("invalid command options");
        }
    }
}

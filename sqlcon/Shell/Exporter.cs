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

namespace SqlCon
{
    class Exporter
    {
        private readonly PathManager mgr;
        private readonly ApplicationCommand cmd;
        private readonly IApplicationConfiguration cfg;


        private readonly TableName tname;
        private readonly DatabaseName dname;
        private readonly ServerName sname;

        readonly XmlDbCreator xmlDbFile;
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

        private TableName[] GetTableNames(ApplicationCommand cmd)
        {
            TableName[] tnames;
            if (cmd.wildcard != null)
            {
                var md = new MatchedDatabase(dname, cmd);
                if (!cmd.IsView)
                    tnames = md.TableNames();
                else
                    tnames = md.ViewNames();

                if (tnames.Length == 0)
                {
                    Cerr.WriteLine("warning: no table is matched");
                    return new TableName[] { };
                }
            }
            else
            {
                if (!cmd.IsView)
                    tnames = dname.GetTableNames();
                else
                    tnames = dname.GetViewNames();
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

                //because string supports Unicode
                if (column.CType == CType.NVarChar || column.CType == CType.NChar)
                {
                    if (column.Length > 0)
                        col.MaxLength = column.Length / 2;
                }
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
                    Cout.WriteLine(sql);
                    writer.WriteLine(sql);
                }
            }
            else
            {
                Cerr.WriteLine("warning: table is not selected");
            }
        }


        public void ExportCreate()
        {
            if (tname != null)
            {
                Cout.WriteLine("start to generate CREATE TABLE script: {0}", dname);
                using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
                {
                    writer.WriteLine(tname.GenerateIfDropClause());
                    writer.WriteLine(tname.GenerateCreateTableClause(appendGO: true));
                }
                Cout.WriteLine("completed to generate script on file: {0}", SqlFileName);
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
                            Cout.WriteLine("start to generate CREATE TABLE script: {0} ", tname);
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
                        Cerr.WriteLine("warning: no table is matched");
                        return;
                    }
                }
                else
                {
                    Cout.WriteLine("start to generate CREATE TABLE script: {0}", dname);
                    using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
                    {
                        writer.WriteLine(dname.GenerateClause());
                    }
                }

                Cout.WriteLine("completed to generate script on file: {0}", SqlFileName);
                return;
            }

            Cerr.WriteLine("warning: table or database is not seleted");
        }

        public void ExportInsertOrUpdateData(SqlScriptType type)
        {
            var option = new SqlScriptGenerationOption
            {
                HasIfExists = cmd.HasIfExists,
                InsertWithoutColumns = cmd.Has("no-columns"),
                IncludeIdentity = cmd.Has("identity"),
            };

            if (tname != null)
            {
                var node = mgr.GetCurrentNode<Locator>();
                int count = 0;
                using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
                {
                    //cout.WriteLine($"start to generate {tname} script to file: \"{SqlFileName}\"");
                    Locator locator = null;
                    string WHERE = "";
                    if (node != null)
                    {
                        locator = mgr.GetCombinedLocator(node);
                        WHERE = $" WHERE {locator}";
                    }

                    long cnt = tname.GetTableRowCount(locator);
                    count = Tools.ForceLongToInteger(cnt);
                    using (var progress = new ProgressBar { Count = count })
                    {
                        count = Compare.GenerateRows(type, writer, new TableSchema(tname), locator, option, progress);
                    }
                    Cout.WriteLine($"{type} clauses (SELECT * FROM {tname}{WHERE}) generated to \"{SqlFileName}\", Done on rows({cnt})");
                }
            }
            else if (dname != null)
            {
                //cout.WriteLine($"start to generate {dname} script to file: \"{SqlFileName}\"");
                using (var writer = SqlFileName.CreateStreamWriter(cmd.Append))
                {
                    var md = new MatchedDatabase(dname, cmd);
                    TableName[] tnames = md.TableNames();

                    if (tnames.Length > 5 && !Cin.YesOrNo($"Are you sure to export {tnames.Length} tables on {dname} (y/n)?"))
                        return;

                    CancelableWork.CanCancel(cts =>
                    {
                        foreach (var tn in tnames)
                        {
                            if (cts.IsCancellationRequested)
                                return;

                            long cnt = tn.GetTableRowCount();
                            if (cnt > cfg.MaxRows)
                            {
                                if (!Cin.YesOrNo($"Are you sure to export {cnt} rows on {tn.ShortName} (y/n)?"))
                                {
                                    Cout.WriteLine("\n{0,10} skipped", tn.ShortName);
                                    continue;
                                }
                            }

                            int count = Tools.ForceLongToInteger(cnt);
                            using (var progress = new ProgressBar { Count = count })
                            {
                                count = Compare.GenerateRows(type, writer, new TableSchema(tn), null, option, progress);
                            }

                            Cout.WriteLine($"{count,10} row(s) generated on {tn.ShortName}");
                        }

                        Cout.WriteLine($"completed to generate {type} clauses to \"{SqlFileName}\"");

                    });
                }
            }
            else
                Cerr.WriteLine("warning: table or database is not selected");
        }

        public void ExportSchema()
        {
            string directory = cmd.OutputDirectory();
            if (directory != null)
                xmlDbFile.XmlDbFolder = directory;

            if (dname != null)
            {
                Cout.WriteLine("start to generate database schema {0}", dname);
                var file = xmlDbFile.WriteSchema(dname);
                Cout.WriteLine("completed {0}", file);
            }
            else if (sname != null)
            {
                if (sname != null)
                {
                    Cout.WriteLine("start to generate server schema {0}", sname);
                    var file = xmlDbFile.WriteSchema(sname);
                    Cout.WriteLine("completed {0}", file);
                }
                else
                    Cerr.WriteLine("warning: server or database is not selected");
            }
        }

        public void ExportData()
        {
            string directory = cmd.OutputDirectory();
            if (directory != null)
                xmlDbFile.XmlDbFolder = directory;

            if (tname != null)
            {
                Cout.WriteLine("start to generate {0} data file", tname);
                var dt = new TableReader(tname).Table;
                var file = xmlDbFile.WriteData(tname, dt);
                Cout.WriteLine("completed {0} =>{1}", tname.ShortName, file);
            }

            else if (dname != null)
            {
                Cout.WriteLine("start to generate {0}", dname);
                var mt = new MatchedDatabase(dname, cmd);
                CancelableWork.CanCancel(cts =>
                {
                    foreach (var tname in mt.TableNames())
                    {
                        if (cts.IsCancellationRequested)
                            return;


                        Cout.WriteLine("start to generate {0}", tname);
                        var dt = new SqlBuilder().SELECT().TOP(cmd.Top).COLUMNS().FROM(tname).SqlCmd.FillDataTable();
                        var file = xmlDbFile.WriteData(tname, dt);
                        Cout.WriteLine("completed {0} => {1}", tname.ShortName, file);
                    }
                    return;
                }
               );

                if (cmd.Top == 0)
                    Cout.WriteLine("completed");
                else
                    Cout.WriteLine("completed to export TOP {0} row(s) for each table", cmd.Top);
            }
            else
            {
                Cerr.WriteLine("warning: table or database is not seleted");
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
                Cout.WriteLine("generated class {0} at {1}", tname.ShortName, option.OutputPath);
            }
            else if (dname != null)
            {
                Cout.WriteLine("start to generate database {0} class to directory: {1}", dname, option.OutputPath);
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
                            Cout.WriteLine("generated class for {0} at {1}", tn.ShortName, option.OutputPath);
                        }
                        catch (Exception ex)
                        {
                            Cerr.WriteLine($"failed to generate class {tn.ShortName}, {ex.Message}");
                        }
                    }

                    Cout.WriteLine("completed");
                    return;
                });
            }
            else
            {
                Cerr.WriteLine("warning: database is not selected");
            }

        }

        public void ExportCsvFile()
        {
            string path = this.cmd.OutputPath(ConfigKey._GENERATOR_CSV_PATH, $"{ConfigurationEnvironment.MyDocuments}\\csv");

            string file;
            string fullName(TableName tname) => $"{path}\\{sname.Path}\\{dname.Name}\\{tname.ShortName}.csv";

            if (tname != null)
            {
                Cout.WriteLine("start to generate {0} csv file", tname);
                file = this.cmd.OutputFileName();
                if (file == null)
                    file = fullName(tname);

                var dt = new SqlBuilder().SELECT().COLUMNS(cmd.Columns).FROM(tname).SqlCmd.FillDataTable();
                using (var writer = file.CreateStreamWriter(cmd.Append))
                {
                    CsvFile.Write(dt, writer, true);
                }
                Cout.WriteLine("completed {0} => {1}", tname.ShortName, file);
            }
            else if (dname != null)
            {
                Cout.WriteLine("start to generate {0} csv to directory: {1}", dname, path);
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
                            Cout.WriteLine("generated for {0} at {1}", tn.ShortName, path);
                        }
                        catch (Exception ex)
                        {
                            Cerr.WriteLine($"failed to generate {tn.ShortName}, {ex.Message}");
                        }
                    }

                    Cout.WriteLine("completed");
                    return;
                });
            }
            else
            {
                Cerr.WriteLine("warning: table or database is not seleted");
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
                TableName[] tnames = GetTableNames(cmd);
                foreach (var tn in tnames)
                {
                    var dt = FillTable(tn);
                    list.Add(new TableNameData { Name = tn, Data = dt });
                }
            }
            else
            {
                Cerr.WriteLine("data table cannot find, use command type or select first");
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
                    Cout.WriteLine($"no primary key found on Table: \"{dt.TableName}\"");
                }

                dt.PrimaryKey = pk;
            }


            TheClassBuilder gen;
            if (version == 0)
                gen = new DataContractClassBuilder(cmd, tnd.Name, dt, allowDbNull);
            else if (version == 1)
                gen = new DataContract1ClassBuilder(cmd, tnd.Name, dt, allowDbNull);
            else if (version == 2)
                gen = new DataContract2ClassBuilder(cmd, tnd.Name, dt, allowDbNull);
            else
                gen = new ViewModelClassBuilder(cmd, tnd.Name, dt, allowDbNull);

            if (gen != null)
            {
                string path = cmd.OutputPath(ConfigKey._GENERATOR_DC_PATH, $"{ConfigurationEnvironment.MyDocuments}\\dc");
                string ns = cmd.GetValue("ns", ConfigKey._GENERATOR_DC_NS, "Sys.DataModel.DataContract");
                string mtd = cmd.GetValue("method");

                gen.SetNamespace(ns);
                gen.SetClassName(dt.TableName);
                gen.SetMethod(mtd);
                string file = gen.WriteFile(path);
                Cout.WriteLine("code generated on {0}", file);
            }

            TableSchemaCache.Clear();
        }

        public void ExportEntityClass()
        {
            if (dname == null)
            {
                Cerr.WriteLine("select a database first");
                return;
            }

            string path = cmd.OutputPath(ConfigKey._GENERATOR_DC_PATH, $"{ConfigurationEnvironment.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns", ConfigKey._GENERATOR_DC_NS, "Sys.DataModel.DataContracts");

            if (tname != null)
            {
                Cout.WriteLine("start to generate {0} entity framework class file", tname);
                var builder = new EntityClassBuilder(cmd, tname)
                {
                };
                builder.SetNamespace(ns);
                if (!builder.IsAssocication)
                {
                    string file = builder.WriteFile(path);
                    Cout.WriteLine("completed {0} => {1}", tname.ShortName, file);
                }
            }
            else if (dname != null)
            {
                Cout.WriteLine("start to generate {0} entity framework class to directory: {1}", dname, path);
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
                                Cout.WriteLine("generated for {0} at {1}", tn.ShortName, path);
                            }
                        }
                        catch (Exception ex)
                        {
                            Cerr.WriteLine($"failed to generate {tn.ShortName}, {ex.Message}");
                        }
                    }

                    Cout.WriteLine("completed");
                    return;
                });
            }
            else
            {
                Cerr.WriteLine("warning: table or database is not seleted");
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
                Cout.WriteLine("code generated on {0}", file);
            }
            else if (dname != null)
            {

                TableName[] tnames = GetTableNames(cmd);
                foreach (var tname in tnames)
                {
                    var builder = new Linq2SQLClassBuilder(cmd, tname)
                    {
                    };
                    builder.SetNamespace(ns);

                    string file = builder.WriteFile(path);
                    Cout.WriteLine("code generated on {0}", file);
                }
            }
            else
            {
                Cerr.WriteLine("warning: table or database is not seleted");
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
            {
                Cout.WriteLine("Cannot find last or current data set.");
                return;
            }

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
                    Cout.WriteLine($"completed to generate json on file: \"{file}\"");
                }
            }
            else
            {
                string file = FileName($"{ds.DataSetName}.json");
                using (var writer = file.CreateStreamWriter(cmd.Append))
                {
                    writer.WriteLine(ds.WriteJson(style));
                    Cout.WriteLine($"completed to generate json on file: \"{file}\"");
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
                Cerr.WriteLine("select a database first");
                return;
            }

            string path = cmd.OutputPath(ConfigKey._GENERATOR_DS_PATH, $"{ConfigurationEnvironment.MyDocuments}\\ds");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (tname != null)
            {
                Cout.WriteLine($"start to generate data file: {tname}");
                var dt = new TableReader(tname).Table;
                dt.TableName = tname.ShortName;

                string file = Path.Combine(path, $"{tname.ShortName}.xml");
                DataSet ds = dt.DataSet;
                ds.DataSetName = dname.Name;

                ds.WriteXml(file, XmlWriteMode.WriteSchema);
                Cout.WriteLine($"completed {tname} => {file}");
            }
            else if (dname != null)
            {
                Cout.WriteLine($"start to generate data file to directory: {dname}");
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
                            Cout.WriteLine($"generated for {tn.ShortName}");
                        }
                        catch (Exception ex)
                        {
                            Cerr.WriteLine($"failed to generate {tn.ShortName}, {ex.Message}");
                        }
                    }

                    string file = Path.Combine(path, $"{dname.Name}.xml");
                    ds.WriteXml(file, XmlWriteMode.WriteSchema);
                    Cout.WriteLine($"completed generated: {file}");
                    return;
                });
            }
            else
            {
                Cerr.WriteLine("warning: table or database is not seleted");
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
                Cerr.WriteLine("name-column is undefined");
                return;
            }

            if (!dt.Columns.Contains(name_column))
            {
                Cerr.WriteLine($"name-column doesn't exist: {name_column}");
                return;
            }

            if (!dt.Columns.Contains(value_column))
            {
                Cerr.WriteLine($"value-column doesn't exist: {value_column}");
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

            Cout.WriteLine($"{count} of entries {_append} on \"{file}\"");
        }

        public static void Help()
        {
            Cout.WriteLine("export data, schema, class, and template on current selected server/db/table");
            Cout.WriteLine("Option:");
            Cout.WriteLine("   /out:xxx : output path or file name");
            Cout.WriteLine("Option of SQL generation:");
            Cout.WriteLine("   /INSERT  : export data in INSERT INTO script on current table/database");
            Cout.WriteLine("   /UPDATE  : export data in UPDATE SET script on current table/database");
            Cout.WriteLine("   /SAVE    : export data in IF NOT EXISTS INSERT ELSE UPDATE script on current table/database");
            Cout.WriteLine("      [/if]           : option /if generate if exists row then UPDATE else INSERT; or check existence of table when drop table");
            Cout.WriteLine("      [/no-columns]   : no columns in INSERT INTO clause");
            Cout.WriteLine("   /create  : generate CREATE TABLE script on current table/database");
            Cout.WriteLine("   /select  : generate template SELECT FROM WHERE");
            Cout.WriteLine("   /insert  : generate template INSERT INTO");
            Cout.WriteLine("   /update  : generate template UPDATE SET WHERE");
            Cout.WriteLine("   /save    : generate template IF EXISTS UPDATE ELSE INSERT");
            Cout.WriteLine("   /delete  : generate template DELETE FROM WHERE, delete rows with foreign keys constraints");
            Cout.WriteLine("   /drop    : generate template DROP TABLE, drop tables with foreign keys constraints");
            Cout.WriteLine("Option of data generation:");
            Cout.WriteLine("   /schema  : generate database schema xml file");
            Cout.WriteLine("   /data    : generate database/table data xml file");
            Cout.WriteLine("      [/include]: include table names with wildcard");
            Cout.WriteLine("   /csv     : generate table csv file");
            Cout.WriteLine("   /ds      : generate data set xml file");
            Cout.WriteLine("   /json    : generate json from last result");
            Cout.WriteLine("      [/ds-name:]     : data set name");
            Cout.WriteLine("      [/dt-names:  ]  : data table name list");
            Cout.WriteLine("      [/style:]       : json style: normal|extended|coded");
            Cout.WriteLine("      [/exclude-table]: exclude table name in json");
            Cout.WriteLine("   /resource: generate i18n resource file from last result");
            Cout.WriteLine("      [/format:]      : resource format: resx|xlf|json, default:resx");
            Cout.WriteLine("      [/name-column:] : name column");
            Cout.WriteLine("      [/value-column:]: value column");
            Cout.WriteLine("      [/language:]    : language: en|es|..., default:en");
            Cout.WriteLine("      [/out:]         : resource file directory, default: current working directory");
            Cout.WriteLine("      [/append]       : update or append to resource file");
            Cout.WriteLine("Option of code generation:");
            Cout.WriteLine("   /dpo     : generate C# table class");
            Cout.WriteLine("   /l2s     : generate C# Linq to SQL class");
            Cout.WriteLine("      [/code-style]: orginal|pascal|camel");
            Cout.WriteLine("   /dc      : generate C# data contract class");
            Cout.WriteLine("   /dc1     : generate C# data contract class and extension class");
            Cout.WriteLine("      [/fk] : create foreign key constraint");
            Cout.WriteLine("      [/assoc]: create association classes");
            Cout.WriteLine("      [/data-column-property]: create data column property: AllowDbNull,MaxLength,Unique in CreateTable()");
            Cout.WriteLine("      [/methods:NewObject,FillObject,UpdateRow,CreateTable,ToDataTable,ToDictionary,FromDictionary,CopyTo,CompareTo,ToSimpleString]");
            Cout.WriteLine("   /dc2     : generate C# data contract class and extension class");
            Cout.WriteLine("   /vm      : generate C# data view model class");
            Cout.WriteLine("      option of data contract /[dc|dc1|dc2|vm] :");
            Cout.WriteLine("      [/readonly]: contract class for reading only");
            Cout.WriteLine("      [/last]: generate C# data contract from last result");
            Cout.WriteLine("      [/method:name] default convert method is defined on the .cfg");
            Cout.WriteLine("      [/methods:NewObject,FillObject,UpdateRow,Equals,CopyTo,CreateTable,ToString]");
            Cout.WriteLine("      [/NULL] allow column type be nullable");
            Cout.WriteLine("      [/col:pk1,pk2] default primary key is the first column");
            Cout.WriteLine("   /entity  : generate C# method copy/compare/clone for Entity framework");
            Cout.WriteLine("      [/base:type] define base class or interface, use ~ to represent generic class itself, delimited by ;");
            Cout.WriteLine("      [/field:constMap] create const fields for name of columns");
            Cout.WriteLine("      [/methods:Map,Copy,Equals,Clone,GetHashCode,ToString] create Copy,Equals,Clone,GetHashCode, and ToString method");
            Cout.WriteLine("   /c#      : generate C# data from last result");
            Cout.WriteLine("      [/type:dict|list|array|enum|const] data type, default is list");
            Cout.WriteLine("      [/code-column:col1=usertype1;col2=usertyp2] define user type for columns");
            Cout.WriteLine("      [/field:col1,col2] const filed name");
            Cout.WriteLine("      [/value:col1,col2] const filed value");
            Cout.WriteLine("      [/dataclass] data-class name, default is DbReadOnly");
            Cout.WriteLine("      [/dataonly] create data only");
            Cout.WriteLine("      [/classonly] create class only");
            Cout.WriteLine("   /conf    : generate Config C# class");
            Cout.WriteLine("      [/type:k|d|f|p|F|P] C# class type, default is kdP");
            Cout.WriteLine("          k : generate class of const key");
            Cout.WriteLine("          d : generate class of default value");
            Cout.WriteLine("          P : generate class of static property");
            Cout.WriteLine("          F : generate class of static field");
            Cout.WriteLine("          M : generate class of static method");
            Cout.WriteLine("          p : generate class of hierarchial property");
            Cout.WriteLine("          f : generate class of hierarchial field");
            Cout.WriteLine("          m : generate class of hierarchial method");
            Cout.WriteLine("          t : generate data contract classes");
            Cout.WriteLine("          j : generate data classes from JSON");
            Cout.WriteLine("      [/method:name] GetValue method name, default is \"GetValue<>\"");
            Cout.WriteLine("      [/key:column] column key, required");
            Cout.WriteLine("      [/default:column] column default value, required");
            Cout.WriteLine("      [/kc:name] class name of const key");
            Cout.WriteLine("      [/dc:name] class name of default value");
            Cout.WriteLine("   /cfg    : generate config file");
            Cout.WriteLine("      [/type:f|h] script type");
            Cout.WriteLine("          h : generate TIE hierarchial config script file");
            Cout.WriteLine("          f : generate TIE config script file");
            Cout.WriteLine("Common options");
            Cout.WriteLine("      [/view] operation in views rather than tables");
            Cout.WriteLine("Common options /conf and /cfg");
            Cout.WriteLine("      [/in:path] input path(.cfg)");
            Cout.WriteLine("      [/key:column] column of key on config table");
            Cout.WriteLine("      [/default:column] column of default value config table");
            Cout.WriteLine("Common options for code generation");
            Cout.WriteLine("      [/ns:name] default name space is defined on the .cfg");
            Cout.WriteLine("      [/class:name] default class name is defined on the .cfg");
            Cout.WriteLine("      [/using:assembly] allow the use of types in a namespace, delimited by ;");
            Cout.WriteLine("      [/out:path] output directory or file name (.cs)");
        }

        public void Run()
        {
            if (cmd.Has("INSERT"))
                ExportInsertOrUpdateData(SqlScriptType.INSERT);
            else if (cmd.Has("UPDATE"))
                ExportInsertOrUpdateData(SqlScriptType.UPDATE);
            else if (cmd.Has("SAVE") || cmd.Has("UPSERT"))
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
            else if (cmd.Has("save") || cmd.Has("upsert"))
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
            else if (cmd.Has("vm"))
                ExportDataContract(3);
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
                Cerr.WriteLine("invalid command options");
        }
    }
}

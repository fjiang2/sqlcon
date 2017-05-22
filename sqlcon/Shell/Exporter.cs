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
using Sys.CodeBuilder;
using Tie;

namespace sqlcon
{
    class Exporter
    {
        private PathManager mgr;
        private Configuration cfg;
        private string fileName;
        private TableName tname;
        private DatabaseName dname;
        private ServerName sname;

        XmlDbFile xml;
        public Exporter(PathManager mgr, TreeNode<IDataPath> pt, Configuration cfg)
        {
            this.mgr = mgr;
            this.cfg = cfg;
            this.xml = new XmlDbFile { XmlDbFolder = cfg.XmlDbFolder };
            this.fileName = cfg.OutputFile;
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

        private TableName[] getTableNames(Command cmd, string[] excludedTables = null)
        {
            TableName[] tnames;
            if (cmd.wildcard != null)
            {
                var md = new MatchedDatabase(dname, cmd.wildcard, excludedTables);
                tnames = md.MatchedTableNames;
                if (tnames.Length == 0)
                {
                    stdio.ErrorFormat("warning: no table is matched");
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
                    stdio.WriteLine(sql);
                    writer.WriteLine(sql);
                }
            }
            else
            {
                stdio.ErrorFormat("warning: table is not selected");
            }
        }


        public void ExportCreate(Command cmd)
        {
            if (tname != null)
            {
                stdio.WriteLine("start to generate CREATE TABLE script: {0}", dname);
                using (var writer = fileName.NewStreamWriter())
                {
                    writer.WriteLine(tname.GenerateCluase(true));
                }
                stdio.WriteLine("completed to generate script on file: {0}", fileName);
                return;
            }


            if (dname != null)
            {
                if (cmd.wildcard != null)
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, cfg.exportExcludedTables);
                    TableName[] tnames = md.MatchedTableNames;
                    if (tnames.Length > 0)
                    {
                        using (var writer = fileName.NewStreamWriter())
                        {
                            foreach (var tname in tnames)
                            {
                                stdio.WriteLine("start to generate CREATE TABLE script: {0} ", tname);
                                writer.WriteLine(tname.GenerateCluase(true));
                            }
                        }
                    }
                    else
                    {
                        stdio.ErrorFormat("warning: no table is matched");
                        return;
                    }
                }
                else
                {
                    stdio.WriteLine("start to generate CREATE TABLE script: {0}", dname);
                    using (var writer = fileName.NewStreamWriter())
                    {
                        writer.WriteLine(dname.GenerateClause());
                    }
                }

                stdio.WriteLine("completed to generate script on file: {0}", fileName);
                return;
            }

            stdio.ErrorFormat("warning: table or database is not seleted");
        }

        public void ExportInsert(Command cmd)
        {
            if (tname != null)
            {
                if (cmd.IsSchema)
                {
                    using (var writer = fileName.NewStreamWriter())
                    {
                        string sql = Compare.GenerateTemplate(new TableSchema(tname), SqlScriptType.INSERT);
                        stdio.WriteLine(sql);
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
                            stdio.WriteLine("start to generate {0} INSERT script to file: {1}", tname, fileName);
                            Locator locator = mgr.GetCombinedLocator(node);
                            count = Compare.GenerateRows(writer, new TableSchema(tname), locator, cmd.HasIfExists);
                            stdio.WriteLine("insert clauses (SELECT * FROM {0} WHERE {1}) generated to {2}", tname, locator, fileName);
                        }
                        else
                        {
                            count = Compare.GenerateRows(writer, new TableSchema(tname), null, cmd.HasIfExists);
                            stdio.WriteLine("insert clauses (SELECT * FROM {0}) generated to {1}", tname, fileName);
                        }
                    }
                }
            }
            else if (dname != null)
            {
                stdio.WriteLine("start to generate {0} script to file: {1}", dname, fileName);
                using (var writer = fileName.NewStreamWriter())
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, cfg.exportExcludedTables);
                    TableName[] tnames = md.MatchedTableNames;
                    CancelableWork.CanCancel(cts =>
                    {
                        foreach (var tn in tnames)
                        {
                            if (cts.IsCancellationRequested)
                                return;

                            if (!cfg.exportExcludedTables.IsMatch(tn.ShortName))
                            {
                                int count = new SqlCmd(tn.Provider, string.Format("SELECT COUNT(*) FROM {0}", tn)).FillObject<int>();
                                if (count > cfg.Export_Max_Count)
                                {
                                    if (!stdio.YesOrNo("are you sure to export {0} rows on {1} (y/n)?", count, tn.ShortName))
                                    {
                                        stdio.WriteLine("\n{0,10} skipped", tn.ShortName);
                                        continue;
                                    }
                                }

                                count = Compare.GenerateRows(writer, new TableSchema(tn), null, cmd.HasIfExists);
                                stdio.WriteLine("{0,10} row(s) generated on {1}", count, tn.ShortName);
                            }
                            else
                                stdio.WriteLine("{0,10} skipped", tn.ShortName);
                        }
                        stdio.WriteLine("completed");

                    });
                }
            }
            else
                stdio.ErrorFormat("warning: table or database is not selected");
        }

        public void ExportSchema()
        {
            if (dname != null)
            {
                stdio.WriteLine("start to generate database schema {0}", dname);
                var file = xml.WriteSchema(dname);
                stdio.WriteLine("completed {0}", file);
            }
            else if (sname != null)
            {
                if (sname != null)
                {
                    stdio.WriteLine("start to generate server schema {0}", sname);
                    var file = xml.WriteSchema(sname);
                    stdio.WriteLine("completed {0}", file);
                }
                else
                    stdio.ErrorFormat("warning: server or database is not selected");
            }
        }

        public void ExportData(Command cmd)
        {
            if (tname != null)
            {
                stdio.WriteLine("start to generate {0} data file", tname);
                var dt = new TableReader(tname).Table;
                var file = xml.Write(tname, dt);
                stdio.WriteLine("completed {0} =>{1}", tname.ShortName, file);
            }

            else if (dname != null)
            {
                stdio.WriteLine("start to generate {0}", dname);
                var mt = new MatchedDatabase(dname, cmd.wildcard, cfg.exportExcludedTables);
                CancelableWork.CanCancel(cts =>
                {
                    foreach (var tname in mt.MatchedTableNames)
                    {
                        if (cts.IsCancellationRequested)
                            return;


                        stdio.WriteLine("start to generate {0}", tname);
                        var dt = new SqlBuilder().SELECT.TOP(cmd.top).COLUMNS().FROM(tname).SqlCmd.FillDataTable();
                        var file = xml.Write(tname, dt);
                        stdio.WriteLine("completed {0} => {1}", tname.ShortName, file);
                    }
                    return;
                }
               );

                if (cmd.top == 0)
                    stdio.WriteLine("completed");
                else
                    stdio.WriteLine("completed to export TOP {0} row(s) for each table", cmd.top);
            }
            else
            {
                stdio.ErrorFormat("warning: table or database is not seleted");
            }
        }

        public void ExportClass(Command cmd)
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
                stdio.WriteLine("generated class {0} at {1}", tname.ShortName, option.OutputPath);
            }
            else if (dname != null)
            {
                stdio.WriteLine("start to generate database {0} class to directory: {1}", dname, option.OutputPath);
                CancelableWork.CanCancel(cts =>
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, cfg.exportExcludedTables);
                    TableName[] tnames = md.MatchedTableNames;
                    foreach (var tn in tnames)
                    {
                        if (cts.IsCancellationRequested)
                            return;


                        try
                        {
                            var clss = new DpoGenerator(tn) { Option = option };
                            clss.CreateClass();
                            stdio.WriteLine("generated class for {0} at {1}", tn.ShortName, option.OutputPath);
                        }
                        catch (Exception ex)
                        {
                            stdio.ErrorFormat("failed to generate class {0}, {1}", tn.ShortName, ex.Message);
                        }
                    }

                    stdio.WriteLine("completed");
                    return;
                });
            }
            else
            {
                stdio.ErrorFormat("warning: database is not selected");
            }

        }

        public void ExportCsvFile(Command cmd)
        {
            string path = cfg.GetValue<string>("csv.path", $"{Configuration.MyDocuments}\\csv");

            string file;
            Func<TableName, string> fullName = tname => $"{path}\\{sname.Path}\\{dname.Name}\\{tname.ShortName}.csv";

            if (tname != null)
            {
                stdio.WriteLine("start to generate {0} csv file", tname);
                file = fullName(tname);
                var dt = new SqlBuilder().SELECT.COLUMNS(cmd.Columns).FROM(tname).SqlCmd.FillDataTable();
                using (var writer = file.NewStreamWriter())
                {
                    CsvFile.Write(dt, writer, true);
                }
                stdio.WriteLine("completed {0} => {1}", tname.ShortName, file);
            }
            else if (dname != null)
            {
                stdio.WriteLine("start to generate {0} csv to directory: {1}", dname, path);
                CancelableWork.CanCancel(cts =>
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, cfg.exportExcludedTables);
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
                            stdio.WriteLine("generated for {0} at {1}", tn.ShortName, path);
                        }
                        catch (Exception ex)
                        {
                            stdio.ErrorFormat("failed to generate {0}, {1}", tn.ShortName, ex.Message);
                        }
                    }

                    stdio.WriteLine("completed");
                    return;
                });
            }
            else
            {
                stdio.ErrorFormat("warning: table or database is not seleted");
            }
        }

        public void ExportDataContract(Command cmd, int version)
        {
            DataTable dt = ShellHistory.LastTable();
            string ns = cmd.GetValue("ns");
            string clss = cmd.GetValue("class");
            string path = cmd.GetValue("out") ?? cfg.GetValue<string>("dc.path", $"{Configuration.MyDocuments}\\dc");

            if (dt != null)
            {
                ns = ns ?? cfg.GetValue<string>("dc.ns", "Sys.DataModel.DataContract");
                clss = clss ?? cfg.GetValue<string>("dc.class", "DataContract");
                ExportDataContractClass(cmd, path, version, dt, ns, clss);
            }
            else
            {
                if (tname != null)
                {
                    dt = new SqlCmd(tname.Provider, $"SELECT TOP 1 * FROM {tname.FormalName}").FillDataTable();
                    ExportDataContractClass(cmd, path, version, dt, ns ?? $"Sys.DataModel.{tname.DatabaseName.Name}", clss ?? tname.ShortName);
                }
                else if (dname != null)
                {
                    path = path + "\\" + dname.Name;
                    TableName[] tnames = getTableNames(cmd);
                    foreach (var tn in tnames)
                    {
                        dt = new SqlCmd(tn.Provider, $"SELECT TOP 1 * FROM {tn.FormalName}").FillDataTable();
                        ExportDataContractClass(cmd, path, version, dt, ns ?? $"Sys.DataModel.{dname.Name}", tn.ShortName);
                    }
                }
                else
                {
                    stdio.ErrorFormat("data table cannot find, use command type or select first");
                    return;
                }
            }
        }

        private void ExportDataContractClass(Command cmd, string path, int version, DataTable dt, string ns, string className)
        {

            string mtd = cmd.GetValue("method");
            string[] keys = cmd.Columns;

            if (version == 1)
            {
                var builder = new DataContractClassBuilder(ns, cmd, dt)
                {
                    cname = className,
                    mtd = mtd,
                    keys = keys
                };

                string file = builder.WriteFile(path);
                stdio.WriteLine("code generated on {0}", file);
            }
            else
            {
                var builder = new DataContract2ClassBuilder(ns, cmd, dt)
                {
                    cname = className,
                    mtd = mtd
                };

                string file = builder.WriteFile(path);
                stdio.WriteLine("code generated on {0}", file);
            }
        }

        public void ExportEntityClass(Command cmd)
        {
            if (dname == null)
            {
                stdio.ErrorFormat("select a database first");
                return;
            }

            string path = cmd.GetValue("out") ?? cfg.GetValue<string>("dc.path", $"{Configuration.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns") ?? cfg.GetValue<string>("dc.ns", "Sys.DataModel.DataContracts");

            if (tname != null)
            {
                stdio.WriteLine("start to generate {0} entity framework class file", tname);
                var builder = new EntityClassBuilder(ns, cmd, tname);

                string file = builder.WriteFile(path);
                stdio.WriteLine("completed {0} => {1}", tname.ShortName, file);
            }
            else if (dname != null)
            {
                stdio.WriteLine("start to generate {0} entity framework class to directory: {1}", dname, path);
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
                            var builder = new EntityClassBuilder(ns, cmd, tn);

                            string file = builder.WriteFile(path);
                            stdio.WriteLine("generated for {0} at {1}", tn.ShortName, path);
                        }
                        catch (Exception ex)
                        {
                            stdio.ErrorFormat("failed to generate {0}, {1}", tn.ShortName, ex.Message);
                        }
                    }

                    stdio.WriteLine("completed");
                    return;
                });
            }
            else
            {
                stdio.ErrorFormat("warning: table or database is not seleted");
            }

        }



        public void ExportLinq2SQLClass(Command cmd)
        {
            string path = cfg.GetValue<string>("l2s.path", $"{Configuration.MyDocuments}\\dc");
            string ns = cmd.GetValue("ns") ?? cfg.GetValue<string>("l2s.ns", "Sys.DataModel.L2s");
            Dictionary<TableName, TableSchema> schemas = new Dictionary<TableName, TableSchema>();

            if (tname != null)
            {
                var builder = new Linq2SQLClassBuilder(ns, cmd, tname, schemas);
                string file = builder.WriteFile(path);
                stdio.WriteLine("code generated on {0}", file);
            }
            else if (dname != null)
            {

                TableName[] tnames = getTableNames(cmd);
                foreach (var tname in tnames)
                {
                    var builder = new Linq2SQLClassBuilder(ns, cmd, tname, schemas);
                    string file = builder.WriteFile(path);
                    stdio.WriteLine("code generated on {0}", file);
                }

                return;
            }
            else
            {
                stdio.ErrorFormat("warning: table or database is not seleted");
            }
        }

        public void ExportEnum(Command cmd)
        {

            DataTable dt = ShellHistory.LastTable();

            if (dt == null)
            {
                stdio.ErrorFormat("data table cannot find, use command type or select first");
                return;
            }

            string path = cfg.GetValue<string>("de.path", $"{Configuration.MyDocuments}\\DataModel\\DataEnum");
            string ns = cmd.GetValue("ns") ?? cfg.GetValue<string>("de.ns", "Sys.DataModel.DataEnum");

            CSharpBuilder builder = new CSharpBuilder()
            {
                nameSpace = ns
            };
            builder.AddUsing("Sys.Data");

            var rows = dt
                .AsEnumerable()
                .Select(row => new
                {
                    Category = row.Field<string>("Category"),
                    Feature = row.Field<string>("Feature"),
                    Value = row.Field<int>("Value"),
                    Label = row.Field<string>("Label")
                });

            var groups = rows.GroupBy(row => row.Category);

            foreach (var group in groups)
            {
                var _enum = new Sys.CodeBuilder.Enum(group.First().Category);
                foreach (var row in group)
                    _enum.Add(row.Feature, row.Value, row.Label);

                builder.AddEnum(_enum);
            }


            string filename = "DataEnum";

            string code = builder.ToString();
            string file = Path.ChangeExtension(Path.Combine(path, filename), "cs");
            code.WriteIntoFile(file);
            stdio.WriteLine("code generated on {0}", file);

        }


        /// <summary>
        /// create C# data from data table
        /// </summary>
        /// <param name="cmd"></param>
        public void ExportCSharpData(Command cmd)
        {
            var dt = ShellHistory.LastTable();
            if (dt == null)
            {
                stdio.ErrorFormat("display data table first by sql clause or command [type]");
                return;
            }

            string ns = cmd.GetValue("ns") ?? "Sql.Data";
            string cname = cmd.GetValue("class") ?? "Table";


            var builder = new CSharpBuilder { nameSpace = ns };
            var clss = new Class(cname)
            {
                modifier = Modifier.Public | Modifier.Partial
            };

            builder.AddClass(clss);

            Property prop;
            foreach (DataColumn column in dt.Columns)
            {
                bool nullable = dt.AsEnumerable().Any(row => row[column] is DBNull);
                TypeInfo ty = new TypeInfo(column.DataType) { Nullable = nullable };

                prop = new Property(ty, column.ColumnName.ToFieldName()) { modifier = Modifier.Public };
                clss.Add(prop);
            }

            clss = new Class("DbReadOnly")
            {
                modifier = Modifier.Public | Modifier.Partial
            };
            builder.AddClass(clss);

            string fieldName = $"{cname}Data";

            string[] columns = dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();

            string dataType = cmd.GetValue("type") ?? "list";

            if (dataType == "list")
            {
                List<Value> L = new List<Value>();
                TypeInfo type = new TypeInfo { userType = $"{cname}" };
                foreach (DataRow row in dt.Rows)
                {
                    ObjectValue V = new ObjectValue();
                    for (int i = 0; i < columns.Length; i++)
                    {
                        V.Add(columns[i], new Value(row[i]));
                    }
                    L.Add(new Value(V) { type = type });
                }

                TypeInfo typeinfo = new TypeInfo { userType = $"{cname}[]" };
                Field field = new Field(typeinfo, fieldName, new Value(L.ToArray()) { type = typeinfo, format = ValueOutputFormat.MultipleLine })
                {
                    modifier = Modifier.Public | Modifier.Static | Modifier.Readonly
                };

                clss.Add(field);
            }
            else
            {
                if (dt.Columns.Count < 2)
                {
                    stdio.ErrorFormat("cannot generate dictionary class, column# > 2");
                    return;
                }

                List<KeyValuePair<object, object>> L = new List<KeyValuePair<object, object>>();
                var keyType = new TypeInfo(dt.Columns[0].DataType);
                var valueType = new TypeInfo(dt.Columns[1].DataType);
                if (dt.Columns.Count != 2)
                    valueType = new TypeInfo { userType = cname };

                TypeInfo type = new TypeInfo { userType = $"{cname}" };
                foreach (DataRow row in dt.Rows)
                {
                    string key = Value.ToPrimitive(row[0]);

                    if (dt.Columns.Count != 2)
                    {
                        ObjectValue V = new ObjectValue();
                        for (int i = 0; i < columns.Length; i++)
                        {
                            V.Add(columns[i], new Value(row[i]));
                        }
                        L.Add(new KeyValuePair<object, object>(key, new Value(V) { type = type }));
                    }
                    else
                    {
                        L.Add(new KeyValuePair<object, object>(key, new Value(row[1])));
                    }
                }

                var groups = L.GroupBy(x => x.Key, x => x.Value);
                Dictionary<object, object> dict = new Dictionary<object, object>();
                foreach (var group in groups)
                {
                    var A = group.ToArray();
                    if (A.Length > 1)
                        valueType.isArray = true;
                }

                foreach (var group in groups)
                {
                    var A = group.ToArray();
                    object val;
                    if (valueType.isArray)
                        val = new Value(A) { type = valueType, format = ValueOutputFormat.Wrap };
                    else
                        val = A[0];

                    dict.Add(group.Key, val);
                }


                TypeInfo typeinfo = new TypeInfo { userType = $"Dictionary<{keyType},{valueType}>" };
                Field field = new Field(typeinfo, fieldName, new Value(dict) {type= typeinfo, format = ValueOutputFormat.MultipleLine })
                {
                    modifier = Modifier.Public | Modifier.Static | Modifier.Readonly
                };

                clss.Add(field);
            }

            string code = $"{builder}";

            string path = cmd.GetValue("out");
            if (path == null)
            {
                stdio.WriteLine(code);
            }
            else
            {
                string file = Path.ChangeExtension(Path.Combine(path, cname), "cs");
                try
                {
                    code.WriteIntoFile(file);
                    stdio.WriteLine("code generated on {0}", file);
                }
                catch (Exception ex)
                {
                    stdio.WriteLine(ex.Message);
                }
            }
        }
    }
}

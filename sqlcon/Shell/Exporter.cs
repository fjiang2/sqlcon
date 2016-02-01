using System;
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
        private string MyDocuments;

        XmlDbFile xml;
        public Exporter(PathManager mgr, TreeNode<IDataPath> pt, Configuration cfg)
        {
            this.mgr = mgr;
            this.cfg = cfg;
            this.xml = new XmlDbFile { XmlDbFolder = cfg.XmlDbFolder };
            this.fileName = cfg.OutputFile;
            if (pt.Item is TableName)
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

            MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\sqlcon";
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


        public void ExportCreate()
        {
            if (tname != null)
            {
                stdio.WriteLine("start to generate {0} CREATE TABLE script to file: {1}", tname, fileName);
                using (var writer = fileName.NewStreamWriter())
                {
                    writer.WriteLine(tname.IF_EXISTS_DROP_TABLE());
                    writer.WriteLine("GO");
                    writer.WriteLine(tname.GenerateScript());
                }
                stdio.WriteLine("completed");
            }
            else if (dname != null)
            {
                stdio.WriteLine("start to generate {0} script to file: {1}", dname, fileName);
                using (var writer = fileName.NewStreamWriter())
                {
                    writer.WriteLine(dname.GenerateScript());
                }
                stdio.WriteLine("completed");
            }
            else
            {
                stdio.ErrorFormat("warning: table or database is not seleted");
            }

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
                    CancelableWork.CanCancel(cancelled =>
                    {
                        foreach (var tn in tnames)
                        {
                            if (cancelled())
                                return CancelableState.Cancelled;

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
                        return CancelableState.Completed;
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
                CancelableWork.CanCancel(cancelled =>
                {
                    foreach (var tname in mt.MatchedTableNames)
                    {
                        if (cancelled())
                            return CancelableState.Cancelled;

                        stdio.WriteLine("start to generate {0}", tname);
                        var dt = new SqlBuilder().SELECT.TOP(cmd.top).COLUMNS().FROM(tname).SqlCmd.FillDataTable();
                        var file = xml.Write(tname, dt);
                        stdio.WriteLine("completed {0} => {1}", tname.ShortName, file);
                    }
                    return CancelableState.Completed;
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
            string path = cfg.GetValue<string>("dpo.path", $"{MyDocuments}\\dpo");
            string ns = cfg.GetValue<string>("dpo.ns", "Sys.DataModel.Dpo");
            string suffix = cfg.GetValue<string>("dpo.suffix", Setting.DPO_CLASS_SUFFIX_CLASS_NAME);

            Func<string, string> rule =
                name => name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower() + suffix;

            if (tname != null)
            {
                TableClass clss = new TableClass(tname) { NameSpace = ns, ClassNameRule = rule };
                clss.CreateClass(path);
                stdio.WriteLine("generated class {0} at {1}", tname.ShortName, path);
            }
            else if (dname != null)
            {
                stdio.WriteLine("start to generate database {0} class to directory: {1}", dname, path);
                CancelableWork.CanCancel(cancelled =>
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, cfg.exportExcludedTables);
                    TableName[] tnames = md.MatchedTableNames;
                    foreach (var tn in tnames)
                    {
                        if (cancelled())
                            return CancelableState.Cancelled;
                        try
                        {
                            TableClass clss = new TableClass(tn) { NameSpace = ns, ClassNameRule = rule };
                            clss.CreateClass(path);
                            stdio.WriteLine("generated class for {0} at {1}", tn.ShortName, path);
                        }
                        catch (Exception ex)
                        {
                            stdio.ErrorFormat("failed to generate class {0}, {1}", tn.ShortName, ex.Message);
                        }
                    }

                    stdio.WriteLine("completed");
                    return CancelableState.Completed;
                });
            }
            else
            {
                stdio.ErrorFormat("warning: database is not selected");
            }

        }

        public void ExportCsvFile(Command cmd)
        {
            string path = cfg.GetValue<string>("csv.path", $"{MyDocuments}\\csv");
            
            string file;
            Func<TableName, string> fullName = tname=> $"{path}\\{sname.Path}\\{dname.Name}\\{tname.ShortName}.csv"; 

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
                CancelableWork.CanCancel(cancelled =>
                {
                    var md = new MatchedDatabase(dname, cmd.wildcard, cfg.exportExcludedTables);
                    TableName[] tnames = md.MatchedTableNames;
                    foreach (var tn in tnames)
                    {
                        if (cancelled())
                            return CancelableState.Cancelled;
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
                    return CancelableState.Completed;
                });
            }
            else
            {
                stdio.ErrorFormat("warning: table or database is not seleted");
            }
        }

        public void ExportDataContract(Command cmd)
        {

            DataTable dt = null;
            if (SqlShell.LastResult is DataTable)
            {
                dt = SqlShell.LastResult as DataTable;
            }

            if (dt == null)
            {
                stdio.ErrorFormat("data table cannot find, use command type or select first");
                return;
            }


            Dictionary<DataColumn, TypeInfo> dict = new Dictionary<DataColumn, TypeInfo>();
            foreach (DataColumn column in dt.Columns)
            {
                TypeInfo ty = new TypeInfo(column.DataType);
                foreach (DataRow row in dt.Rows)
                {
                    if (row[column] == DBNull.Value)
                        ty.Nullable = true;
                    break;
                }

                dict.Add(column, ty);
            }


            string path = cfg.GetValue<string>("dc.path", $"{MyDocuments}\\dpo");
            string ns = cmd.GetValue("ns") ?? cfg.GetValue<string>("dc.ns", "Sys.DataContracts");
            string clss = cmd.GetValue("class") ?? cfg.GetValue<string>("dc.class", "DataContract");
            string mtd = cmd.GetValue("method") ?? cfg.GetValue<string>("dc.method", "ToEnumerable");

            ClassBuilder builder1 = new ClassBuilder(ns, AccessModifier.Public | AccessModifier.Partial, clss);
            builder1.AddUsing("System");
            builder1.AddUsing("System.Collections.Generic");
            builder1.AddUsing("System.Data");
            builder1.AddUsing("System.Linq");

            foreach (DataColumn column in dt.Columns)
            {
                builder1.AddProperty(new Property(dict[column], column.ColumnName));
            }

            ClassBuilder builder2 = new ClassBuilder(ns, AccessModifier.Public | AccessModifier.Static, clss + "Reader");
            Method method = new Method
            {
                modifier = AccessModifier.Public | AccessModifier.Static,
                userReturnType = $"IEnumerable<{clss}>",
                methodName = mtd,
                args = new Argument[] { new Argument(typeof(DataTable), "dt") }
            };
            builder2.AddMethod(method);

            Statement sent = new Statement();
            sent.Append("return dt.AsEnumerable()");
            sent.Append($".Select(row => new {clss}");
            sent.Append("{");

            int count = dt.Columns.Count;
            int i = 0;
            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var line = $"\t{column.ColumnName} = row.Field<{type}>(\"{column.ColumnName}\")";
                if (++i < count)
                    line += ",";

                sent.Add(line);
            }
            sent.Append("})");

            method.AddStatement(sent);
            string code = $"{ builder1}\r\n{builder2}" ;
            string file = Path.ChangeExtension(Path.Combine(path, clss), "cs");
            using (var writer = file.NewStreamWriter())
            {
                writer.WriteLine(code);
            }

            stdio.WriteLine("code generated on {0}", file);

        }

    }
}

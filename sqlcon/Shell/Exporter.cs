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

        public Exporter(PathManager mgr, TreeNode<IDataPath> pt, Configuration cfg)
        {
            this.mgr = mgr;
            this.cfg = cfg;

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
        }

        private string getPath(ServerName sname) => string.Format("{0}\\{1}", cfg.XmlDbFolder, sname.Path);

        private string getPath(DatabaseName dname) => string.Format("{0}\\{1}", getPath(dname.ServerName), dname.Name);

        private string getDataFileName(TableName tname) => string.Format("{0}\\{1}.xml", getPath(tname.DatabaseName), tname.ShortName);

        private string getSchemaFilName(ServerName sname) => string.Format("{0}\\{1}.xml", getPath(sname), sname.Path);

        private string getSchemaFilName(DatabaseName dname) => string.Format("{0}\\{1}.xml", getPath(sname), dname.Name);

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
                    TableName[] tnames = md.DefaultTableNames;
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
            string file;
            if (dname != null)
            {
                file = getSchemaFilName(dname);
                stdio.WriteLine("start to generate database {0} schema to file: {1}", dname, file);
                using (var writer = file.NewStreamWriter())
                {
                    DataTable dt = dname.DatabaseSchema();
                    dt.WriteXml(writer, XmlWriteMode.WriteSchema);
                }
                stdio.WriteLine("completed");
            }
            else if (sname != null)
            {
                file = getSchemaFilName(sname);
                if (sname != null)
                {
                    stdio.WriteLine("start to generate server {0} schema to file: {1}", sname, file);
                    using (var writer = file.NewStreamWriter())
                    {
                        DataSet ds = sname.ServerSchema();
                        ds.WriteXml(writer, XmlWriteMode.WriteSchema);
                    }
                    stdio.WriteLine("completed");
                }
                else
                    stdio.ErrorFormat("warning: server or database is not selected");
            }
        }

        public void ExportData(Command cmd)
        {
            string file;
            if (tname != null)
            {
                file = getDataFileName(tname);

                stdio.WriteLine("start to generate {0} data file: {1}", tname, file);
                using (var writer = file.NewStreamWriter())
                {
                    var dt = new TableReader(tname).Table;
                    dt.TableName = tname.Name;
                    dt.DataSet.DataSetName = tname.DatabaseName.Name;
                    dt.WriteXml(writer, XmlWriteMode.WriteSchema);
                }
                stdio.WriteLine("completed");
            }
            else if (dname != null)
            {
                stdio.WriteLine("start to generate {0} data files", dname);
                var mt = new MatchedDatabase(dname, cmd.wildcard, cfg.exportExcludedTables);
                CancelableWork.CanCancel(cancelled =>
                {
                    foreach (var tname in mt.DefaultTableNames)
                    {
                        if (cancelled())
                            return CancelableState.Cancelled;

                        file = getDataFileName(tname);
                        stdio.WriteLine("generate {0} => {1}", tname.ShortName, file);
                        using (var writer = file.NewStreamWriter())
                        {
                            var dt = new SqlBuilder().SELECT.TOP(cmd.top).COLUMNS().FROM(tname).SqlCmd.FillDataTable();
                            dt.TableName = tname.Name;
                            dt.DataSet.DataSetName = tname.DatabaseName.Name;
                            dt.WriteXml(writer, XmlWriteMode.WriteSchema);
                        }
                    }
                    return CancelableState.Completed;
                }
               );
                stdio.WriteLine("completed");
            }
            else
            {
                stdio.ErrorFormat("warning: table or database is not seleted");
            }
        }

        public void ExportClass(Command cmd)
        {
            string path = cfg.GetValue<string>("dpo.path", "c:\\temp\\dpo");
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
                    TableName[] tnames = md.DefaultTableNames;
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

    }
}

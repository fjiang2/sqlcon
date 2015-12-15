using System;
using System.Collections.Generic;
using System.Linq;
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

        public Exporter(PathManager mgr, TreeNode<IDataPath> pt, Configuration cfg)
        {
            this.mgr = mgr;
            this.cfg = cfg;

            this.fileName = cfg.OutputFile;
            if (pt.Item is TableName)
            {
                this.tname = (TableName)pt.Item;
                this.dname = (DatabaseName)pt.Parent.Item;
            }
            else if (pt.Item is DatabaseName)
            {
                this.tname = null;
                this.dname = (DatabaseName)pt.Item;
            }
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
                    TableName[] tnames = dname.GetDependencyTableNames();
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
                stdio.WriteLine("start to generate database {0} schema to file: {1}", dname, cfg.SchemaFile);
                using (var writer = cfg.SchemaFile.NewStreamWriter())
                {
                    DataTable dt = dname.DatabaseSchema();
                    dt.WriteXml(writer, XmlWriteMode.WriteSchema);
                }
                stdio.WriteLine("completed");
            }
            else
            {
                ServerName sname = mgr.GetCurrentPath<ServerName>();
                if (sname != null)
                {
                    stdio.WriteLine("start to generate server {0} schema to file: {1}", sname, cfg.SchemaFile);
                    using (var writer = cfg.SchemaFile.NewStreamWriter())
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

        public void ExportData()
        {
            throw new NotImplementedException();
        }

        public void ExportClass()
        {
            string path = cfg.GetValue<string>("dpo.path", "c:\\temp\\dpo");
            string ns = cfg.GetValue<string>("dpo.ns", "Sys.DataModel.Dpo");
            string suffix = cfg.GetValue<string>("dpo.suffix", Setting.DPO_CLASS_SUFFIX_CLASS_NAME);

            Func<string, string> rule = 
                name =>  name.Substring(0,1).ToUpper() + name.Substring(1).ToLower() + suffix;

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
                    foreach (var tn in dname.GetTableNames())
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

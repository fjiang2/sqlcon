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
        private ShellContext context;

        private string fileName;
        private TableName tname;
        private DatabaseName dname;

        public Exporter(ShellContext context)
        {
            this.context = context;

            this.fileName = context.cfg.OutputFile;
            this.tname = context.mgr.GetCurrentPath<TableName>();
            this.dname = context.mgr.GetCurrentPath<DatabaseName>();
        }

       
        private void ExportScud(SqlScriptType type)
        {
            if (tname != null)
            {
                using (var writer = fileName.NewStreamWriter())
                {
                    string sql = context.theSide.GenerateTemplate(tname, type);
                    stdio.WriteLine(sql);
                    writer.WriteLine(sql);
                }
            }
            else
            {
                stdio.ErrorFormat("warning: table is not selected");
            }
        }


        private void ExportCreate()
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

        private void ExportInsert(Command cmd)
        {
            if (tname != null)
            {
                if (cmd.IsSchema)
                {
                    using (var writer = fileName.NewStreamWriter())
                    {
                        string sql = context.theSide.GenerateTemplate(tname, SqlScriptType.INSERT);
                        stdio.WriteLine(sql);
                        writer.WriteLine(sql);
                    }
                }
                else
                {
                    var node = context.mgr.GetCurrentNode<Locator>();
                    int count;

                    using (var writer = fileName.NewStreamWriter())
                    {
                        if (node != null)
                        {
                            stdio.WriteLine("start to generate {0} INSERT script to file: {1}", tname, fileName);
                            Locator locator = context.mgr.GetCombinedLocator(node);
                            count = context.theSide.GenerateRows(writer, tname, locator, cmd.HasIfExists);
                            stdio.WriteLine("insert clauses (SELECT * FROM {0} WHERE {1}) generated to {2}", tname, locator, fileName);
                        }
                        else
                        {
                            count = context.theSide.GenerateRows(writer, tname, null, cmd.HasIfExists);
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

                            if (!context.cfg.exportExcludedTables.IsMatch(tn.ShortName))
                            {
                                int count = new SqlCmd(tn.Provider, string.Format("SELECT COUNT(*) FROM {0}", tn)).FillObject<int>();
                                if (count > context.cfg.Export_Max_Count)
                                {
                                    if (!stdio.YesOrNo("are you sure to export {0} rows on {1} (y/n)?", count, tn.ShortName))
                                    {
                                        stdio.WriteLine("\n{0,10} skipped", tn.ShortName);
                                        continue;
                                    }
                                }

                                count = context.theSide.GenerateRows(writer, tn, null, cmd.HasIfExists);
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

        private void ExportSchema()
        {
            if (dname != null)
            {
                stdio.WriteLine("start to generate database {0} schema to file: {1}", dname, context.cfg.SchemaFile);
                using (var writer = context.cfg.SchemaFile.NewStreamWriter())
                {
                    DataTable dt = dname.DatabaseSchema();
                    dt.WriteXml(writer, XmlWriteMode.WriteSchema);
                }
                stdio.WriteLine("completed");
            }
            else
            {
                ServerName sname = context.mgr.GetCurrentPath<ServerName>();
                if (sname != null)
                {
                    stdio.WriteLine("start to generate server {0} schema to file: {1}", sname, context.cfg.SchemaFile);
                    using (var writer = context.cfg.SchemaFile.NewStreamWriter())
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


        private void ExportClass()
        {
            string path = context.cfg.GetValue<string>("dpo.path", "c:\\temp\\dpo");
            string ns = context.cfg.GetValue<string>("dpo.ns", "Sys.DataModel.Dpo");
            string suffix = context.cfg.GetValue<string>("dpo.suffix", Setting.DPO_CLASS_SUFFIX_CLASS_NAME);

            Func<string, string> rule = 
                name =>  name.Substring(0,1).ToUpper() + name.Substring(1).ToLower() + suffix;

            if (tname != null)
            {
                TableClass clss = new TableClass(tname) { NameSpace = ns, ClassNameRule = rule };
                clss.CreateClass(path);
                stdio.WriteLine("generated class {0}", tname.ShortName);
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
                            stdio.WriteLine("generated class for {0}", tn.ShortName);
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


        public bool ExportSqlScript(Command cmd)
        {
            if (cmd.HasHelp)
            {
                stdio.WriteLine("export data, schema, class, and template");
                stdio.WriteLine("export insert  : export INSERT INTO script on current table/database");
                stdio.WriteLine("  [/if]        : option /if generate if exists row then UPDATE else INSERT");
                stdio.WriteLine("export create  : generate CREATE TABLE script on current table/database");
                stdio.WriteLine("export select  : generate SELECT FROM WHERE template");
                stdio.WriteLine("export update  : generate UPDATE SET WHERE template");
                stdio.WriteLine("export delete  : generate DELETE FROM WHERE template");
                stdio.WriteLine("export schema  : generate database schema xml file");
                stdio.WriteLine("export class   : generate C# table class");
                return true;
            }


            switch (cmd.arg1)
            {
                case "insert":
                    ExportInsert(cmd);
                    return true;

                case "create":
                    ExportCreate();
                    return true;

                case "select":
                    ExportScud(SqlScriptType.SELECT);
                    return true;

                case "delete":
                    ExportScud(SqlScriptType.DELETE);
                    return true;

                case "update":
                    ExportScud(SqlScriptType.UPDATE);
                    return true;

                case "schema":
                    ExportSchema();
                    return true;

                case "class":
                    ExportClass();
                    return true;

                default:
                    stdio.ErrorFormat("invalid command");
                    break;
            }

            return true;
        }
    }
}

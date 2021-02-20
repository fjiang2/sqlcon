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
    class Importer
    {
        private PathManager mgr;
        private ApplicationCommand cmd;
        private IApplicationConfiguration cfg;


        private TableName tname;
        private DatabaseName dname;
        private ServerName sname;

        XmlDbCreator xmlDbFile;

        public Importer(PathManager mgr, TreeNode<IDataPath> pt, ApplicationCommand cmd, IApplicationConfiguration cfg)
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

        public void Run()
        {
            if (cmd.Has("zip"))
                ProcessZipArchive();
            else if (cmd.Has("resource"))
                ImportResourceData();
            else
                cerr.WriteLine("invalid command options");
        }


        public void ProcessZipArchive()
        {
            string file = cmd.arg1;
            if (file == null)
            {
                cerr.WriteLine("file name not specified");
                return;
            }

            if (!File.Exists(file))
            {
                cerr.WriteLine($"cannot find the file \"{file}\"");
                return;
            }

            bool zip = false;
            if (Path.GetExtension(file) == ".zip")
                zip = true;

            if (cmd.Has("zip"))
                zip = true;

            using (var reader = new StreamReader(file))
            {
                if (zip)
                {
                    ZipFileReader.ProcessZipArchive(file, line => Console.WriteLine(line));
                }
            }
        }

        private void ImportResourceData()
        {
            ResourceFomat format = cmd.GetEnum("format", ResourceFomat.resx);
            string file_name = cmd.InputPath() ?? ".";
            string schema_name = cmd.GetValue("schema-name") ?? SchemaName.dbo;
            string table_name = cmd.GetValue("table-name");
            string name_column = cmd.GetValue("name-column");
            string value_column = cmd.GetValue("value-column") ?? name_column;

            if (!File.Exists(file_name))
            {
                cerr.WriteLine($"file doesn't exist: \"{file_name}\"");
                return;
            }

            if (tname == null)
            {
                if (table_name == null)
                {
                    cerr.WriteLine($"/table-name is not defined");
                    return;
                }

                if (dname == null)
                {
                    cerr.WriteLine($"required to select a database");
                    return;
                }

                tname = new TableName(dname, schema_name, table_name);
                if (!tname.Exists())
                {
                    cerr.WriteLine($"table-name doesn't exist: {tname}");
                    return;
                }
            }

            DataTable dt = new TableReader(tname)
            {
                CaseSensitive = true,
            }.Table;

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

            ResourceTableWriter writer = new ResourceTableWriter(file_name)
            {
                tname = tname,
                dt = dt,
                name_column = name_column,
                value_column = value_column,
            };

            List<ResourceEntry> entries = writer.Preprocess(format, dt, name_column, value_column);
            foreach (var entry in entries)
            {
                if (entry.Action == DataRowAction.Add)
                {
                    Console.WriteLine($"new entry: \"{entry.Name}\", \"{entry.NewValue}\"");
                }
                else if (entry.Action == DataRowAction.Change)
                {
                    Console.WriteLine($"update entry: \"{entry.Name}\", \"{entry.OldValue}\" -> \"{entry.NewValue}\"");
                }
            }

            cout.WriteLine($"{entries.Count} of entries on \"{file_name}\"");

            bool commit = cmd.Has("commit");
            if (commit)
            {
                cout.WriteLine($"starting to save changes into table \"{tname}\"");
                writer.SubmitChanges(entries);
                cout.WriteLine($"completed to save on table \"{tname}\"");
            }
        }


        public static void Help()
        {
            cout.WriteLine("import data");
            cout.WriteLine("import [path]              :");
            cout.WriteLine("options:");
            cout.WriteLine("  /zip                     : dump variables memory to output file");
            cout.WriteLine("  /out                     : define output file or directory");
            cout.WriteLine("  /resource: import resource file into a table");
            cout.WriteLine("      [/format:]      : resource format: resx|xlf|json, default:resx");
            cout.WriteLine("      [/schema-name:] : default is dbo");
            cout.WriteLine("      [/table-name:]  : default is current table selected");
            cout.WriteLine("      [/name-column:] : name column");
            cout.WriteLine("      [/value-column:]: value column");
            cout.WriteLine("      [/language:]    : language: en|es|..., default:en");
            cout.WriteLine("      [/in:]          : resource file name");
            cout.WriteLine("example:");
            cout.WriteLine("  import insert.sql        : run script");
            cout.WriteLine("  import insert.zip  /zip  : run script, default extension is .sqt");
        }
    }
}

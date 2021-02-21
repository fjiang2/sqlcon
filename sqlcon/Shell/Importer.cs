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
            string file_name = cmd.InputPath();
            ResourceFormat format = cmd.GetEnum("format", ResourceFormat.resx);
            string schema_name = cmd.GetValue("schema-name") ?? SchemaName.dbo;
            string table_name = cmd.GetValue("table-name");
            string name_column = cmd.GetValue("name-column") ?? "name";
            string value_column = cmd.GetValue("value-column") ?? name_column;
            string order_column = cmd.GetValue("order-column");
            bool trim_name = cmd.Has("trim-name");
            bool trim_value = cmd.Has("trim-value");

            if (file_name == null)
            {
                cerr.WriteLine($"file name is not defined, use option /in:file_name");
                return;
            }

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

            cout.WriteLine($"{dt.Rows.Count} of entries on \"{file_name}\"");

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

            ResourceTableWriter writer = new ResourceTableWriter(file_name, tname, name_column, value_column, order_column);
            List<ResourceEntry> entries = writer.Difference(format, dt, trim_name, trim_value);
            foreach (var entry in entries)
            {
                if (entry.Action == DataRowAction.Add)
                {
                    cout.WriteLine($"new entry: \"{entry.Name}\", \"{entry.NewValue}\"");
                }
                else if (entry.Action == DataRowAction.Change)
                {
                    cout.WriteLine($"update entry: \"{entry.Name}\", \"{entry.OldValue}\" -> \"{entry.NewValue}\"");
                }
            }

            if (entries.Count > 0)
                cout.WriteLine($"{entries.Count} of entries were changed");
            else
                cout.WriteLine($"no entry is changed");

            if (entries.Count > 0)
            {
                bool commit = cmd.Has("submit-changes");
                if (commit)
                {
                    cout.WriteLine($"starting to save changes into table \"{tname}\"");
                    writer.SubmitChanges(entries);
                    cout.WriteLine($"completed to save on table \"{tname}\" from \"{file_name}\"");
                }
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
            cout.WriteLine("      [/in:]            : resource file name");
            cout.WriteLine("      [/format:]        : resource format: resx|xlf|json, default:resx");
            cout.WriteLine("      [/schema-name:]   : default is dbo");
            cout.WriteLine("      [/table-name:]    : default is current table selected");
            cout.WriteLine("      [/name-column:]   : name column, default is name");
            cout.WriteLine("      [/value-column:]  : value column");
            cout.WriteLine("      [/order-column:]  : keep order of entries, it is integer");
            cout.WriteLine("      [/trim-name]      : trim string of property [name]");
            cout.WriteLine("      [/trim-value]     : trim string of property [value]");
            cout.WriteLine("      [/submit-changes] : save entries into database");
            cout.WriteLine("example:");
            cout.WriteLine("  import insert.sql        : run script");
            cout.WriteLine("  import insert.zip  /zip  : run script, default extension is .sqt");
            cout.WriteLine("  import /resource /format:resx /table-name:i18n-resx-table /name-column:name /value-column:es /in:.\resource.es.resx /commit");
        }
    }
}

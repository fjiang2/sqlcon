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
            bool deleteRowNotInResource = cmd.Has("delete-rows-not-in-resource-file");

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

            if (!ValidateColumn<string>(dt, name_column, "name-column", required: true))
                return;
            if (!ValidateColumn<string>(dt, value_column, "value-column", required: true))
                return;
            if (!ValidateColumn<int>(dt, order_column, "order-column", required: false))
                return;

            cout.WriteLine($"{dt.Rows.Count} of entries on \"{file_name}\"");

            ResourceTableWriter writer = new ResourceTableWriter(file_name, tname, name_column, value_column, order_column);
            List<ResourceEntry> entries = writer.Differ(format, dt, trim_name, trim_value);
            foreach (var entry in entries)
            {
                switch (entry.Action)
                {
                    case DataRowAction.Add:
                        cout.WriteLine($"new entry: \"{entry.Name}\", \"{entry.NewValue}\"");
                        break;

                    case DataRowAction.Change:
                        cout.WriteLine($"update entry: \"{entry.Name}\", \"{entry.OldValue}\" -> \"{entry.NewValue}\"");
                        break;

                    case DataRowAction.Delete:
                        cout.WriteLine($"delete entry: \"{entry.Name}\"");
                        break;
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
                    writer.SubmitChanges(entries, deleteRowNotInResource);
                    cout.WriteLine($"completed to save on table \"{tname}\" from \"{file_name}\"");
                }
            }
        }

        public static bool ValidateColumn<T>(DataTable dt, string columnName, string option, bool required)
        {
            if (columnName == null)
            {
                if (!required)
                    return true;

                cerr.WriteLine($"{option} is undefined");
                return false;
            }

            if (!dt.Columns.Contains(columnName))
            {
                cerr.WriteLine($"{option} doesn't exist: {columnName}");
                return false;
            }

            DataColumn column = dt.Columns[columnName];
            if (column.DataType != typeof(T))
            {
                cerr.WriteLine($"{option} data type is required: {typeof(T)}");
                return false;
            }

            return true;
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
            cout.WriteLine("  import /resource /format:resx /table-name:i18n-resx-table /name-column:name /value-column:es /in:.\\resource.es.resx /submit-changes");
            cout.WriteLine("  import /resource /format:json /table-name:i18n-json-table /name-column:name /value-column:es /in:.\\es.json /submit-changes");
        }
    }
}

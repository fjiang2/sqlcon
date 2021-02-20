using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;

namespace Sys.Data.Resource
{
    public class ResourceTableWriter
    {
        private string path { get; }
        public TableName tname { get; set; }
        public DataTable dt { get; set; }

        public string name_column { get; set; }
        public string value_column { get; set; }


        public ResourceTableWriter(string path)
        {
            this.path = path;
        }

        public void SubmitChanges(List<ResourceEntry> entries)
        {
            SqlMaker gen = new SqlMaker(tname.FormalName)
            {
                PrimaryKeys = new string[] { name_column }
            };

            StringBuilder builder = new StringBuilder();
            int count = 0;
            int max = 1000;
            foreach (var entry in entries)
            {
                gen.Clear();
                switch (entry.Action)
                {
                    case DataRowAction.Add:
                        gen.Add(name_column, entry.Name);
                        gen.Add(value_column, entry.NewValue);
                        builder.AppendLine(gen.Insert());
                        break;

                    case DataRowAction.Change:
                        gen.Add(name_column, entry.Name);
                        gen.Add(value_column, entry.NewValue);
                        builder.AppendLine(gen.Update());
                        break;
                }

                count = (count + 1) % max;
                if (count == 0)
                    execute();
            }

            execute();

            void execute()
            {
                string SQL = builder.ToString();
                builder.Clear();
                if (!string.IsNullOrEmpty(SQL))
                {
                    SqlCmd cmd = new SqlCmd(SQL);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<ResourceEntry> Preprocess(ResourceFomat format, DataTable dt, string name_column, string value_column)
        {
            //load data rows from database
            var rows = dt.ToList(row => new entry
            {
                name = row.GetField<string>(name_column),
                value = row.GetField<string>(value_column)
            });

            ResourceFileReader reader = new ResourceFileReader();
            List<entry> entries = new List<entry>();

            if (format == ResourceFomat.resx)
                entries = reader.ReadResx(path);

            var list = Preprocess(entries, rows);
            return list;
        }


        private List<ResourceEntry> Preprocess(List<entry> entries, List<entry> rows)
        {
            List<ResourceEntry> list = new List<ResourceEntry>();
            foreach (entry entry in entries)
            {
                var found = rows.SingleOrDefault(row => row.name == entry.name);
                ResourceEntry item = new ResourceEntry
                {
                    Name = entry.name,
                    NewValue = entry.value,
                };

                if (found == null)
                {
                    item.Action = DataRowAction.Add;
                    list.Add(item);
                }
                else if (found.value != entry.value)
                {
                    item.Action = DataRowAction.Change;
                    item.OldValue = found.value;
                    list.Add(item);
                }
                else
                {
                    //no change
                }
            }

            return list;
        }

    }
}

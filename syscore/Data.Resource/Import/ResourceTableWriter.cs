using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;

namespace Sys.Data.Resource
{

    /// <summary>
    /// Write data row into resource definition table
    /// </summary>
    public class ResourceTableWriter
    {
        private readonly string path;
        private readonly TableName tname;

        private readonly string name_column;
        private readonly string value_column;
        private readonly string order_column;

        public ResourceTableWriter(string path, TableName tname, string name_column, string value_column, string order_column)
        {
            this.path = path;
            this.tname = tname;
            this.name_column = name_column;
            this.value_column = value_column;
            this.order_column = order_column;
        }

        /// <summary>
        /// Save into database
        /// </summary>
        /// <param name="entries"></param>
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
                gen.Add(name_column, entry.Name);
                gen.Add(value_column, entry.NewValue);
                if (order_column != null)
                    gen.Add(order_column, entry.Index);

                switch (entry.Action)
                {
                    case DataRowAction.Add:
                        builder.AppendLine(gen.Insert());
                        break;

                    case DataRowAction.Change:
                        builder.AppendLine(gen.Update());
                        break;
                }

                count = (count + 1) % max;
                if (count == 0)
                    ExecuteNonQuery();
            }

            ExecuteNonQuery();

            void ExecuteNonQuery()
            {
                string SQL = builder.ToString();
                builder.Clear();
                if (!string.IsNullOrEmpty(SQL))
                {
                    SqlCmd cmd = new SqlCmd(tname.Provider, SQL);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// Compare data table and resouce file, and find differences
        /// </summary>
        /// <param name="format"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<ResourceEntry> Difference(ResourceFomat format, DataTable dt, bool trimName, bool trimValue)
        {
            //load data rows from database
            var rows = dt.ToList(row => new entry
            {
                name = row.GetField<string>(name_column),
                value = row.GetField<string>(value_column)
            });

            ResourceFileReader reader = new ResourceFileReader
            {
                TrimPropertyName = trimName,
                TrimPropertyValue = trimValue,
            };

            List<entry> entries = new List<entry>();

            if (format == ResourceFomat.resx)
                entries = reader.ReadResx(path);

            var list = Preprocess(entries, rows);
            return list;
        }


        private List<ResourceEntry> Preprocess(List<entry> entries, List<entry> rows)
        {
            List<ResourceEntry> list = new List<ResourceEntry>();

            int index = 0;
            foreach (entry entry in entries)
            {
                var found = rows.SingleOrDefault(row => row.name == entry.name);
                ResourceEntry item = new ResourceEntry
                {
                    Name = entry.name,
                    NewValue = entry.value,
                    Index = index++,
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

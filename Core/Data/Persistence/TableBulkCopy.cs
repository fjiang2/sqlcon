using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Sys.Data
{
    public class TableBulkCopy
    {
        public int MaxRowCount { get; set; } = 5000;

        private TableReader tableReader;

        public TableBulkCopy(TableReader reader)
        {
            this.tableReader = reader;
        }


        public int CopyTo(TableName tname2, SqlBulkCopyColumnMapping[] mappings, CancellationTokenSource cts, IProgress<int> progress)
        {
            DataTable table = new DataTable();
            int step = 0;

            Action<DbDataReader> export = reader =>
            {
                table = new DbReader(reader).Table;

                DataRow row;

                while (reader.Read())
                {
                    step++;

                    if (step % 19 == 0)
                        progress?.Report(step);

                    row = table.NewRow();

                    for (int i = 0; i < reader.FieldCount; i++)
                        row[i] = reader.GetValue(i);

                    table.Rows.Add(row);

                    if (step % MaxRowCount == 0)
                        BulkCopy(table, tname2, mappings);

                    if (cts.IsCancellationRequested)
                        break;
                }

                if (step % MaxRowCount != 0)
                    BulkCopy(table, tname2, mappings);
            };

            tableReader.cmd.Execute(export);

            return step;
        }

        private static void BulkCopy(DataTable table, TableName tname2, SqlBulkCopyColumnMapping[] mappings)
        {
            table.AcceptChanges();
            table.TableName = tname2.Name;

            BulkCopy(table, mappings, tname2.Provider.ConnectionString);
            table.Clear();
        }

        private static void BulkCopy(DataTable table, SqlBulkCopyColumnMapping[] mappings, string connectionString)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                foreach (var mapping in mappings)
                    bulkCopy.ColumnMappings.Add(mapping);

                bulkCopy.DestinationTableName = table.TableName;
                bulkCopy.WriteToServer(table);
            }
        }


        /// <summary>
        /// return number of records copied
        /// </summary>
        /// <param name="tname1"></param>
        /// <param name="tname2"></param>
        /// <returns></returns>
        public static int Copy(TableName tname1, TableName tname2)
        {
            using (var cts = new CancellationTokenSource())
            {
                return Copy(tname1, tname2, cts, null);
            }
        }
        public static int Copy(TableName tname1, TableName tname2, CancellationTokenSource cts, IProgress<int> progress)
        {
            var reader = new TableReader(tname1);
            var bulkcopy = new TableBulkCopy(reader);
            return bulkcopy.CopyTo(tname2, new SqlBulkCopyColumnMapping[] { }, cts, progress);
        }
    }
}

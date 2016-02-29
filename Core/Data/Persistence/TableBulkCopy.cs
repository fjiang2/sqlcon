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


        public void CopyTo(TableName tname2, CancellationTokenSource cts, IProgress<int> progress)
        {
            DataTable table = new DataTable();
            Action<DbDataReader> export = reader =>
            {
                table = TableReader.BuildTable(reader);
                table.TableName = tname2.Name;

                int step = 0;
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
                        BulkCopy(table, tname2);

                    if (cts.IsCancellationRequested)
                        break;
                }

                if (step % MaxRowCount != 0)
                    BulkCopy(table, tname2);
            };

            tableReader.cmd.Execute(export);

        }

        private static void BulkCopy(DataTable table, TableName tname2)
        {
            table.AcceptChanges();
            BulkCopy(table, tname2.Provider.ConnectionString);
            table.Clear();
        }

        private static void BulkCopy(DataTable table, string connectionString)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.DestinationTableName = table.TableName;
                bulkCopy.WriteToServer(table);
            }
        }
    }
}

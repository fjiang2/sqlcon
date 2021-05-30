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


        public int CopyTo(TableName tname2, SqlBulkCopyColumnMapping[] mappings, CancellationToken cancellationToken, IProgress<int> progress)
        {
            DataTable table = new DataTable();
            int step = 0;

            Action<DbDataReader> copy = reader =>
            {
                var dbReader = new DbReader(reader);
                table = DbReader.CreateTable(reader);

                DataRow row;

                while (reader.Read())
                {
                    step++;

                    if (step % 19 == 0)
                        progress?.Report(step);

                    row = dbReader.ReadRow(table);
                    table.Rows.Add(row);

                    if (step % MaxRowCount == 0)
                        BulkCopy(table, tname2, mappings);

                    if (cancellationToken.IsCancellationRequested)
                        break;
                }

                if (step % MaxRowCount != 0)
                    BulkCopy(table, tname2, mappings);
            };

            tableReader.cmd.Read(copy);

            return step;
        }

        private static void BulkCopy(DataTable table1, TableName tname2, SqlBulkCopyColumnMapping[] mappings)
        {
            table1.AcceptChanges();
            table1.TableName = tname2.FormalName;

            string connectionString = tname2.Provider.ConnectionString;
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                foreach (var mapping in mappings)
                    bulkCopy.ColumnMappings.Add(mapping);

                bulkCopy.DestinationTableName = table1.TableName;
                bulkCopy.WriteToServer(table1);
            }

            table1.Clear();
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
                return Copy(tname1, tname2, cts.Token, null);
            }
        }
        public static int Copy(TableName tname1, TableName tname2, CancellationToken cancellationToken, IProgress<int> progress)
        {
            var reader = new TableReader(tname1);
            var bulkcopy = new TableBulkCopy(reader);
            return bulkcopy.CopyTo(tname2, new SqlBulkCopyColumnMapping[] { }, cancellationToken, progress);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using System.Threading;

namespace Sys.Data
{
    class DbReader
    {
        private DbDataReader reader;

        private DataTable table;

        public DbReader(DbDataReader reader)
        {
            this.reader = reader;
            table = CreateTable(reader);
        }


        private DataRow ReadLine()
        {
            DataRow row = table.NewRow();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[i] = reader.GetValue(i);
            }

            return row;
        }

        public void ReadToEnd(CancellationToken cancellationToken, IProgress<DataRow> progress)
        {
            while (reader.Read())
            {
                var row = ReadLine();
                progress.Report(row);

                if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                    break;
            }

        }


        public DataTable ReadToEnd(CancellationToken cancellationToken, IProgress<int> progress)
        {
            int step = 0;

            while (reader.Read())
            {
                step++;
                progress?.Report(step);

                var row = ReadLine();
                table.Rows.Add(row);

                if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                    break;
            }

            table.AcceptChanges();

            return table;
        }

        public static DataTable CreateTable(DbDataReader reader)
        {
            DataTable table = new DataTable();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                DataColumn column = new DataColumn(reader.GetName(i), reader.GetFieldType(i));
                table.Columns.Add(column);
            }

            table.AcceptChanges();

            return table;
        }
    }
}



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

        public DbReader(DbDataReader reader)
        {
            this.reader = reader;
        }


        public DataRow ReadRow(DataTable table)
        {
            DataRow row = table.NewRow();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[i] = reader.GetValue(i);
            }

            return row;
        }

        public void ReadTable(CancellationToken cancellationToken, IProgress<DataRow> progress)
        {
            var table = CreateTable(reader);

            while (reader.Read())
            {
                var row = ReadRow(table);
                progress.Report(row);

                if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                    break;
            }

        }


        public DataTable ReadTable(CancellationToken cancellationToken, IProgress<int> progress)
        {
            var table = CreateTable(reader);

            int step = 0;

            while (reader.Read())
            {
                step++;
                progress?.Report(step);

                var row = ReadRow(table);
                table.Rows.Add(row);

                if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                    break;
            }

            table.AcceptChanges();

            return table;
        }

        public static DataTable CreateTable(DbDataReader reader)
        {
            DataTable table = new DataTable
            {
                CaseSensitive = true,
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                DataColumn column = new DataColumn(reader.GetName(i), reader.GetFieldType(i));
                table.Columns.Add(column);
            }

            table.AcceptChanges();

            return table;
        }

        public void ReadDataSet(CancellationToken cancellationToken, IProgress<int> tableChanged, IProgress<DataRow> progress)
        {
            int step = 0;
            while (reader.HasRows)
            {
                ReadTable(cancellationToken, progress);
                tableChanged.Report(step++);
                reader.NextResult();
            }
        }

        public DataSet ReadDataSet(CancellationToken cancellationToken, IProgress<DataTable> tableChanged, IProgress<int> progress)
        {
            DataSet ds = new DataSet();
            while (reader.HasRows)
            {
                var dt = ReadTable(cancellationToken, progress);
                tableChanged.Report(dt);
                ds.Tables.Add(dt);
                reader.NextResult();
            }

            return ds;
        }
    }
}



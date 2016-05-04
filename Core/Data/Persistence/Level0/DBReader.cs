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

    public interface IDbReadLine
    {
        void Read(DbDataReader reader);
    }

    public class DbReader
    {
        private DbDataReader reader;

        public CancellationTokenSource cts { get; set; }
        public IProgress<int> progress { get; set; }

        private DataTable table;

        public DbReader(DbDataReader reader)
        {
            this.reader = reader;
            table = CreateTable();
        }

        public DataTable Table => table;

        public DataRow ReadLine()
        {
            DataRow row = table.NewRow();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[i] = reader.GetValue(i);
            }

            return row;
        }

        public void ReadToEnd(Action<DataRow> line)
        {
            int count = reader.FieldCount;
            object[] values = new object[count];

            int step = 0;

            while (reader.Read())
            {
                step++;
                progress?.Report(step);
                var row = ReadLine();
                line(row);

                if (cts != null && cts.IsCancellationRequested)
                    break;
            }

        }


        public DataTable ReadToEnd()
        {
            int step = 0;

            while (reader.Read())
            {
                step++;
                progress?.Report(step);

                var row = ReadLine();
                table.Rows.Add(row);

                if (cts != null && cts.IsCancellationRequested)
                    break;
            }

            table.AcceptChanges();

            return table;
        }

        private DataTable CreateTable()
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



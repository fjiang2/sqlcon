using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace sqlcon
{
    public class OutputDataTable
    {
        private DataTable dt;
        private OutputDataLine line;
        private string[] headers;
        private bool vertical;

        public OutputDataTable(DataTable table, TextWriter textWriter, bool vertical)
            : this(table, textWriter.WriteLine, vertical)
        {

        }

        internal OutputDataTable(DataTable table, Action<string> writeLine, bool vertical)
        {
            this.dt = table;
            this.vertical = vertical;

            List<string> list = new List<string>();
            foreach (DataColumn column in dt.Columns)
                list.Add(column.ColumnName);

            this.headers = list.ToArray();

            if (vertical)
            {
                line = new OutputDataLine(writeLine, dt.Rows.Count + 1);
            }
            else
            {
                line = new OutputDataLine(writeLine, headers.Length);
            }
        }

        public OutputDataLine Line => line;

        public void WriteData()
        {
            if (vertical)
                ToVGrid();
            else
                ToHorizontalGrid();
        }

        private void ToHorizontalGrid()
        {
            line.MeasureWidth(headers);
            foreach (DataRow row in dt.Rows)
            {
                line.MeasureWidth(row.ItemArray);
            }

            line.DisplayLine();
            line.DisplayLine(headers);
            line.DisplayLine();

            if (dt.Rows.Count == 0)
                return;

            foreach (DataRow row in dt.Rows)
            {
                line.DisplayLine(row.ItemArray);
            }

            line.DisplayLine();

        }

        private void ToVGrid()
        {
            int m = dt.Rows.Count;
            int n = dt.Columns.Count;
            object[] L = new object[m + 1];

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                foreach (DataRow row in dt.Rows)
                    L[k++] = row[i];

                line.MeasureWidth(L);
            }

            line.DisplayLine();

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                foreach (DataRow row in dt.Rows)
                    L[k++] = row[i];

                line.DisplayLine(L);
            }

            line.DisplayLine();

        }

    }
}

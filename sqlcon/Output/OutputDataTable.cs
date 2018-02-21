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
        private string[] headers;
        private bool vertical;

        public OutputDataTable(DataTable dt, TextWriter textWriter, bool vertical)
            : this(dt, textWriter.WriteLine, vertical)
        {

        }

        internal OutputDataTable(DataTable dt, Action<string> writeLine, bool vertical)
        {
            this.dt = dt;
            this.vertical = vertical;

            List<string> list = new List<string>();
            foreach (DataColumn column in this.dt.Columns)
                list.Add(column.ColumnName);

            this.headers = list.ToArray();

            if (vertical)
            {
                Line = new OutputDataLine(writeLine, this.dt.Rows.Count + 1);
            }
            else
            {
                Line = new OutputDataLine(writeLine, headers.Length);
            }
        }

        public OutputDataLine Line { get; }

        public void Output()
        {
            if (vertical)
                ToVerticalGrid();
            else
                ToHorizontalGrid();
        }

        private void ToHorizontalGrid()
        {
            Line.MeasureWidth(headers);
            foreach (DataRow row in dt.Rows)
            {
                Line.MeasureWidth(row.ItemArray);
            }

            Line.DisplayLine();
            Line.DisplayLine(headers);
            Line.DisplayLine();

            if (dt.Rows.Count == 0)
                return;

            foreach (DataRow row in dt.Rows)
            {
                Line.DisplayLine(row.ItemArray);
            }

            Line.DisplayLine();

        }

        private void ToVerticalGrid()
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

                Line.MeasureWidth(L);
            }

            Line.DisplayLine();

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                foreach (DataRow row in dt.Rows)
                    L[k++] = row[i];

                Line.DisplayLine(L);
            }

            Line.DisplayLine();

        }

    }
}

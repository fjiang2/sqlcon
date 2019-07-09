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
        private OutputDataLine line;

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
                line = new OutputDataLine(writeLine, this.dt.Rows.Count + 1);
            }
            else
            {
                line = new OutputDataLine(writeLine, headers.Length);
            }
        }

        public int MaxColumnWidth
        {
            get { return line.MaxColumnWidth; }
            set { line.MaxColumnWidth = value; }
        }


        public bool OutputDbNull
        {
            get { return line.OutputDbNull; }
            set { line.OutputDbNull = value; }
        }

        public void Output()
        {
            if (vertical)
                ToVerticalGrid();
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

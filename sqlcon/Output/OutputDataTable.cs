using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys.Data;

namespace sqlcon
{
    public class OutputDataTable
    {
        private DataTable dt;
        private OutputDataLine consoleLine;
        private string[] headers;
        private bool vertical;

        public Action<string> WriteLine { get; set; } = cout.TrimWriteLine;

        public OutputDataTable(DataTable table, bool vertical)
        {
            this.dt = table;
            this.vertical = vertical;

            List<string> list = new List<string>();
            foreach (DataColumn column in dt.Columns)
                list.Add(column.ColumnName);

            this.headers = list.ToArray();

            if (!vertical)
            {
                consoleLine = new OutputDataLine(WriteLine, headers.Length);
            }
            else
            {
                consoleLine = new OutputDataLine(WriteLine, dt.Rows.Count + 1);
            }
        }

        public OutputDataLine Line => consoleLine;

        public void WriteData()
        {
            if (vertical)
                ToVGrid();
            else
                ToHGrid();
        }

        private void ToHGrid()
        {
            consoleLine.MeasureWidth(headers);
            foreach (DataRow row in dt.Rows)
            {
                consoleLine.MeasureWidth(row.ItemArray);
            }

            consoleLine.DisplayLine();
            consoleLine.DisplayLine(headers);
            consoleLine.DisplayLine();

            if (dt.Rows.Count == 0)
                return;

            foreach (DataRow row in dt.Rows)
            {
                consoleLine.DisplayLine(row.ItemArray);
            }

            consoleLine.DisplayLine();

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

                consoleLine.MeasureWidth(L);
            }

            consoleLine.DisplayLine();

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                foreach (DataRow row in dt.Rows)
                    L[k++] = row[i];

                consoleLine.DisplayLine(L);
            }

            consoleLine.DisplayLine();

        }

    }
}

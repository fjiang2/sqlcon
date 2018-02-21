using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqlcon
{
    class OutputCollection<T>
    {
        private IEnumerable<T> source;
        private bool vertical;
        //private OutputDataLine line;

        public Action<string> writeLine { get; set; } = cout.TrimWriteLine;

        public OutputCollection(IEnumerable<T> source, Action<string> writeLine, bool vertical)
        {
            this.source = source;
            this.writeLine = writeLine;
            this.vertical = vertical;

            //if (vertical)
            //{
            //    line = new OutputDataLine(writeLine, dt.Rows.Count + 1);
            //}
            //else
            //{
            //    line = new OutputDataLine(writeLine, headers.Length);
            //}

        }

        public void WriteData()
        {
            if (vertical)
                ToVerticalGrid();
            else
                ToHorizontalGrid();
        }
        
        private void ToHorizontalGrid()
        {
            var properties = typeof(T).GetProperties();
            string[] headers = properties.Select(p => p.Name).ToArray();

            Func<T, object[]> selector = row =>
            {
                var values = new object[headers.Length];
                int i = 0;

                foreach (var propertyInfo in properties)
                {
                    values[i++] = propertyInfo.GetValue(row);
                }
                return values;
            };

            ToHorizontalGrid(headers, selector);
        }

        private void ToHorizontalGrid(string[] headers, Func<T, object[]> selector)
        {
            var line = new OutputDataLine(writeLine, headers.Length);

            line.MeasureWidth(headers);
            foreach (var row in source)
            {
                line.MeasureWidth(selector(row));
            }

            line.DisplayLine();
            line.DisplayLine(headers);
            line.DisplayLine();

            if (source.Count() == 0)
                return;

            foreach (var row in source)
            {
                line.DisplayLine(selector(row));
            }

            line.DisplayLine();
        }

        private void ToVerticalGrid()
        {
            var properties = typeof(T).GetProperties();
            string[] headers = properties.Select(p => p.Name).ToArray();

            Func<T, object[]> selector = row =>
            {
                var values = new object[headers.Length];
                int i = 0;

                foreach (var propertyInfo in properties)
                {
                    values[i++] = propertyInfo.GetValue(row);
                }
                return values;
            };

            ToVerticalGrid(headers, selector);
        }


        private void ToVerticalGrid(string[] headers, Func<T, object[]> selector)
        {
            int m = 1;
            int n = headers.Length;

            var line = new OutputDataLine(writeLine, m + 1);

            object[] L = new object[m + 1];
            T[] src = source.ToArray();

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                L[k++] = src[i];

                line.MeasureWidth(L);
            }

            line.DisplayLine();

            if (source.Count() == 0)
                return;

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                L[k++] = src[i];

                line.DisplayLine(L);
            }

            line.DisplayLine();
        }


    }
}

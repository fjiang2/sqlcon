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

        public Action<string> WriteLine { get; set; } = cout.TrimWriteLine;

        public OutputCollection(IEnumerable<T> source, bool vertical)
        {
            this.source = source;
            this.vertical = vertical;
        }

        public void WriteData()
        {
            if (vertical)
                ToVGrid();
            else
                ToHGrid();
        }
        
        private void ToHGrid()
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

            ToHGrid(headers, selector);
        }

        private void ToHGrid(string[] headers, Func<T, object[]> selector)
        {
            var D = new OutputDataLine(WriteLine, headers.Length);

            D.MeasureWidth(headers);
            foreach (var row in source)
            {
                D.MeasureWidth(selector(row));
            }

            D.DisplayLine();
            D.DisplayLine(headers);
            D.DisplayLine();

            if (source.Count() == 0)
                return;

            foreach (var row in source)
            {
                D.DisplayLine(selector(row));
            }

            D.DisplayLine();
        }

        private void ToVGrid()
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

            ToVGrid(headers, selector);
        }


        private void ToVGrid(string[] headers, Func<T, object[]> selector)
        {
            int m = 1;
            int n = headers.Length;

            var D = new OutputDataLine(WriteLine, m + 1);

            object[] L = new object[m + 1];
            T[] src = source.ToArray();

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                L[k++] = src[i];

                D.MeasureWidth(L);
            }

            D.DisplayLine();

            if (source.Count() == 0)
                return;

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = headers[i];
                L[k++] = src[i];

                D.DisplayLine(L);
            }

            D.DisplayLine();
        }


    }
}

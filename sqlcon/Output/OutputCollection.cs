using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace sqlcon
{
    class OutputCollection<T>
    {
        private IEnumerable<T> source;
        private bool vertical;


        public Func<T, object[]> selector { get; set; }

        public OutputCollection(IEnumerable<T> source, TextWriter textWriter, bool vertical)
            : this(source, textWriter.WriteLine, vertical)
        {

        }

        internal OutputCollection(IEnumerable<T> source, Action<string> writeLine, bool vertical)
        {
            this.source = source;
            this.vertical = vertical;

            this.Headers = defaultHeaders;
            this.selector = defaultSelector;

            if (vertical)
            {
                Line = new OutputDataLine(writeLine, 2);
            }
            else
            {
                Line = new OutputDataLine(writeLine, Headers.Length);
            }

        }

        public OutputDataLine Line { get; }
        public string[] Headers { get; set; }

        public void Output()
        {
            if (vertical)
                ToVerticalGrid();
            else
                ToHorizontalGrid();
        }



        private string[] defaultHeaders
        {
            get
            {
                var properties = typeof(T).GetProperties();
                string[] _headers = properties.Select(p => p.Name).ToArray();
                return _headers;
            }
        }

        private Func<T, object[]> defaultSelector
        {
            get
            {
                var properties = typeof(T).GetProperties();
                Func<T, object[]> _selector = row =>
                {
                    var values = new object[Headers.Length];
                    int i = 0;

                    foreach (var propertyInfo in properties)
                    {
                        values[i++] = propertyInfo.GetValue(row);
                    }

                    return values;
                };

                return _selector;
            }
        }


        private void ToHorizontalGrid()
        {
            Line.MeasureWidth(Headers);
            foreach (var row in source)
            {
                Line.MeasureWidth(selector(row));
            }

            Line.DisplayLine();
            Line.DisplayLine(Headers);
            Line.DisplayLine();

            if (source.Count() == 0)
                return;

            foreach (var row in source)
            {
                Line.DisplayLine(selector(row));
            }

            Line.DisplayLine();
        }


        private void ToVerticalGrid()
        {
            int m = 1;
            int n = Headers.Length;

            object[] L = new object[m + 1];
            T[] src = source.ToArray();

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = Headers[i];
                L[k++] = src[i];

                Line.MeasureWidth(L);
            }

            Line.DisplayLine();

            if (source.Count() == 0)
                return;

            for (int i = 0; i < n; i++)
            {
                int k = 0;
                L[k++] = Headers[i];
                L[k++] = src[i];

                Line.DisplayLine(L);
            }

            Line.DisplayLine();
        }


    }
}

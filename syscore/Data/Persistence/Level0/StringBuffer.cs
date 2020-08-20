using System.Collections.Generic;

namespace Sys.Data
{
    class StringBuffer
    {
        private int batchSize;
        private List<string> list = new List<string>();

        public int Line { get; set; }
        public int BatchLine { get; set; }
        public long TotalSize { get; set; }

        public StringBuffer(int batchSize)
        {
            this.batchSize = batchSize;
        }

        public int BatchSize => list.Count;

        public bool Append(int line, string text, bool commit)
        {
            Line = line;

            if (list.Count == 0)
                BatchLine = line;

            if (!string.IsNullOrEmpty(text))
            {
                TotalSize++;
                list.Add(text);
            }

            if (commit)
                return false;
            else
                return list.Count < batchSize;
        }

        public void Clear()
        {
            list.Clear();
        }

        public string AllText => string.Join(string.Empty, list);

        public override string ToString()
        {
            return $"BatchSize = {BatchSize}";
        }
    }
}

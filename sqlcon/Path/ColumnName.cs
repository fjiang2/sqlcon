using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data;

namespace sqlcon
{
    class ColumnPath : IDataPath
    {
        public readonly string Columns;

        public ColumnPath(string columns)
        {
            this.Columns = columns;
        }

        public string GetColumn(int i)
        {
            string[] C = Columns.Split(',');
            if (i >= 0 && i < C.Length)
                return C[i];

            return null;
        }

        public string Path
        {
            get { return this.Columns; }
        }

        public override string ToString()
        {
            return Columns;
        }
    }
}

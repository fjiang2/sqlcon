using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    class ColumnPair
    {
        public string ColumnName { get; set; }
        public ColumnValue Value;


        private const string DELIMETER = "'";

        public ColumnPair()
        { 
        }

        public ColumnPair(string columnName, object value)
        {
            this.ColumnName = columnName;
            this.Value = new ColumnValue(value);
        }

        public override string ToString()
        {
            return string.Format("[{0}] = {1}", ColumnName, Value.ToScript());
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data.Linq
{
    public class RowEvent
    {
        public string TableName { get; set; }

        public string[] PrimaryKeys { get; set; }
        
        public RowOperation Operation { get; set; }

        public IDictionary<string, object> Columns { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data.Linq
{
    public class RowEvent
    {
        public string TypeName { get; set; }

        public RowOperation Operation { get; set; }

        public IDictionary<string, object> Row { get; set; }
    }
}

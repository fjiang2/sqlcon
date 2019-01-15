using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public class SelectClause : SqlClause
    {
        public int Top { get; set; }
        public string[] Columns { get; set; }
        public string Where { get; set; }
    }
}

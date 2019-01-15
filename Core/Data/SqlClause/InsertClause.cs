using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public class InsertClause : SqlClause
    {
        public string[] Columns { get; set; }
    }
}

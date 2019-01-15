using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public class UpdateClause : SqlClause
    {
        public string[] Columns { get; set; }
        public string Where { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public class UpdateClause : SqlClause
    {
        public ColumnDescriptor[] Columns { get; set; }
        public Locator Locator { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public class DeleteClause : SqlClause
    {
        public string Where { get; set; }
    }
}

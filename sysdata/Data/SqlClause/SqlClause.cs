using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public class SqlClause
    {
        public SqlClauseAction ClauseAction { get; set; } = SqlClauseAction.Select;
        public TableName TableName { get; set; }
    }
}

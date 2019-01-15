using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public class SqlClause
    {
        public SqlClauseType ClauseType { get; set; } = SqlClauseType.Select;
        public TableName TableName { get; set; }

    }

    public enum SqlClauseType
    {
        Select,
        Insert,
        Update,
        Delete,
    }
}

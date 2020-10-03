using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sys.Data.Linq
{
    class SqlCode
    {
        class SqlStatement
        {
            public Type Table { get; set; }
            public string Statement { get; set; }
            public bool NonQuery { get; set; }

            public override string ToString() => Statement;
        }

        List<SqlStatement> clauses = new List<SqlStatement>();

        public void AppendLine<TEntity>(string clause)
        {
            clauses.Add(new SqlStatement
            {
                Table = typeof(TEntity),
                Statement = clause,
                NonQuery = true,
            });
        }

        public void AppendQuery<TEntity>(string clause)
        {
            clauses.Add(new SqlStatement
            {
                Table = typeof(TEntity),
                Statement = clause,
                NonQuery = false,
            });
        }

        public int Length => clauses.Count;

        public void Clear() => clauses.Clear();

        public string GetNonQuery()
        {
            var L = clauses.Where(x => x.NonQuery).Select(x => x.Statement);
            return string.Join(Environment.NewLine, L);
        }

        public string GetQuery()
        {
            var L = clauses.Where(x => !x.NonQuery).Select(x => x.Statement);
            return string.Join(Environment.NewLine, L);
        }

        public Type[] GetQueryTypes()
        {
            return clauses.Where(x => !x.NonQuery).Select(x => x.Table).ToArray();
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, clauses.Select(x => x.Statement));
        }
    }
}

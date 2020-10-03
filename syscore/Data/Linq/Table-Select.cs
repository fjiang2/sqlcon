using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Sys.Data.Linq
{
    public sealed partial class Table<TEntity> : ITable
    {

        public IEnumerable<TEntity> Select(Expression<Func<TEntity, bool>> where)
        {
            var translator = new QueryTranslator();
            string _where = translator.Translate(where);
            return Select(_where);
        }

        public IEnumerable<TEntity> Select(string where = null)
        {
            string SQL = SelectFromWhere(where);

            var dt = Context.FillDataTable(SQL);
            return ToList(dt);
        }

        public void SelectOnSubmit(Expression<Func<TEntity, bool>> where)
        {
            var translator = new QueryTranslator();
            string _where = translator.Translate(where);
            SelectOnSubmit(_where);
        }

        public void SelectOnSubmit(string where = null)
        {
            string SQL = SelectFromWhere(where);
            Context.CodeBlock.AppendQuery<TEntity>(SQL);
        }


        public List<TEntity> ToList(DataTable dt)
        {
            object obj = Invoke($"To{type.Name}Collection", new object[] { dt });
            return (List<TEntity>)obj;
        }


        private string SelectFromWhere(string where)
        {
            string SQL;

            if (where != null)
            {
                SQL = $"SELECT * FROM {formalName} WHERE {where}";
            }
            else
            {
                SQL = $"SELECT * FROM {formalName}";
            }

            return SQL;
        }

    }
}

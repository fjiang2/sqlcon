﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Sys.Data.Linq
{
    public sealed partial class Table<TEntity>
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
            return broker.ToList(dt);
        }

        private string SelectFromWhere(string where)
        {
            return SelectFromWhere(where, null);
        }

        internal string SelectFromWhere(string where, IEnumerable<string> columns)
        {
            string SQL;
            string _columns = "*";
            if (columns != null && columns.Count() == 0)
                _columns = string.Join(",", columns);

            if (!string.IsNullOrEmpty(where))
                SQL = $"SELECT {_columns} FROM {formalName} WHERE {where}";
            else
                SQL = $"SELECT {_columns} FROM {formalName}";

            return SQL;
        }

    }
}

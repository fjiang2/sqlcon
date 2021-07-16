using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Text;
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


        /// <summary>
        /// Select rows by primary keys in entities
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> Select(IEnumerable<TEntity> entities)
        {
            List<TEntity> list = new List<TEntity>();
            if (entities == null || entities.Count() == 0)
                return list;

            var gen = this.Generator;
            StringBuilder SQL = new StringBuilder();
            foreach (TEntity entity in entities)
            {
                foreach (string key in schema.PrimaryKeys)
                {
                    object obj = typeof(TEntity).GetProperty(key)?.GetValue(entity);
                    gen.Add(key, obj);
                }

                SQL.AppendLine(gen.Select());
                gen.Clear();
            }

            var ds = Context.FillDataSet(SQL.ToString());
            foreach (DataTable dt in ds.Tables)
            {
                if (dt.Rows.Count > 0)
                    list.AddRange(ToList(dt));
            }

            return list;
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

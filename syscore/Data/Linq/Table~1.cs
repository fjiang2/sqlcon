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
        public List<TEntity> Select(Expression<Func<TEntity, bool>> where)
        {
            var translator = new QueryTranslator();
            string _where = translator.Translate(where);
            return Select(_where);
        }

        /// <summary>
        /// Read entities from SQL Server
        /// </summary>
        /// <param name="where">default returns all entities</param>
        /// <returns></returns>
        public List<TEntity> Select(string where = null)
        {
            string SQL;

            if (where != null)
            {
                SQL = $"SELECT * FROM {tableName.FormalName} WHERE {where}";
            }
            else
            {
                SQL = $"SELECT * FROM {tableName.FormalName}";
            }

            var dt = Context.FillDataTable(SQL);
            return ToList(dt);
        }


        public List<TEntity> ToList(DataTable dt)
        {
            object obj = Invoke($"To{type.Name}Collection", new object[] { dt });
            return (List<TEntity>)obj;
        }


        public void SelectOnSubmit<T>(TEntity entity) where T : class
        {
            IAssociation assoc = schema.Associations?.FirstOrDefault(x => x.OtherType == typeof(T));
            if (assoc == null)
                return;

            var dict = ToDictionary(entity);
            object value = dict[assoc.ThisKey];
            SqlValue svalue = new SqlValue(value);

            var table = Context.GetTable<T>();
            string where = $"[{assoc.OtherKey}] = {svalue}";
            table.SelectOnSubmit(where);
        }

        public void SelectOnSubmit(Expression<Func<TEntity, bool>> where)
        {
            var translator = new QueryTranslator();
            string _where = translator.Translate(where);
            SelectOnSubmit(_where);
        }

        public void SelectOnSubmit(string where = null)
        {
            string SQL;

            if (where != null)
            {
                SQL = $"SELECT * FROM {tableName.FormalName} WHERE {where}";
            }
            else
            {
                SQL = $"SELECT * FROM {tableName.FormalName}";
            }

            Context.Script.AppendQuery<TEntity>(SQL);
        }

        
    }
}

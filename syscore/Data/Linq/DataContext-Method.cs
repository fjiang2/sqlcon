using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Sys.Data.Linq
{
    partial class DataContext
    {

        public IEnumerable<TResult> Expand<TEntity, TResult>(TEntity entity)
         where TEntity : class
         where TResult : class
        {
            var table = GetTable<TEntity>();
            return table.Expand<TResult>(entity);
        }

        public void ExpandOnSubmit<TEntity, TResult>(TEntity entity)
           where TEntity : class
           where TResult : class
        {
            var table = GetTable<TEntity>();
            table.ExpandOnSubmit<TResult>(entity);
        }

        public Type[] ExpandAllOnSubmit<TEntity>(TEntity entity)
         where TEntity : class
        {
            var table = GetTable<TEntity>();
            return table.ExpandAllOnSubmit(entity);
        }

        public Type[] ExpandAllOnSubmit<TEntity>(IEnumerable<TEntity> entities)
         where TEntity : class
        {
            var table = GetTable<TEntity>();
            return table.ExpandAllOnSubmit(entities);
        }

        public IEnumerable<TEntity> Select<TEntity>(Expression<Func<TEntity, bool>> where)
            where TEntity : class
        {
            var table = GetTable<TEntity>();
            return table.Select(where);
        }

        public void SelectOnSubmit<TEntity>(Expression<Func<TEntity, bool>> where)
            where TEntity : class
        {
            var table = GetTable<TEntity>();
            table.SelectOnSubmit(where);
        }
    }
}

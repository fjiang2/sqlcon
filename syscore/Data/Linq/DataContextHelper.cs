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

        public IEnumerable<TSubEntity> Expand<TEntity, TSubEntity>(TEntity entity)
         where TEntity : class
         where TSubEntity : class
        {
            var table = GetTable<TEntity>();
            return table.Expand<TSubEntity>(entity);
        }

        public IEnumerable<TSubEntity> Expand<TEntity, TSubEntity>(IEnumerable<TEntity> entities)
         where TEntity : class
         where TSubEntity : class
        {
            var table = GetTable<TEntity>();
            return table.Expand<TSubEntity>(entities);
        }

        public QueryResultReader Expand<TEntity>(TEntity entity)
         where TEntity : class
        {
            ExpandOnSubmit(entity);
            return SumbitQueries();
        }

        public QueryResultReader Expand<TEntity>(IEnumerable<TEntity> entities)
         where TEntity : class
        {
            ExpandOnSubmit(entities);
            return SumbitQueries();
        }

        public void ExpandOnSubmit<TEntity, TSubEntity>(TEntity entity)
           where TEntity : class
           where TSubEntity : class
        {
            var table = GetTable<TEntity>();
            table.ExpandOnSubmit<TSubEntity>(entity);
        }

        public Type[] ExpandOnSubmit<TEntity>(TEntity entity)
         where TEntity : class
        {
            var table = GetTable<TEntity>();
            return table.ExpandOnSubmit(entity);
        }

        public Type[] ExpandOnSubmit<TEntity>(IEnumerable<TEntity> entities)
         where TEntity : class
        {
            var table = GetTable<TEntity>();
            return table.ExpandOnSubmit(entities);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Sys.Data.Linq
{
    public static class Query
    {
        public static Func<string, IDbCmd> SqlCommand { get; set; } = query => new SqlCmd(query);

        public static IEnumerable<TEntity> Select<TEntity>(string where = null) where TEntity : class
        {
            using (var db = new DataContext(SqlCommand))
            {
                var table = db.GetTable<TEntity>();
                return table.Select(where);
            }
        }

        public static IEnumerable<TEntity> Select<TEntity>(this Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            using (var db = new DataContext(SqlCommand))
            {
                var table = db.GetTable<TEntity>();
                return table.Select(where);
            }
        }

        public static QueryResultReader Select(this Action<DataContext> action)
        {
            using (var db = new DataContext(SqlCommand))
            {
                action(db);
                return db.SumbitQueries();
            }
        }

        public static int Submit(this Action<DataContext> action)
        {
            using (var db = new DataContext(SqlCommand))
            {
                action(db);
                return db.SubmitChanges();
            }
        }

        public static int Submit<TEntity>(this Action<Table<TEntity>> action) where TEntity : class
        {
            using (var db = new DataContext(SqlCommand))
            {
                var table = db.GetTable<TEntity>();
                action(table);
                return db.SubmitChanges();
            }
        }

        public static int Insert<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
        {
            return Submit<TEntity>(table => table.InsertOnSubmit(entities));
        }

        public static int Update<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
        {
            return Submit<TEntity>(table => table.UpdateOnSubmit(entities));
        }

        public static int PatialUpdate<TEntity>(this IEnumerable<object> entities, bool throwException = false) where TEntity : class
        {
            return Submit<TEntity>(table => table.PartialUpdateOnSubmit(entities, throwException));
        }

        public static int InsertOrUpdate<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
        {
            return Submit<TEntity>(table => table.InsertOrUpdateOnSubmit(entities));
        }

        public static int Delete<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
        {
            return Submit<TEntity>(table => table.DeleteOnSubmit(entities));
        }

        public static IEnumerable<TSubEntity> Expand<TEntity, TSubEntity>(this IEnumerable<TEntity> entities)
         where TEntity : class
         where TSubEntity : class
        {
            using (var db = new DataContext(SqlCommand))
            {
                return db.Expand<TEntity, TSubEntity>(entities);
            }
        }

        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            return new T[] { item };
        }

        public static IEnumerable<T> Enumerable<T>(params T[] items)
        {
            return items;
        }

    }
}

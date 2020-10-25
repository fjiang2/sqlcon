using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Sys.Data.Linq
{
    public static class Query
    {
        public static Func<string, IDbCmd> DefaultSqlCommand { get; set; } = query => new SqlCmd(query);

        private static T Invoke<T>(this Func<DataContext, T> func)
        {
            using (var db = new DataContext(DefaultSqlCommand))
            {
                return func(db);
            }
        }

        public static IEnumerable<TEntity> Select<TEntity>(string where = null) where TEntity : class
        {
            return Invoke(db => db.GetTable<TEntity>().Select(where));
        }

        public static IEnumerable<TEntity> Select<TEntity>(this Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            return Invoke(db => db.GetTable<TEntity>().Select(where));
        }

        public static QueryResultReader Select(this Action<DataContext> action)
        {
            return Invoke(db =>
            {
                action(db);
                return db.SumbitQueries();
            });
        }

        public static int Submit<TEntity>(this Action<Table<TEntity>> action) where TEntity : class
        {
            return Invoke(db =>
            {
                var table = db.GetTable<TEntity>();
                action(table);
                return db.SubmitChanges();
            });
        }

        public static int Insert<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
            => Submit<TEntity>(table => table.InsertOnSubmit(entities));

        public static int Update<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
            => Submit<TEntity>(table => table.UpdateOnSubmit(entities));

        public static int PatialUpdate<TEntity>(this IEnumerable<object> entities, bool throwException = false) where TEntity : class
            => Submit<TEntity>(table => table.PartialUpdateOnSubmit(entities, throwException));

        public static int InsertOrUpdate<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
            => Submit<TEntity>(table => table.InsertOrUpdateOnSubmit(entities));

        public static int Delete<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
            => Submit<TEntity>(table => table.DeleteOnSubmit(entities));

        public static IEnumerable<TSubEntity> Expand<TEntity, TSubEntity>(this IEnumerable<TEntity> entities) where TEntity : class where TSubEntity : class
            => Invoke(db => db.Expand<TEntity, TSubEntity>(entities));

        public static QueryResultReader Expand<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
            => Invoke(db => db.Expand(entities));

        public static IEnumerable<T> AsEnumerable<T>(this T item)
            => new T[] { item };

        public static IEnumerable<T> Enumerable<T>(params T[] items)
            => items;

    }
}

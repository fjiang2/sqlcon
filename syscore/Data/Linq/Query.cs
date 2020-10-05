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

        public static int Insert<TEntity>(this TEntity entity) where TEntity : class
        {
            return Submit<TEntity>(table => table.InsertOnSubmit(entity));
        }

        public static int Update<TEntity>(this TEntity entity) where TEntity : class
        {
            return Submit<TEntity>(table => table.UpdateOnSubmit(entity));
        }

        public static int PatialUpdate<TEntity>(this TEntity entity, bool throwException = false) where TEntity : class
        {
            return Submit<TEntity>(table => table.PartialUpdateOnSubmit(entity, throwException));
        }

        public static int InsertOrUpdate<TEntity>(this TEntity entity) where TEntity : class
        {
            return Submit<TEntity>(table => table.InsertOrUpdateOnSubmit(entity));
        }

        public static int Delete<TEntity>(this TEntity entity) where TEntity : class
        {
            return Submit<TEntity>(table => table.DeleteOnSubmit(entity));
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
            return Submit<TEntity>(table =>
            {
                foreach (object entity in entities)
                    table.PartialUpdateOnSubmit(entity, throwException);
            });
        }

        public static int InsertOrUpdate<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
        {
            return Submit<TEntity>(table => table.InsertOrUpdateOnSubmit(entities));
        }

        public static int Delete<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
        {
            return Submit<TEntity>(table => table.DeleteOnSubmit(entities));
        }

        public static IEnumerable<TResult> Expand<TEntity, TResult>(this IEnumerable<TEntity> entities)
         where TEntity : class
         where TResult : class
        {
            using (var db = new DataContext(SqlCommand))
            {
                return db.Expand<TEntity, TResult>(entities);
            }
        }

        public static IEnumerable<TResult> Expand<TEntity, TResult>(this TEntity entity)
        where TEntity : class
        where TResult : class
        {
            using (var db = new DataContext(SqlCommand))
            {
                return db.Expand<TEntity, TResult>(entity);
            }
        }
    }
}

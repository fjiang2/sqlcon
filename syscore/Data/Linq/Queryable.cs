using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Sys.Data.Linq
{
    public static class Queryable
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

        public static IEnumerable<TEntity> Select<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            using (var db = new DataContext(SqlCommand))
            {
                var table = db.GetTable<TEntity>();
                return table.Select(where);
            }
        }


        public static QueryResultReader Query<TEntity1, TEntity2>(Expression<Func<TEntity1, bool>> where1, Expression<Func<TEntity2, bool>> where2)
            where TEntity1 : class
            where TEntity2 : class
        {
            using (var db = new DataContext(SqlCommand))
            {
                var t1 = db.GetTable<TEntity1>();
                t1.SelectOnSubmit(where1);

                var t2 = db.GetTable<TEntity2>();
                t2.SelectOnSubmit(where2);

                return db.SumbitQueries();
            }
        }
        public static QueryResultReader Query<TEntity1, TEntity2, TEntity3>(Expression<Func<TEntity1, bool>> where1, Expression<Func<TEntity2, bool>> where2, Expression<Func<TEntity3, bool>> where3)
            where TEntity1 : class
            where TEntity2 : class
            where TEntity3 : class
        {
            using (var db = new DataContext(SqlCommand))
            {
                var t1 = db.GetTable<TEntity1>();
                t1.SelectOnSubmit(where1);

                var t2 = db.GetTable<TEntity2>();
                t2.SelectOnSubmit(where2);

                var t3 = db.GetTable<TEntity3>();
                t3.SelectOnSubmit(where3);

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
    }
}

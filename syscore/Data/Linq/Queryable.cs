using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

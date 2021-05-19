using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Data;

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
        public static IEnumerable<TEntity> Select<TEntity>() where TEntity : class
        {
            return Invoke(db => db.GetTable<TEntity>().Select(where: string.Empty));
        }

        public static IEnumerable<TEntity> Select<TEntity>(string where = null) where TEntity : class
        {
            return Invoke(db => db.GetTable<TEntity>().Select(where));
        }

        public static IEnumerable<TEntity> Select<TEntity>(this Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            return Invoke(db => db.GetTable<TEntity>().Select(where));
        }

        public static IEnumerable<TEntity> Select<TEntity>(this Expression<Func<TEntity, object>> selectedColumns, Expression<Func<TEntity, bool>> where = null) where TEntity : class, new()
        {
            TEntity CreateInstance(System.Reflection.PropertyInfo[] properties, DataRow row, IEnumerable<string> columns)
            {
                TEntity entity = new TEntity();
                foreach (var property in properties)
                {
                    if (columns.Contains(property.Name))
                        property.SetValue(entity, row.GetField<object>(property.Name));
                }

                return entity;
            }

            return Invoke(db =>
            {
                var table = db.GetTable<TEntity>();

                List<string> _columns = new PropertyTranslator().Translate(selectedColumns);
                string _where = new QueryTranslator().Translate(where);
                string SQL = table.SelectFromWhere(_where, _columns);

                var dt = db.FillDataTable(SQL);
                if (dt == null || dt.Rows.Count == 0)
                    return new List<TEntity>();

                var properties = typeof(TEntity).GetProperties();
                return dt.ToList(row => CreateInstance(properties, row, _columns));
            });
        }

        public static IEnumerable<TResult> Select<TEntity, TKey, TResult>(this Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TResult, TKey>> resultSelector)
            where TEntity : class
            where TResult : class
        {
            var translator = new QueryTranslator();
            string _where = translator.Translate(where);
            string _keySelector = translator.Translate(keySelector);
            string _resultSelector = translator.Translate(resultSelector);

            return Invoke(db =>
            {
                var dt = db.GetTable<TEntity>();
                string SQL = $"{_resultSelector} IN (SELECT {_keySelector} FROM {dt} WHERE {_where})";
                return db.GetTable<TResult>().Select(SQL);
            });
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="modifiedProperties">The properties are modified</param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static int PatialUpdate<TEntity>(this TEntity entity, Expression<Func<TEntity, object>> modifiedProperties, Expression<Func<TEntity, bool>> where) where TEntity : class
            => Submit<TEntity>(table => table.PartialUpdateOnSubmit(entity, modifiedProperties, where));

        public static int InsertOrUpdate<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
            => Submit<TEntity>(table => table.InsertOrUpdateOnSubmit(entities));

        public static int Delete<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class
            => Submit<TEntity>(table => table.DeleteOnSubmit(entities));

        public static int Delete<TEntity>(this Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            return Submit<TEntity>(table => table.DeleteOnSubmit(where));
        }

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

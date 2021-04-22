using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sys.Data.Linq
{
    public sealed partial class Table<TEntity>
    {

        /// <summary>
        /// Expand single assoication immediately
        /// </summary>
        /// <typeparam name="TSubEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEnumerable<TSubEntity> Expand<TSubEntity>(TEntity entity) where TSubEntity : class
        {
            string where = AssociationWhere<TSubEntity>(entity);

            var table = Context.GetTable<TSubEntity>();
            return table.Select(where);
        }

        /// <summary>
        /// Expand single assoication immediately
        /// </summary>
        /// <typeparam name="TSubEntity"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<TSubEntity> Expand<TSubEntity>(IEnumerable<TEntity> entities) where TSubEntity : class
        {
            string where = AssociationWhere<TSubEntity>(entities);

            var table = Context.GetTable<TSubEntity>();
            return table.Select(where);
        }


        /// <summary>
        /// Expand single association
        /// </summary>
        /// <typeparam name="TSubEntity"></typeparam>
        /// <param name="entity"></param>
        public void ExpandOnSubmit<TSubEntity>(TEntity entity) where TSubEntity : class
        {
            string where = AssociationWhere<TSubEntity>(entity);

            var table = Context.GetTable<TSubEntity>();
            table.SelectOnSubmit(where);
        }

        /// <summary>
        /// Expand single association
        /// </summary>
        /// <typeparam name="TSubEntity"></typeparam>
        /// <param name="entities"></param>
        public void ExpandOnSubmit<TSubEntity>(IEnumerable<TEntity> entities) where TSubEntity : class
        {
            string where = AssociationWhere<TSubEntity>(entities);

            var table = Context.GetTable<TSubEntity>();
            table.SelectOnSubmit(where);
        }


        /// <summary>
        /// Expand all associations
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Type[] ExpandOnSubmit(TEntity entity)
        {
            List<Type> types = new List<Type>();
            var dict = broker.ToDictionary(entity);

            foreach (var a in schema.Constraints)
            {
                var schema = broker.GetSchmea(a.OtherType);
                var formalName = schema.FormalTableName();

                object value = dict[a.ThisKey];
                string where = Compare(a.OtherKey, value);
                var SQL = $"SELECT * FROM {formalName} WHERE {where}";

                Context.CodeBlock.AppendQuery(a.OtherType, SQL);
                types.Add(a.OtherType);
            }

            return types.ToArray();
        }

        /// <summary>
        /// Expand all associations
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public Type[] ExpandOnSubmit(IEnumerable<TEntity> entities)
        {
            List<Type> types = new List<Type>();

            foreach (var a in schema.Constraints)
            {
                var schema = broker.GetSchmea(a.OtherType);
                var formalName = schema.FormalTableName();

                string where = Compare(a.OtherKey, entities.Select(entity => broker.ToDictionary(entity)[a.ThisKey]));
                var SQL = $"SELECT * FROM {formalName} WHERE {where}";

                Context.CodeBlock.AppendQuery(a.OtherType, SQL);
                types.Add(a.OtherType);
            }

            return types.ToArray();
        }

        private string AssociationWhere<TSubEntity>(TEntity entity)
        {
            IConstraint a = schema.Constraints?.FirstOrDefault(x => x.OtherType == typeof(TSubEntity));
            if (a == null)
                throw new InvalidConstraintException($"invalid assoication from {typeof(TEntity)} to {typeof(TSubEntity)}");

            var dict = broker.ToDictionary(entity);
            object value = dict[a.ThisKey];
            return Compare(a.OtherKey, value);
        }

        private string AssociationWhere<TSubEntity>(IEnumerable<TEntity> entities)
        {
            IConstraint a = schema.Constraints?.FirstOrDefault(x => x.OtherType == typeof(TSubEntity));
            if (a == null)
                throw new InvalidConstraintException($"invalid assoication from {typeof(TEntity)} to {typeof(TSubEntity)}");

            return Compare(a.OtherKey, entities.Select(entity => broker.ToDictionary(entity)[a.ThisKey]));
        }

        private static string Compare(string column, object value)
        {
            if (value == null)
            {
                return $"[{column}] IS NULL";
            }
            else
            {
                SqlValue svalue = new SqlValue(value);
                return $"[{column}] = {svalue}";
            }
        }

        private static string Compare(string column, IEnumerable<object> values)
        {
            List<string> L = new List<string>();
            foreach (var value in values)
            {
                SqlValue svalue = new SqlValue(value);
                L.Add(svalue.ToString());
            }

            string X = string.Join(",", L);
            return $"[{column}] IN ({X})";
        }
    }
}

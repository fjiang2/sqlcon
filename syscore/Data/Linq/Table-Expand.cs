using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sys.Data.Linq
{
    public sealed partial class Table<TEntity> : ITable
    {

        /// <summary>
        /// Expand single assoication immediately
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEnumerable<TResult> Expand<TResult>(TEntity entity) where TResult : class
        {
            string where = AssociationWhere<TResult>(entity);

            var table = Context.GetTable<TResult>();
            return table.Select(where);
        }

        /// <summary>
        /// Expand single assoication immediately
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<TResult> Expand<TResult>(IEnumerable<TEntity> entities) where TResult : class
        {
            string where = AssociationWhere<TResult>(entities);

            var table = Context.GetTable<TResult>();
            return table.Select(where);
        }


        /// <summary>
        /// Expand single association
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="entity"></param>
        public void ExpandOnSubmit<TResult>(TEntity entity) where TResult : class
        {
            string where = AssociationWhere<TResult>(entity);

            var table = Context.GetTable<TResult>();
            table.SelectOnSubmit(where);
        }

        /// <summary>
        /// Expand single association
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="entities"></param>
        public void ExpandOnSubmit<TResult>(IEnumerable<TEntity> entities) where TResult : class
        {
            string where = AssociationWhere<TResult>(entities);

            var table = Context.GetTable<TResult>();
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
            var dict = ToDictionary(entity);

            foreach (var a in schema.Associations)
            {
                var schema = a.OtherType.GetTableSchema(out var _);
                var formalName = schema.FormalTableName();

                object value = dict[a.ThisKey];
                SqlValue svalue = new SqlValue(value);
                string where = $"[{a.OtherKey}] = {svalue}";

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

            foreach (var a in schema.Associations)
            {
                var schema = a.OtherType.GetTableSchema(out var _);
                var formalName = schema.FormalTableName();

                List<string> L = new List<string>();
                foreach (var entity in entities)
                {
                    var dict = ToDictionary(entity);
                    object value = dict[a.ThisKey];
                    SqlValue svalue = new SqlValue(value);
                    L.Add(svalue.ToString());
                }

                string x = string.Join(",", L);
                string where = $"[{a.OtherKey}] IN ({x})";
                var SQL = $"SELECT * FROM {formalName} WHERE {where}";
                Context.CodeBlock.AppendQuery(a.OtherType, SQL);
                types.Add(a.OtherType);
            }

            return types.ToArray();
        }

        private string AssociationWhere<TResult>(TEntity entity)
        {
            IAssociation assoc = schema.Associations?.FirstOrDefault(x => x.OtherType == typeof(TResult));
            if (assoc == null)
                throw new InvalidConstraintException($"invalid assoication from {typeof(TEntity)} to {typeof(TResult)}");

            var dict = ToDictionary(entity);
            object value = dict[assoc.ThisKey];
            SqlValue svalue = new SqlValue(value);

            return $"[{assoc.OtherKey}] = {svalue}";
        }

        private string AssociationWhere<TResult>(IEnumerable<TEntity> entities)
        {
            IAssociation assoc = schema.Associations?.FirstOrDefault(x => x.OtherType == typeof(TResult));
            if (assoc == null)
                throw new InvalidConstraintException($"invalid assoication from {typeof(TEntity)} to {typeof(TResult)}");
            List<string> L = new List<string>();
            foreach (var entity in entities)
            {
                var dict = ToDictionary(entity);
                object value = dict[assoc.ThisKey];
                SqlValue svalue = new SqlValue(value);
                L.Add(svalue.ToString());
            }

            string X = string.Join(",", L);
            return $"[{assoc.OtherKey}] IN ({X})";
        }

    }
}

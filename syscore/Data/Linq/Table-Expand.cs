using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Sys.Data.Linq
{
    public sealed partial class Table<TEntity> : ITable
    {

        public IEnumerable<TResult> Expand<TResult>(TEntity entity) where TResult : class
        {
            string where = AssociationWhere<TResult>(entity);

            var table = Context.GetTable<TResult>();
            return table.Select(where);
        }


        public void ExpandOnSubmit<TResult>(TEntity entity) where TResult : class
        {
            string where = AssociationWhere<TResult>(entity);

            var table = Context.GetTable<TResult>();
            table.SelectOnSubmit(where);
        }

        public Type[] ExpandAllOnSubmit(TEntity entity)
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
                Context.Script.AppendQuery(a.OtherType, SQL);

                types.Add(a.OtherType);
            }

            return types.ToArray();
        }

        public Type[] ExpandAllOnSubmit(IEnumerable<TEntity> entities)
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
                Context.Script.AppendQuery(a.OtherType, SQL);
                types.Add(a.OtherType);
            }

            return types.ToArray();
        }

        private string AssociationWhere<T>(TEntity entity)
        {
            IAssociation assoc = schema.Associations?.FirstOrDefault(x => x.OtherType == typeof(T));
            if (assoc == null)
                throw new InvalidConstraintException($"invalid assoication from {typeof(TEntity)} to {typeof(T)}");

            var dict = ToDictionary(entity);
            object value = dict[assoc.ThisKey];
            SqlValue svalue = new SqlValue(value);

            string where = $"[{assoc.OtherKey}] = {svalue}";
            return where;
        }

    }
}

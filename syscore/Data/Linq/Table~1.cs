﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Sys.Data.Linq
{
    public sealed partial class Table<TEntity> : ITable
    {

        public List<TResult> Expand<TResult>(TEntity entity) where TResult : class
        {
            string where = AssociationWhere<TResult>(entity);

            var table = Context.GetTable<TResult>();
            return table.Select(where);
        }


        public List<TEntity> Select(Expression<Func<TEntity, bool>> where)
        {
            var translator = new QueryTranslator();
            string _where = translator.Translate(where);
            return Select(_where);
        }

        public List<TEntity> Select(string where = null)
        {
            string SQL = SelectFromWhere(where);

            var dt = Context.FillDataTable(SQL);
            return ToList(dt);
        }


        public Type[] ExpandAllOnSubmit(TEntity entity)
        {
            List<Type> types = new List<Type>();
            foreach (var assoc in schema.Associations)
            {
                var dict = ToDictionary(entity);
                object value = dict[assoc.ThisKey];
                SqlValue svalue = new SqlValue(value);
                string where = $"[{assoc.OtherKey}] = {svalue}";

                var schema = assoc.OtherType.GetTableSchema(out var _);
                var formalName = schema.FormalTableName();
                var SQL = $"SELECT * FROM {formalName} WHERE {where}";
                Context.Script.AppendQuery(assoc.OtherType, SQL);

                types.Add(assoc.OtherType);
            }

            return types.ToArray();
        }

        public void ExpandOnSubmit<TResult>(TEntity entity) where TResult : class
        {
            string where = AssociationWhere<TResult>(entity);

            var table = Context.GetTable<TResult>();
            table.SelectOnSubmit(where);
        }


        public void SelectOnSubmit(Expression<Func<TEntity, bool>> where)
        {
            var translator = new QueryTranslator();
            string _where = translator.Translate(where);
            SelectOnSubmit(_where);
        }

        public void SelectOnSubmit(string where = null)
        {
            string SQL = SelectFromWhere(where);
            Context.Script.AppendQuery<TEntity>(SQL);
        }


        public List<TEntity> ToList(DataTable dt)
        {
            object obj = Invoke($"To{type.Name}Collection", new object[] { dt });
            return (List<TEntity>)obj;
        }


        private string SelectFromWhere(string where)
        {
            string SQL;

            if (where != null)
            {
                SQL = $"SELECT * FROM {tableName.FormalName} WHERE {where}";
            }
            else
            {
                SQL = $"SELECT * FROM {tableName.FormalName}";
            }

            return SQL;
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

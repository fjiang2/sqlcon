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
        private readonly IDataContract<TEntity> broker;
        private readonly string formalName;
        private readonly ITableSchema schema;

        public SqlMaker Generator { get; }
        public DataContext Context { get; }

        internal Table(DataContext context)
        {
            this.Context = context;
            this.broker = new DataContract1<TEntity>();
            this.schema = broker.Schema;
            this.formalName = schema.FormalTableName();

            this.Generator = new SqlMaker(formalName)
            {
                PrimaryKeys = schema.PrimaryKeys,
                IdentityKeys = schema.IdentityKeys,
            };

        }



        public void InsertOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Insert, entity);
        public void UpdateOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Update, entity);
        public void InsertOrUpdateOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.InsertOrUpdate, entity);
        public void DeleteOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Delete, entity);

        public void InsertOnSubmit(IEnumerable<TEntity> entities) => OperateOnSubmitRange(RowOperation.Insert, entities);
        public void UpdateOnSubmit(IEnumerable<TEntity> entities) => OperateOnSubmitRange(RowOperation.Update, entities);
        public void InsertOrUpdateOnSubmit(IEnumerable<TEntity> entities) => OperateOnSubmitRange(RowOperation.InsertOrUpdate, entities);
        public void DeleteOnSubmit(IEnumerable<TEntity> entities) => OperateOnSubmitRange(RowOperation.Delete, entities);
        public void PartialUpdateOnSubmit(IEnumerable<object> entities, bool throwException = false)
        {
            foreach (var entity in entities)
            {
                PartialUpdateOnSubmit(entity, throwException);
            }
        }

        private void OperateOnSubmitRange(RowOperation operation, IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                OperateOnSubmit(operation, entity);
            }
        }

        private void OperateOnSubmit(RowOperation operation, TEntity entity)
        {
            SqlMaker gen = this.Generator;

            var dict = broker.ToDictionary(entity);
            gen.AddRange(dict);

            string sql = null;
            switch (operation)
            {
                case RowOperation.Insert:
                    sql = gen.Insert();
                    break;

                case RowOperation.Update:
                    sql = gen.Update();
                    break;

                case RowOperation.InsertOrUpdate:
                    sql = gen.InsertOrUpdate();
                    break;

                case RowOperation.Delete:
                    sql = gen.Delete();
                    break;
            }

            if (sql == null)
                return;

            Context.CodeBlock.AppendLine<TEntity>(sql);

            var evt = new RowEvent
            {
                TypeName = typeof(TEntity).Name,
                Operation = operation,
                Row = gen.ToDictionary(),
            };

            Context.RowEvents.Add(evt);
            gen.Clear();
        }

        /// <summary>
        /// Update partial columns of entity, values of primary key requried
        /// </summary>
        /// <param name="entity">
        /// example of partial entity
        /// 1.object: new { Id=7, Name="XXXX"} 
        /// 2.Dictionary: new Dictionary&lt;string, object&gt;{["Id"]=7, ["Name"]="XXXX"}</string>
        /// </param>
        /// <param name="throwException">check column existence</param>
        public void PartialUpdateOnSubmit(object entity, bool throwException = false)
        {
            if (entity == null)
            {
                if (throwException)
                    throw new ArgumentNullException($"argument {nameof(entity)} cannot be null");
                else
                    return;
            }

            var gen = this.Generator;
            List<string> names = typeof(TEntity).GetProperties().Select(x => x.Name).ToList();

            if (entity is IDictionary<string, object>)
            {
                foreach (var kvp in (IDictionary<string, object>)entity)
                {
                    if (names.IndexOf(kvp.Key) == -1)
                    {
                        if (throwException)
                            throw new ArgumentException($"invalid column \"{kvp.Key}\" in Table {schema.TableName}");
                        else
                            continue;
                    }

                    gen.Add(kvp.Key, kvp.Value);
                }
            }
            else
            {
                foreach (var propertyInfo in entity.GetType().GetProperties())
                {
                    if (names.IndexOf(propertyInfo.Name) == -1)
                    {
                        if (throwException)
                            throw new ArgumentException($"invalid column \"{propertyInfo.Name}\" in Table {schema.TableName}");
                        else
                            continue;
                    }

                    object value = propertyInfo.GetValue(entity);
                    gen.Add(propertyInfo.Name, value);
                }
            }

            Context.CodeBlock.AppendLine<TEntity>(gen.Update());

            var evt = new RowEvent
            {
                TypeName = typeof(TEntity).Name,
                Operation = RowOperation.PartialUpdate,
                Row = gen.ToDictionary(),
            };

            Context.RowEvents.Add(evt);

            gen.Clear();
        }

        /// <summary>
        /// Update rows 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="modifiedProperties">The properties are modified</param>
        /// <param name="where"></param>
        public void PartialUpdateOnSubmit(TEntity entity, Expression<Func<TEntity, object>> modifiedProperties, Expression<Func<TEntity, bool>> where)
        {
            if (entity == null)
                throw new ArgumentNullException($"argument {nameof(entity)} cannot be null");

            List<string> names = new PropertyTranslator().Translate(modifiedProperties);
            string _where = new QueryTranslator().Translate(where);

            var gen = new SqlColumnValuePairCollection();
            foreach (var propertyInfo in entity.GetType().GetProperties())
            {
                if (names.IndexOf(propertyInfo.Name) == -1)
                    continue;

                object value = propertyInfo.GetValue(entity);
                gen.Add(propertyInfo.Name, value);
            }

            SqlTemplate template = new SqlTemplate(formalName);
            string update = template.Update(gen.Join(","), _where);
            Context.CodeBlock.AppendLine<TEntity>(update);

            var evt = new RowEvent
            {
                TypeName = typeof(TEntity).Name,
                Operation = RowOperation.PartialUpdate,
                Row = gen.ToDictionary(),
            };

            Context.RowEvents.Add(evt);

            gen.Clear();
            return;
        }

        public override string ToString()
        {
            return formalName;
        }
    }
}

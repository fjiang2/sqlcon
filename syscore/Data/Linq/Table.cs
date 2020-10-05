using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sys.Data.Linq
{

    public sealed partial class Table<TEntity> : ITable
    {
        private readonly Type type;
        private readonly Type extension;
        private readonly ITableSchema schema;
        private readonly MethodInfo functionToDictionary;
        private readonly string formalName;

        public SqlMaker Generator { get; }
        public DataContext Context { get; }

        internal Table(DataContext context)
        {

            this.Context = context;

            this.type = typeof(TEntity);
            this.schema = type.GetTableSchema(out var ext);
            this.extension = ext;

            this.formalName = schema.FormalTableName();

            this.Generator = new SqlMaker(schema.FormalTableName())
            {
                PrimaryKeys = schema.PrimaryKeys,
                IdentityKeys = schema.IdentityKeys,
            };

            this.functionToDictionary = extension.GetMethod(nameof(ToDictionary), BindingFlags.Public | BindingFlags.Static);
        }

        private object Invoke(string name, object[] parameters)
        {
            var methodInfo = extension.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo != null)
                return methodInfo.Invoke(null, parameters);

            return null;
        }

        private static T Invoke<T>(MethodInfo methodInfo, params object[] parameters)
        {
            if (methodInfo != null)
                return (T)methodInfo.Invoke(null, parameters);

            return default(T);
        }

        internal IDictionary<string, object> ToDictionary(TEntity entity)
        {
            return Invoke<IDictionary<string, object>>(functionToDictionary, entity);
        }

        internal TEntity FromDictionary(IDictionary<string, object> dict)
        {
            object obj = Invoke(nameof(FromDictionary), new object[] { dict });
            return (TEntity)obj;
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
            List<string> names = type.GetProperties().Select(x => x.Name).ToList();

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
            gen.Clear();
        }

        public void InsertOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Insert, entity);
        public void UpdateOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Update, entity);
        public void InsertOrUpdateOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.InsertOrUpdate, entity);
        public void DeleteOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Delete, entity);

        public void InsertOnSubmit(IEnumerable<TEntity> entities) => OperateOnSubmitRange(RowOperation.Insert, entities);
        public void UpdateOnSubmit(IEnumerable<TEntity> entities) => OperateOnSubmitRange(RowOperation.Update, entities);
        public void InsertOrUpdateOnSubmit(IEnumerable<TEntity> entities) => OperateOnSubmitRange(RowOperation.InsertOrUpdate, entities);
        public void DeleteOnSubmit(IEnumerable<TEntity> entities) => OperateOnSubmitRange(RowOperation.Delete, entities);

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

            if (functionToDictionary != null)
            {
                var dict = ToDictionary(entity);
                gen.AddRange(dict);
            }
            else
            {
                gen.AddRange(entity);
            }

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
            gen.Clear();
        }

        public override string ToString()
        {
            return formalName;
        }
    }
}

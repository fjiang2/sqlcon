using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using Tie;

namespace Sys.Data.Linq
{
    public sealed class Table<TEntity>
    {
        private readonly Type type;
        private readonly Type extension;
        private readonly ITableSchema schema;
        private readonly MethodInfo functionToDictionary;
        private readonly TableName tableName;

        public SqlMaker Generator { get; }
        public DataContext Context { get; }

        internal Table(DataContext context)
        {
            const string EXTENSION = "Extension";

            this.Context = context;

            this.type = typeof(TEntity);
            this.extension = HostType.GetType(type.FullName + EXTENSION);

            this.schema = extension.GetTableSchemaFromExtensionType();
            this.tableName = new TableName(context.ConnectionProvider, $"[{schema.SchemaName}].[{schema.TableName}]");

            this.Generator = new SqlMaker(tableName)
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

        internal IDictionary<string, object> ToDictionary(TEntity entity)
        {
            object obj = Invoke(nameof(ToDictionary), new object[] { entity });
            return (IDictionary<string, object>)obj;
        }

        internal TEntity FromDictionary(IDictionary<string, object> dict)
        {
            object obj = Invoke(nameof(FromDictionary), new object[] { dict });
            return (TEntity)obj;
        }

        private void OperateAllOnSubmit(RowOperation operation, IEnumerable<TEntity> entities)
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
                var dict = (IDictionary<string, object>)functionToDictionary.Invoke(null, new object[] { entity });
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

            Context.Script.AppendLine(sql);
            gen.Clear();
        }

        public List<TEntity> Select(Expression<Func<TEntity, bool>> where)
        {
            var translator = new QueryTranslator();
            string _where = translator.Translate(where);
            return Select(_where);
        }

        /// <summary>
        /// Read entities from SQL Server
        /// </summary>
        /// <param name="where">default returns all entities</param>
        /// <returns></returns>
        public List<TEntity> Select(string where = null)
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

            var dt = Context.FillDataTable(SQL);
            return ToList(dt);
        }


        /// <summary>
        /// Use any data from DataTable instance
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<TEntity> ToList(DataTable dt)
        {
            object obj = Invoke($"To{type.Name}Collection", new object[] { dt });
            return (List<TEntity>)obj;
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
        public void UpdateOnSubmit(object entity, bool throwException = false)
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

            Context.Script.AppendLine(gen.Update());
            gen.Clear();
        }

        /// <summary>
        /// Insert entity on submit
        /// </summary>
        /// <param name="entity"></param>
        public void InsertOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Insert, entity);

        /// <summary>
        /// Update entity on submit
        /// </summary>
        /// <param name="entity"></param>
        public void UpdateOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Update, entity);

        /// <summary>
        /// Insert or update entity on submit
        /// </summary>
        /// <param name="entity"></param>
        public void InsertOrUpdateOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.InsertOrUpdate, entity);

        /// <summary>
        /// Delete entity on submit
        /// </summary>
        /// <param name="entity"></param>
        public void DeleteOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Delete, entity);

        /// <summary>
        /// Insert entities on submit
        /// </summary>
        /// <param name="entities"></param>
        public void InsertAllOnSubmit(IEnumerable<TEntity> entities) => OperateAllOnSubmit(RowOperation.Insert, entities);

        /// <summary>
        /// Update entities on submit
        /// </summary>
        /// <param name="entities"></param>
        public void UpdateAllOnSubmit(IEnumerable<TEntity> entities) => OperateAllOnSubmit(RowOperation.Update, entities);

        /// <summary>
        /// Insert or update entities on submit
        /// </summary>
        /// <param name="entities"></param>
        public void InsertOrUpdateAllOnSubmit(IEnumerable<TEntity> entities) => OperateAllOnSubmit(RowOperation.InsertOrUpdate, entities);

        /// <summary>
        /// Delete entities on submit
        /// </summary>
        /// <param name="entities"></param>
        public void DeleteAllOnSubmit(IEnumerable<TEntity> entities) => OperateAllOnSubmit(RowOperation.Delete, entities);



        public override string ToString()
        {
            return tableName.FullName;
        }
    }
}

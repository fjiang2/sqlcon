using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;

using Tie;

namespace Sys.Data.Linq
{
    public sealed class Table<TEntity>
    {
        private const string EXTENSION = "Extension";
        private const string TABLENAME = "TableName";
        private const string KEYS = "Keys";
        private const string IDENTITY = "Identity";

        private Type type;
        private Type extension;
        private string entityName;
        private Lazy<SqlMaker> lazyGen;

        public DataContext Context { get; }

        internal Table(DataContext context)
        {
            this.Context = context;

            this.type = typeof(TEntity);
            this.entityName = type.Name;

            string ext = type.FullName + EXTENSION;
            this.extension = HostType.GetType(ext);

            this.lazyGen = new Lazy<SqlMaker>(() => new SqlMaker(Context.ConnectionProvider, GetTableSchema()));
        }


        private ITableSchema GetTableSchema()
        {
            string tableName = GetField(TABLENAME, string.Empty);
            string[] keys = GetField(KEYS, new string[] { });
            string[] identity = GetField(IDENTITY, new string[] { });

            return new TableSchema
            {
                TableName = tableName,
                PrimaryKeys = keys,
                IdentityKeys = identity,
            };
        }

        private DataTable FillDataTable(string where)
        {
            if (where == null)
                return Context.FillDataTable($"SELECT * FROM [{entityName}]");
            else
                return Context.FillDataTable($"SELECT * FROM [{entityName}] WHERE {where}");
        }

        private object Invoke(string name, object[] parameters)
        {
            var methodInfo = extension.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo != null)
                return methodInfo.Invoke(null, parameters);

            return null;
        }

        private T GetField<T>(string name, T defaultValue = default(T))
        {
            var fieldInfo = extension.GetField(name, BindingFlags.Public | BindingFlags.Static);
            if (fieldInfo != null)
                return (T)fieldInfo.GetValue(null);
            else
                return defaultValue;
        }

        public List<TEntity> ToList(string where = null)
        {
            var dt = FillDataTable(where);
            return ToList(dt);
        }

        public List<TEntity> ToList(DataTable dt)
        {
            object obj = Invoke($"To{entityName}Collection", new object[] { dt });
            return (List<TEntity>)obj;
        }

        public IDictionary<string, object> ToDictionary(TEntity entity)
        {
            object obj = Invoke(nameof(ToDictionary), new object[] { entity });
            if (obj != null)
                return (IDictionary<string, object>)obj;
            return null;
        }

        public TEntity FromDictionary(IDictionary<string, object> dict)
        {
            object obj = Invoke(nameof(FromDictionary), new object[] { dict });
            return (TEntity)obj;
        }


        public void InsertOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Insert, entity);
        public void UpdateOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Update, entity);
        public void InsertOrUpdateOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.InsertOrUpdate, entity);
        public void DeleteOnSubmit(TEntity entity) => OperateOnSubmit(RowOperation.Delete, entity);

        public void InsertAllOnSubmit(IEnumerable<TEntity> entities) => OperateAllOnSubmit(RowOperation.Insert, entities);
        public void UpdateAllOnSubmit(IEnumerable<TEntity> entities) => OperateAllOnSubmit(RowOperation.Update, entities);
        public void InsertOrUpdateAllOnSubmit(IEnumerable<TEntity> entities) => OperateAllOnSubmit(RowOperation.InsertOrUpdate, entities);
        public void DeleteOnAllSubmit(IEnumerable<TEntity> entities) => OperateAllOnSubmit(RowOperation.Delete, entities);

        private void OperateAllOnSubmit(RowOperation operation, IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                OperateOnSubmit(operation, entity);
            }
        }

        private void OperateOnSubmit(RowOperation operation, TEntity entity)
        {
            SqlMaker gen = lazyGen.Value;
            
            var dict = ToDictionary(entity);

            //if method ToDictionary is undefined
            if (dict != null)   
                gen.AddRange(dict);
            else
                gen.AddRange(entity);

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

            if (sql != null)
                Context.AppendScript(sql);
        }

        public override string ToString()
        {
            return entityName;
        }
    }
}

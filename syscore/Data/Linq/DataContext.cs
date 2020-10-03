using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sys.Data.Linq
{

    public class DataContext : IDisposable
    {
        internal ConnectionProvider ConnectionProvider { get; }
        internal SqlCode Script { get; } = new SqlCode();

        private Dictionary<Type, ITable> tables = new Dictionary<Type, ITable>();

        public DataContext(string connectionString)
        {
            this.ConnectionProvider = ConnectionProvider.CreateProvider("ServerName", connectionString);
        }

        public DataContext(ConnectionProvider provider)
        {
            this.ConnectionProvider = provider;
        }

        public void Dispose()
        {
            Script.Clear();
            tables.Clear();
        }

        public Table<TEntity> GetTable<TEntity>() where TEntity : class
        {
            Type key = typeof(TEntity);
            if (tables.ContainsKey(key))
                return (Table<TEntity>)tables[key];

            var obj = new Table<TEntity>(this);
            tables.Add(key, obj);
            return obj;
        }

        public string GetNonQueryScript()
        {
            return Script.GetNonQuery();
        }

        public string GetQueryScript()
        {
            return Script.GetQuery();
        }

        public List<TOther> Select<TEntity, TOther>(TEntity entity)
            where TEntity : class
            where TOther : class
        {
            var table = GetTable<TEntity>();
            return table.Select<TOther>(entity);
        }

        public void SelectOnSubmit<TEntity, TOther>(TEntity entity)
           where TEntity : class
           where TOther : class
        {
            var table = GetTable<TEntity>();
            table.SelectOnSubmit<TOther>(entity);
        }

        public List<TEntity> Select<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            var table = GetTable<TEntity>();
            return table.Select(where);
        }

        public void SelectOnSubmit<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            var table = GetTable<TEntity>();
            table.SelectOnSubmit(where);
        }

        internal DataTable FillDataTable(string query)
        {
            var cmd = new SqlCmd(ConnectionProvider, query);
            return cmd.FillDataTable();
        }

        private DataSet FillDataSet(string query)
        {
            var cmd = new SqlCmd(ConnectionProvider, query);
            return cmd.FillDataSet();
        }

        public QueryResultReader SumbitQueries()
        {
            if (Script.Length == 0)
                return null;

            string query = Script.GetQuery();
            Type[] types = Script.GetQueryTypes();
            var ds = FillDataSet(query);
            Script.Clear();

            return new QueryResultReader(this, types, ds);
        }

        public void SubmitChanges()
        {
            if (Script.Length == 0)
                return;

            var cmd = new SqlCmd(ConnectionProvider, Script.GetNonQuery());
            cmd.ExecuteNonQuery();
            Script.Clear();
        }


        public override string ToString()
        {
            return ConnectionProvider.ConnectionString;
        }
    }
}

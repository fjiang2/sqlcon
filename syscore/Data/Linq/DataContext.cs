using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace Sys.Data.Linq
{

    public partial class DataContext : IDisposable
    {
        private readonly Dictionary<Type, ITable> tables = new Dictionary<Type, ITable>();
        private readonly Func<string, IDbCmd> sqlCommand;

        internal SqlCodeBlock CodeBlock { get; } = new SqlCodeBlock();
        internal List<RowEvent> RowEvents { get; } = new List<RowEvent>();

        public string Description { get; set; }

        public event EventHandler<RowEventArgs> RowChanging;
        public event EventHandler<RowEventArgs> RowChanged;

        public DataContext()
        {
            this.sqlCommand = query => new SqlCmd(query);
            this.Description = "Default SQL command handler";
        }

        public DataContext(string connectionString)
        {
            var connectionProvider = ConnectionProvider.CreateProvider("ServerName", connectionString);
            this.sqlCommand = query => new SqlCmd(connectionProvider, query);
            this.Description = connectionString;
        }

        public DataContext(ConnectionProvider provider)
        {
            this.sqlCommand = query => new SqlCmd(provider, query);
            this.Description = provider.ConnectionString;
        }

        public DataContext(Func<string, IDbCmd> cmd)
        {
            this.sqlCommand = cmd;
            this.Description = "SQL command handler";
        }

        public void Dispose()
        {
            CodeBlock.Clear();
            tables.Clear();
        }

        protected void OnRowChanging(IEnumerable<RowEvent> evt)
        {
            RowChanging?.Invoke(this, new RowEventArgs(evt));
        }

        protected void OnRowChanged(IEnumerable<RowEvent> evt)
        {
            RowChanged?.Invoke(this, new RowEventArgs(evt));
        }

        public Table<TEntity> GetTable<TEntity>()
            where TEntity : class
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
            return CodeBlock.GetNonQuery();
        }

        public string GetQueryScript()
        {
            return CodeBlock.GetQuery();
        }



        internal DataTable FillDataTable(string query)
        {
            DataSet ds = FillDataSet(query);
            if (ds == null)
                return null;

            if (ds.Tables.Count >= 1)
                return ds.Tables[0];

            return null;
        }

        private DataSet FillDataSet(string query)
        {
            var cmd = sqlCommand(query);
            var ds = new DataSet();
            return cmd.FillDataSet(ds);
        }

        public QueryResultReader SumbitQueries()
        {
            if (CodeBlock.Length == 0)
                return null;

            string query = CodeBlock.GetQuery();
            Type[] types = CodeBlock.GetQueryTypes();
            var ds = FillDataSet(query);
            CodeBlock.Clear();

            return new QueryResultReader(this, types, ds);
        }

        public int SubmitChanges()
        {
            if (CodeBlock.Length == 0)
                return -1;
            
            OnRowChanging(RowEvents);
            
            var cmd = sqlCommand(CodeBlock.GetNonQuery());
            int count = cmd.ExecuteNonQuery();
            CodeBlock.Clear();

            OnRowChanged(RowEvents);
            RowEvents.Clear();

            return count;
        }


        public override string ToString()
        {
            return Description;
        }

    }
}

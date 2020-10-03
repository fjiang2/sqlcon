using System;
using System.Text;
using System.Data;

namespace Sys.Data.Linq
{

    public class DataContext : IDisposable
    {
        internal ConnectionProvider ConnectionProvider { get; }
        internal SqlCode Script { get; } = new SqlCode();

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

        }

        public Table<TEntity> GetTable<TEntity>() where TEntity : class
        {
            return new Table<TEntity>(this);
        }

        public string GetNonQueryScript()
        {
            return Script.GetNonQuery();
        }

        public string GetQueryScript()
        {
            return Script.GetQuery();
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

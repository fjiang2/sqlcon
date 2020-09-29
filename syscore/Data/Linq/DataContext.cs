using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Sys.Data.Linq
{
    public class DataContext : IDisposable
    {
        internal ConnectionProvider ConnectionProvider { get; }
        public StringBuilder Script { get; } = new StringBuilder();

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

        internal DataTable FillDataTable(string sql)
        {
            var cmd = new SqlCmd(ConnectionProvider, sql);
            return cmd.FillDataTable();
        }

        public string GetScript()
        {
            return Script.ToString();
        }

        public void SubmitChanges()
        {
            if (Script.Length == 0)
                return;

            var cmd = new SqlCmd(ConnectionProvider, Script.ToString());
            cmd.ExecuteNonQuery();
            Script.Clear();
        }

        public override string ToString()
        {
            return ConnectionProvider.ConnectionString;
        }
    }
}

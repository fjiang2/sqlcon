using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Sys.Data.Linq
{
    public class DataContext : IDisposable
    {
        internal ConnectionProvider ConnectionProvider { get; }
        StringBuilder script = new StringBuilder();

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

        internal void AppendScript(string sql)
        {
            script.AppendLine(sql);
        }

        public string GetScript()
        {
            return script.ToString();
        }

        public void SubmitChanges()
        {
            var cmd = new SqlCmd(ConnectionProvider, script.ToString());
            cmd.ExecuteNonQuery();
        }

        public override string ToString()
        {
            return ConnectionProvider.ConnectionString;
        }
    }
}

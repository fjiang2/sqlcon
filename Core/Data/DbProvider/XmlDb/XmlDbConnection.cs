using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.Common;
using System.Data;
using Sys.Data.IO;

namespace Sys.Data
{
    public sealed class XmlDbConnection : DbConnection
    {
        public override string ConnectionString { get; set; }
        public override string Database { get { return this.database; } }
        public override string DataSource { get; }
        public override string ServerVersion { get; }
        public override ConnectionState State { get { return this.state; } }

        public ConnectionProvider Provider { get; }
        public FileLink FileLink { get; }


        public XmlDbConnection(ConnectionProvider provider)
        {
            this.Provider = provider;
            this.ConnectionString = Provider.ConnectionString;
            this.DataSource = Provider.DataSource;
            this.database = Provider.InitialCatalog;

            this.FileLink = FileLink.CreateLink(provider.DataSource, provider.UserId, provider.Password);
        }

        public DatabaseName DatabaseName
        {
            get
            {
                return new DatabaseName(Provider, database);
            }
        }


        public override void ChangeDatabase(string databaseName)
        {
            this.database = databaseName;
        }

        public override void Close()
        {
            state = ConnectionState.Closed;
        }

        public override void Open()
        {
            state = ConnectionState.Open;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return null;
        }

        protected override DbCommand CreateDbCommand()
        {
            return new XmlDbCommand("", this);
        }

        private string database;
        private ConnectionState state = ConnectionState.Closed;

    }
}

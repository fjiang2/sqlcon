using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace Sys.Data
{
    public sealed class XmlDbConnection : DbConnection
    {
        public override string ConnectionString { get; set; }
        public override string Database { get; }
        public override string DataSource { get; }
        public override string ServerVersion { get; }
        public override ConnectionState State { get; }

        public XmlDbConnection(string connectionString)
        {
        }


        public override void ChangeDatabase(string databaseName)
        { }

        public override void Close()
        { }

        public override void Open()
        { }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return null;
        }

        protected override DbCommand CreateDbCommand()
        {
            return null;
        }
    }
}

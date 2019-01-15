using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data.IO;

namespace Sys.Data
{
    public sealed class FileDbCommand : DbCommand
    {
        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection { get; }
        protected override DbTransaction DbTransaction { get; set; }


        public FileDbCommand(string cmdText, FileDbConnection connection)
        {
            this.CommandText = cmdText;
            this.CommandType = CommandType.Text;
            this.DbConnection = connection;

        }

        public override void Cancel()
        {
        }

        public override int ExecuteNonQuery()
        {
            FileDbConnection connection = DbConnection as FileDbConnection;
            FileLink link = connection.FileLink;

            var parser = new SqlClauseParser(connection.Provider, CommandText);
            SqlClause clause = parser.Parse();

            return -1;
        }

        public override object ExecuteScalar()
        {
            return null;
        }

        public override void Prepare()
        {

        }
        protected override DbParameter CreateDbParameter()
        {
            return null;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return null;
        }
    }
}

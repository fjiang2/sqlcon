using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using Sys.Networking;

namespace Sys.Data
{
    public sealed class RiaDbCommand : DbCommand
    {
        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection { get; }
        protected override DbTransaction DbTransaction { get; set; }


        private RemoteInvoke agent;

        public RiaDbCommand(string cmdText, RiaDbConnection connection)
        {
            this.CommandText = cmdText;
            this.CommandType = CommandType.Text;
            this.DbConnection = connection;

            this.agent = new RemoteInvoke(new Uri(connection.Provider.DataSource));
        }

        public override void Cancel()
        {
        }


        public override int ExecuteNonQuery()
        {
            string code = $"var cmd=new SqlCmd('{CommandText}'); result= cmd.ExecuteNonQuery();";
            agent.Execute(code);
            return agent.GetValue<int>("result");
        }

        public override object ExecuteScalar()
        {
            string code = $"var cmd=new SqlCmd('{CommandText}'); result= cmd.ExecuteScalar();";
            agent.Execute(code);
            return agent.GetValue<object>("result");
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

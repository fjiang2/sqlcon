using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using Sys.Networking;

namespace Sys.Data
{
    public class RiaDbDataAdapter : DbDataAdapter
    {
        RiaDbCommand command;
        RiaDbConnection connection;
        ConnectionProvider provider;

        public RiaDbDataAdapter()
        {
        }

        public override int Fill(DataSet dataSet)
        {
            command = (RiaDbCommand)this.SelectCommand;
            connection = (RiaDbConnection)command.Connection;
            provider = connection.Provider;
            var parameters = this.GetFillParameters();

            RemoteInvoke agent = new RemoteInvoke(new Uri(provider.DataSource));
            string sql = command.CommandText;
            string code = $"var cmd=new tw.Common.SqlCmdDirect('{sql}');ds=cmd.FillDataSet();";
            agent.Execute(code);

            var ds = agent.GetValue<DataSet>("ds");
            if (ds != null)
            {
                foreach (DataTable dt in ds.Tables)
                {
                    dataSet.Tables.Add(dt.Copy());
                }
            }

            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace Sys.Data
{
    public class RiaDbDataAdapter : DbDataAdapter
    {
        SqlCommand command;
        SqlConnection connection;
        ConnectionProvider provider;

        public RiaDbDataAdapter()
        {
        }

        public override int Fill(DataSet dataSet)
        {
            command = (SqlCommand)this.SelectCommand;
            connection = (SqlConnection)command.Connection;
           // provider = connection.Provider;

            string sql = command.CommandText;
            return 0;
        }
    }
}

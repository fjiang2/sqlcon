using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data
{
    static class ConnectionProviderExtension
    {
        public static object ExecuteScalar(this ConnectionProvider provider, string script, params object[] args)
        {
            SqlCmd cmd = new SqlCmd(provider, string.Format(script, args));
            return cmd.ExecuteScalar();
        }

        public static int ExecuteNonQuery(this ConnectionProvider provider, string script)
        {
            SqlCmd cmd = new SqlCmd(provider, script);
            return cmd.ExecuteNonQuery();
        }

        public static DataTable FillDataTable(this DatabaseName dname, string script)
        {
            SqlCmd cmd = new SqlCmd(dname.Provider, script);
            return cmd.FillDataTable();
        }
        public static DataTable FillDataTable(this TableName tname, string script)
        {
            SqlCmd cmd = new SqlCmd(tname.Provider, script);
            return cmd.FillDataTable();
        }

        public static DataTable FillDataTable(this ConnectionProvider provider, string script)
        {
            SqlCmd cmd = new SqlCmd(provider, script);
            return cmd.FillDataTable();
        }
    }
}

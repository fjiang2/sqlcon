using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;

namespace Sys.Data
{
    public class SqlExceptionEventArgs
    {
        public readonly Exception Exception;
        public readonly string Command;
        public int Line { get; set; }

        public SqlExceptionEventArgs(DbCommand cmd, Exception ex)
        {
            this.Command = cmd.CommandText;
            this.Exception = ex;
        }

        public SqlExceptionEventArgs(string command, Exception ex)
        {
            this.Command = command;
            this.Exception = ex;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Sys.Data
{
    public static class ExceptionExtension
    {
        public static string AllMessage(this SqlException ex, string sql = null)
        {
            StringBuilder builder = new StringBuilder();
            if (sql != null)
                builder.AppendLine(sql);

            for (int i = 0; i < ex.Errors.Count; i++)
            {
                var error = ex.Errors[i];
                builder
                    .AppendLine("Message: " + error.Message)
                    .AppendLine("LineNumber: " + error.LineNumber);
                //.AppendLine("Source: " + error.Source)
                //.AppendLine("Procedure: " + error.Procedure);
            }

            return builder.ToString();
        }
    }
}

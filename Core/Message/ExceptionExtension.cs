using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Sys
{
    public static class ExceptionExtension
    {
        public static string AllMessage(this SqlException ex)
        {
            StringBuilder builder = new StringBuilder(ex.Message);
            for (int i = 0; i < ex.Errors.Count; i++)
            {
                var error = ex.Errors[i];
                builder
                    .AppendLine("Message: " + error.Message)
                    .AppendLine("LineNumber: " + error.LineNumber)
                    .AppendLine("Source: " + error.Source)
                    .AppendLine("Procedure: " + error.Procedure);
            }

            return builder.ToString();
        }

        public static string AllMessage(this Exception exception)
        {
            StringBuilder builder = new StringBuilder(exception.Message);

            Exception innerException = exception.InnerException;
            while (innerException != null)
            {
                builder.AppendLine();
                builder.AppendLine(innerException.Message);

                innerException = innerException.InnerException;
            }

            return builder.ToString();
        }

    }
}

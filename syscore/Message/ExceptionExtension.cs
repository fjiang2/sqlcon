using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys
{
    public static class ExceptionExtension
    {
        public static string AllMessages(this Exception exception)
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

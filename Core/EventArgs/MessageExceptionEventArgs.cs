using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys
{
    public class MessageExceptionEventArgs : EventArgs
    {
        public readonly MessageException Exception;

        public MessageExceptionEventArgs(MessageException exception)
        {
            this.Exception = exception;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys
{
    public class MessageEventArgs : EventArgs
    {
        public readonly Message Message;

        public MessageEventArgs(Message message)
        {
            this.Message = message;
        }
    }
}

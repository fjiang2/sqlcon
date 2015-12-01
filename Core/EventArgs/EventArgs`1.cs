using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys
{
    public class EventArgs<T> : EventArgs
    {
        public readonly T Value;

        public EventArgs(T value)
        {
            this.Value = value;
        }
    }
}

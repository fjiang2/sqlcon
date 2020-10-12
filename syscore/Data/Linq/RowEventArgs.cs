using System;
using System.Collections.Generic;

namespace Sys.Data.Linq
{
    public class RowEventArgs: EventArgs
    {
        public IEnumerable<RowEvent> Events { get; }
        public RowEventArgs(IEnumerable<RowEvent> events)
        {
            this.Events = events;
        }
    }
}

using System;

namespace Sys.Data
{
    public class SqlExecutionEventArgs : EventArgs
    {
        public int BatchLine { get; set; }
        
        public int BatchSize { get; set; }

        public long Line { get; set; }

        public long TotalSize { get; set; }

        public string CommandText { get; }

        public SqlExecutionEventArgs(string command)
        {
            this.CommandText = command;
        }

        public override string ToString()
        {
            return $"{BatchLine}/{BatchSize} : {CommandText}";
        }
    }


}

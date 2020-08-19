using System;

namespace Sys.Data
{
    public class SqlExecutionEventArgs : EventArgs
    {
        public int Line { get; set; }
        public int BatchSize { get; set; }

        public string CommandText { get; }

        public SqlExecutionEventArgs(string command)
        {
            this.CommandText = command;
        }

        public int StopLine => Line + BatchSize - 1;

        public override string ToString()
        {
            return $"{Line} - {StopLine} : {CommandText}";
        }
    }


}

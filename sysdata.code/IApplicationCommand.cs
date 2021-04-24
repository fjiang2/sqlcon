using System.Collections.Generic;
using Sys.Stdio;

namespace sqlcon
{
    public interface IApplicationCommand : ICommand
    {

        string InputPath();
        string OutputPath();
    }
}
using System.Collections.Generic;
using Sys.Stdio;

namespace sqlcon
{
    interface IApplicationCommand : ICommand
    {

        string InputPath();
        string OutputPath();
    }
}
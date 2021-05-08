using System.Collections.Generic;

namespace Sys.Stdio
{
    public interface IApplicationCommand : ICommand
    {

        string InputPath();
        string OutputPath();
    }
}
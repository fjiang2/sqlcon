namespace Sys.Stdio
{
    public interface IShell
    {
        NextStep Run(string line);
    }
}
namespace Sys.Stdio
{
    public interface ITabCompletion
    {
        string[] TabCandidates(string argument);
    }
}

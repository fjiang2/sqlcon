namespace sqlcon
{
    public interface ITabCompletion
    {
        string[] TabCandidates(string argument);
    }
}

namespace Sys
{
    public interface IWildcard
    {
        string[] Excludes { get; set; }
        string[] Includes { get; set; }
        string Pattern { get; set; }
    }
}
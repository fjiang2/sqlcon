namespace Sys
{
    public interface IWildcard
    {
        /// <summary>
        /// matches all if it were null
        /// </summary>
        string Pattern { get; set; }

        /// <summary>
        /// Inclusive pattern list
        /// include all if it were empty
        /// </summary>
        string[] Includes { get; set; }

        /// <summary>
        /// Exclusive pattern list
        /// Exclude nothing if it were empty
        /// </summary>
        string[] Excludes { get; set; }
    }
}
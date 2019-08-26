using System.Linq;
using System.Text.RegularExpressions;
using Sys;
using Sys.Data;

namespace sqlcon
{
    class MatchedDataPath<TPath> where TPath : IDataPath
    {
        private TPath[] paths { get; }

        public string Pattern { get; set; }
        public string[] Includes { get; set; } = new string[] { };
        public string[] Excludes { get; set; } = new string[] { };

        public MatchedDataPath(TPath[] paths)
        {
            this.paths = paths;
        }

        public MatchedDataPath(TPath[] paths, ApplicationCommand cmd)
        {
            this.paths = paths;
            this.Pattern = cmd.wildcard;
            this.Includes = cmd.Includes;
            this.Excludes = cmd.Excludes;
        }

        public TPath[] Results()
        {
            var names = this.paths
                .Where(name => Include(name) && !Exclude(name))
                .ToArray();

            if (Pattern == null)
                return names;

            names = Search(Pattern, names);

            return names;
        }

        public bool Contains(TPath path)
        {
            if (!Include(path) && !Exclude(path))
                return false;

            return Pattern.IsMatch(path.Path);
        }

        private bool Include(TPath path)
        {
            if (Includes == null || Includes.Length == 0)
                return true;

            return Includes.IsMatch(path.Path);
        }

        private bool Exclude(TPath path)
        {
            if (Excludes == null || Excludes.Length == 0)
                return false;

            return Excludes.IsMatch(path.Path);
        }

        private TPath[] Search(string pattern, TPath[] paths)
        {
            return paths.Where(x => pattern.IsMatch(x.Path)).ToArray();
        }

    }
}

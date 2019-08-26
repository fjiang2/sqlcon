using System.Linq;
using System.Text.RegularExpressions;
using Sys;
using Sys.Data;

namespace sqlcon
{
    class MatchedTable
    {
        private TableName[] tnames { get; }

        public string Pattern { get; set; }
        public string[] Includes { get; set; } = new string[] { };
        public string[] Excludes { get; set; } = new string[] { };

        public MatchedTable(TableName[] tnames)
        {
            this.tnames = tnames;
        }

        public MatchedTable(TableName[] tnames, ApplicationCommand cmd)
        {
            this.tnames = tnames;
            this.Pattern = cmd.wildcard;
            this.Includes = cmd.Includes;
            this.Excludes = cmd.Excludes;
        }

        private string Compare(TableName tname)
        {
            if (Pattern.StartsWith("dbo."))
                return tname.Path;
            else
                return tname.ShortName;
        }

        public TableName[] Results()
        {
            var names = this.tnames
                .Where(name => Include(name) && !Exclude(name))
                .ToArray();

            if (Pattern == null)
                return names;

            names = Search(Pattern, names);

            return names;
        }

        public bool Contains(TableName tname)
        {
            if (!Include(tname) || Exclude(tname))
                return false;

            if (Pattern == null)
                return true;

            return Pattern.IsMatch(tname.ShortName);
        }

        private bool Include(TableName tname)
        {
            if (Includes == null || Includes.Length == 0)
                return true;

            return Includes.IsMatch(tname.ShortName);
        }

        private bool Exclude(TableName tname)
        {
            if (Excludes == null || Excludes.Length == 0)
                return false;

            return Excludes.IsMatch(tname.ShortName);
        }

        private static TableName[] Search(string pattern, TableName[] tnames)
        {
            return tnames.Where(x => pattern.IsMatch(x.ShortName)).ToArray();
        }

    }
}

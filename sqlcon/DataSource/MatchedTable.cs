using System.Linq;
using System.Text.RegularExpressions;
using Sys;
using Sys.Data;

namespace sqlcon
{
    class MatchedTable
    {
        private TableName[] tableNames { get; }

        public string Pattern { get; set; }
        public string[] IncludedTables { get; set; } = new string[] { };
        public string[] ExcludedTables { get; set; } = new string[] { };

        public MatchedTable(TableName[] tnames)
        {
            this.tableNames = tnames;
        }

        public MatchedTable(TableName[] tnames, ApplicationCommand cmd)
        {
            this.tableNames = tnames;
            this.Pattern = cmd.wildcard;
            this.IncludedTables = cmd.Includes;
            this.ExcludedTables = cmd.Excludes;
        }

        public TableName[] MatchedTables()
        {
            var names = this.tableNames
                .Where(name => Includes(name) && !Excludes(name))
                .ToArray();

            if (Pattern == null)
                return names;

            names = Search(Pattern, names);

            return names;
        }

        public bool Contains(TableName tname)
        {
            if (!Includes(tname) && !Excludes(tname))
                return false;

            return Pattern.IsMatch(tname.Path);
        }

        private bool Includes(TableName tableName)
        {
            if (IncludedTables == null || IncludedTables.Length == 0)
                return true;

            return IncludedTables.IsMatch(tableName.ShortName);
        }

        private bool Excludes(TableName tableName)
        {
            if (ExcludedTables == null || ExcludedTables.Length == 0)
                return false;

            return ExcludedTables.IsMatch(tableName.ShortName);
        }

        public static TableName[] Search(string pattern, TableName[] tableNames)
        {
            var result = tableNames.Where(tname => pattern.IsMatch(tname.Path)).ToArray();
            return result;
        }

    }
}

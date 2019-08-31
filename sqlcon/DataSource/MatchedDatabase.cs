using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Sys;
using Sys.Data;

namespace sqlcon
{
    class MatchedDatabase
    {
        private string Pattern;
        private DatabaseName DatabaseName;

        public string[] Includedtables { get; set; }
        public string[] Excludedtables { get; set; }

        public MatchedDatabase(DatabaseName databaseName, ApplicationCommand cmd)
            : this(databaseName, cmd.wildcard)
        {
            this.Includedtables = cmd.Includes;
            this.Excludedtables = cmd.Excludes;
        }

        public MatchedDatabase(DatabaseName databaseName, string pattern)
        {
            this.Pattern = pattern;
            this.DatabaseName = databaseName;
        }

        public TableName[] TableNames()
        {
            TableName[] names = this.DatabaseName.GetDependencyTableNames();
            return Search(names);
        }

        public TableName[] ViewNames()
        {
            TableName[] names = this.DatabaseName.GetViewNames();
            return Search(names);
        }

        private TableName[] Search(TableName[] names)
        {
            var selector = KeySelector(Pattern);
            Wildcard<TableName> match = new Wildcard<TableName>(selector)
            {
                Pattern = Pattern,
                Includes = Includedtables,
                Excludes = Excludedtables,
            };

            return match.Results(names);
        }

        public static Wildcard<TableName> CreateWildcard(ApplicationCommand cmd)
        {
            var selector = KeySelector(cmd.wildcard);
            Wildcard<TableName> match = new Wildcard<TableName>(selector)
            {
                Pattern = cmd.wildcard,
                Includes = cmd.Includes,
                Excludes = cmd.Excludes,
            };

            return match;
        }

        private static Func<TableName, string> KeySelector(string pattern)
        {
            if (pattern != null && pattern.IndexOf(".") > 0)
                return x => x.Path;
            else
                return x => x.ShortName;
        }

    }
}

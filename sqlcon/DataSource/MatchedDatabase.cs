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
        private string namePattern;

        public readonly DatabaseName DatabaseName;
        public string[] Includedtables { get; set; }
        public string[] Excludedtables { get; set; }

        public MatchedDatabase(DatabaseName databaseName, ApplicationCommand cmd)
            : this(databaseName, cmd.wildcard)
        {
            this.Includedtables = cmd.Includes;
            this.Excludedtables = cmd.Excludes;
        }

        public MatchedDatabase(DatabaseName databaseName, string namePattern)
        {
            this.namePattern = namePattern;
            this.DatabaseName = databaseName;
        }

        public TableName[] TableNames()
        {
            return TableNames(x => x.Path);
        }

        public TableName[] TableNames(Func<TableName, string> selector)
        {
            TableName[] names = this.DatabaseName.GetDependencyTableNames();
            Wildcard<TableName> match = new Wildcard<TableName>(selector)
            {
                Pattern = namePattern,
                Includes = Includedtables,
                Excludes = Excludedtables,
            };

            return match.Results(names);
        }

        public TableName[] ViewNames()
        {
            return ViewNames(x => x.Path);
        }

        public TableName[] ViewNames(Func<TableName, string> selector)
        {
            TableName[] names = this.DatabaseName.GetViewNames();
            Wildcard<TableName> match = new Wildcard<TableName>(selector)
            {
                Pattern = namePattern,
                Includes = Includedtables,
                Excludes = Excludedtables,
            };

            return match.Results(names);
        }

    }
}

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


        public TableName[] MatchedTableNames
        {
            get
            {
                TableName[] names = this.DatabaseName.GetDependencyTableNames();
                MatchedTable match = new MatchedTable(names)
                {
                    Pattern = namePattern,
                    IncludedTables = Includedtables,
                    ExcludedTables = Excludedtables,
                };

                return match.MatchedTables();
            }
        }

        public TableName[] DefaultViewNames
        {
            get
            {
                TableName[] names = this.DatabaseName.GetViewNames();
                MatchedTable match = new MatchedTable(names)
                {
                    Pattern = namePattern,
                    IncludedTables = Includedtables,
                    ExcludedTables = Excludedtables,
                };

                return match.MatchedTables();
            }
        }

    }
}

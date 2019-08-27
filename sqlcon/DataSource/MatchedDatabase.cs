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
            this.Pattern = namePattern;
            this.DatabaseName = databaseName;
        }

        public TableName[] TableNames()
        {
            if (Pattern != null && Pattern.IndexOf(".") > 0)
                return TableNames(x => x.Path);
            else
                return TableNames(x => x.ShortName);
        }

        public TableName[] TableNames(Func<TableName, string> selector)
        {
            TableName[] names = this.DatabaseName.GetDependencyTableNames();
            return TableName(names, selector);
        }

        public TableName[] ViewNames()
        {
            if (Pattern != null && Pattern.IndexOf(".") > 0)
                return ViewNames(x => x.Path);
            else
                return ViewNames(x => x.ShortName);
        }

        public TableName[] ViewNames(Func<TableName, string> selector)
        {
            TableName[] names = this.DatabaseName.GetViewNames();
            return TableName(names, selector);
        }

        public TableName[] TableName(TableName[] names, Func<TableName, string> selector)
        {
            Wildcard<TableName> match = new Wildcard<TableName>(selector)
            {
                Pattern = Pattern,
                Includes = Includedtables,
                Excludes = Excludedtables,
            };

            return match.Results(names);
        }
    }
}

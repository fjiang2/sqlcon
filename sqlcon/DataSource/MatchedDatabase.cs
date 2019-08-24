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
        public readonly string[] includedtables;
        public readonly string[] excludedtables;

        public MatchedDatabase(DatabaseName databaseName, ApplicationCommand cmd)
            : this(databaseName, cmd.wildcard, cmd.Includes)
        {
            this.excludedtables = cmd.Excludes;
        }

        public MatchedDatabase(DatabaseName databaseName, string namePattern, string[] includedtables)
        {
            this.namePattern = namePattern;
            this.DatabaseName = databaseName;

            if (includedtables != null)
                this.includedtables = includedtables;
        }




        public TableName[] MatchedTableNames
        {
            get
            {
                TableName[] names = this.DatabaseName.GetDependencyTableNames();

                names = names.Where(name => Includes(name)).ToArray();
                if (namePattern == null)
                    return names;

                names = Search(namePattern, names);

                return names;
            }
        }

        public TableName[] DefaultViewNames
        {
            get
            {
                TableName[] names = this.DatabaseName.GetViewNames();

                names = names.Where(name => Includes(name)).ToArray();
                if (namePattern == null)
                    return names;

                names = Search(namePattern, names);

                return names;
            }
        }

        public bool Includes(TableName tableName)
        {
            return Includes(includedtables, tableName);
        }

        public static bool Includes(string[] includedtables, TableName tableName)
        {
            if (includedtables == null || includedtables.Length == 0)
                return true;

            return includedtables.IsMatch(tableName.ShortName);
        }

        public static TableName[] Search(string pattern, TableName[] tableNames)
        {
            Regex regex = pattern.WildcardRegex();
            var result = tableNames.Where(tname => regex.IsMatch(tname.Name)).ToArray();

            return result;
        }

    }
}

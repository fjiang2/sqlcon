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
        public readonly string[] Excludedtables;

        public MatchedDatabase(DatabaseName databaseName, string namePattern, string[] excludedtables)
        {
            this.namePattern = namePattern;
            this.DatabaseName = databaseName;
            
            if(excludedtables != null)
                this.Excludedtables = excludedtables;
        }


        public TableName[] MatchedTableNames
        {
            get
            {
                if (namePattern == null)
                    return DatabaseName.GetTableNames();

                var names = Search(namePattern, this.DatabaseName.GetDependencyTableNames());
                return names;
            }
        }

        public TableName[] DefaultTableNames
        {
            get
            {
                TableName[] names = this.DatabaseName.GetTableNames();

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
            if (Excludedtables == null)
                return true;

            return !Excludedtables.IsMatch(tableName.ShortName);
        }

        public static TableName[] Search(string pattern, TableName[] tableNames)
        {
            Regex regex = pattern.WildcardRegex();
            var result = tableNames.Where(tname => regex.IsMatch(tname.Name)).ToArray();

            return result;
        }


   
    }
}

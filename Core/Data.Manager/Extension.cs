using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Sys.Data.Manager
{
    public static class Extension
    {

        public static TableSchema GetSchema(this TableName tname)
        {
            var schema = new TableSchema(tname);
            return schema;
        }


        public static string ToFieldName(this IColumn column)
        {
            string columnName = column.ColumnName;

            string fieldName = columnName;
            if (columnName.IndexOf("#") != -1
                || columnName.IndexOf(" ") != -1
                || columnName.IndexOf("/") != -1
                || !Char.IsLetter(columnName[0]))
            {
                fieldName = columnName.Replace("#", "_").Replace(" ", "_").Replace("/", "_");

                if (!Char.IsLetter(columnName[0]))
                    fieldName = "_" + fieldName;
            }

            return fieldName;

        }



        public static string ToClassName(this TableName tname, Func<string, string> rule)
        {
            string tableName = tname.Name;
            string className = ident.Identifier(tableName);

            //remove plural
            if (className.EndsWith("ees"))
                className = className.Substring(0, className.Length - 1);
            else if (className.EndsWith("ies"))
                className = className.Substring(0, className.Length - 3) + "y";
            else if (className.EndsWith("es"))
            {
                char ch1 = className[className.Length - 3];
                char ch2 = className[className.Length - 4];

                if (!IsVowel(ch1) && IsVowel(ch2))
                    className = className.Substring(0, className.Length - 1);
                else
                    className = className.Substring(0, className.Length - 2);
            }
            else if (className.EndsWith("s"))
            {
                char vowel = className[className.Length - 2];
                if (vowel != 'u')
                    className = className.Substring(0, className.Length - 1);
            }

            if (rule != null)
                className = rule(className);

            return className;

        }

        private static bool IsVowel(char ch)
        {
            return ch == 'a' || ch == 'e' || ch == 'i' || ch == 'o' || ch == 'u' || ch == 'y';
        }
    }
}

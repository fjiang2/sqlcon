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
            className = Singularize(className);

            if (rule != null)
                className = rule(className);

            return className;

        }

        private static string Singularize(string word)
        {
            if (word.EndsWith("ees"))
                word = word.Substring(0, word.Length - 1);
            else if (word.EndsWith("ies"))
                word = word.Substring(0, word.Length - 3) + "y";
            else if (word.EndsWith("es"))
            {
                char ch1 = word[word.Length - 3];
                char ch2 = word[word.Length - 4];

                if (!IsVowel(ch1) && IsVowel(ch2))
                    word = word.Substring(0, word.Length - 1);
                else
                    word = word.Substring(0, word.Length - 2);
            }
            else if (word.EndsWith("s"))
            {
                char vowel = word[word.Length - 2];
                if (vowel != 'u')
                    word = word.Substring(0, word.Length - 1);
            }

            return word;
        }

        private static bool IsVowel(char ch)
        {
            return ch == 'a' || ch == 'e' || ch == 'i' || ch == 'o' || ch == 'u' || ch == 'y';
        }
    }
}

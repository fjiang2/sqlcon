//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        DPO(Data Persistent Object)                                                               //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sys.Data;
using System.Globalization;

namespace Sys.Data.Manager
{
    public sealed class ClassTableName : TableName
    {

        public ClassTableName(DatabaseName databaseName, string tableName)
            : this( new TableName(databaseName, TableName.dbo, tableName))
        {
        }

        public ClassTableName(TableName tname)
            : base(new DatabaseName(tname.Provider, tname.DatabaseName.Name), tname.SchemaName, tname.Name)
        {
            Level = Level.Fixed;
            Pack = true;
            HasProvider = true;
        }


        public Level Level { get; set;}
        public bool Pack { get; set;}
        public bool HasProvider { get; set; }
      

        public string SubNamespace
        {
            get { return ident.Identifier(this.DatabaseName.Name); }
        }


        public string ClassName
        {
            get { return toClassName(this.tableName); }
        }


        private static string toClassName(string tableName)
        {
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
             
            //Add "Dpo"
            className += Setting.DPO_CLASS_SUFFIX_CLASS_NAME;

            return className;

        }

        private static bool IsVowel(char ch)
        {
            return ch == 'a' || ch == 'e' || ch == 'i' || ch == 'o' || ch == 'u' || ch == 'y';
        }
    }
}

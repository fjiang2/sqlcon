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

namespace Sys.Data
{
    /// <summary>
    /// Modeling SQL SELECT clause
    /// </summary>
    public class Selector
    {
        string[] columns;


        /// <summary>
        /// select all columns from table(SELECT * FROM table)
        /// </summary>
        public Selector()
        {
            columns = new string[0];
        }


        /// <summary>
        /// SELECT column1, column2,....
        /// </summary>
        /// <param name="columns"></param>
        public Selector(string[] columns)
        {
            this.columns = columns;
        }

        public Selector(IEnumerable<string> columns)
        {
            this.columns = columns.ToArray();
        }


        public bool Exists(string columnName)
        {
            if (columns.Length == 0)
                return true;

            foreach (string name in columns)
            {
                if (columnName == name)
                    return true;
            }

            return false;
        }


        public override string ToString()
        {
            if (columns.Length == 0)
                return "*";         //SELECT * FROM tableName

            return string.Join(",", columns.Select(column => string.Format("[{0}]", column))); ;
        }
    }
}

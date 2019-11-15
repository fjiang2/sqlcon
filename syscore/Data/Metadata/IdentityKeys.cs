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
using System.Data;

namespace Sys.Data
{
 

    public class IdentityKeys : IIdentityKeys
    {
        private string[] columnNames;

        public IdentityKeys()
        {
            this.columnNames = new string[0];
        }

        public IdentityKeys(string[] columns)
        {
            this.columnNames = columns;
        }

        internal IdentityKeys(ColumnCollection columns)
        {
            this.columnNames = columns.Where(column => column.IsIdentity).Select(column => column.ColumnName).ToArray();
        }

      
        public string[] ColumnNames
        {
            get
            {
                return this.columnNames;
            }
        }

        public int Length { get { return this.ColumnNames.Length; } }

        public override string ToString()
        {
            return string.Join(" , ", columnNames);
        }

    }
}

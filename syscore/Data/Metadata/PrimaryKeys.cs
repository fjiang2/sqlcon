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
   
    public class PrimaryKeys : IPrimaryKeys
    {
        private string[] keys;
        private string constraintName;

        public PrimaryKeys(string[] columns)
        {
            this.keys = columns;
            this.constraintName = null;
        }

        internal PrimaryKeys(ColumnCollection columns)
        {
            var pk = columns.Where(column => (column as ColumnSchema).PkContraintName != null);
            
            this.keys = pk.Select(column => column.ColumnName).ToArray();
            if (this.keys.Length != 0)
                this.constraintName = pk.Select(column => (column as ColumnSchema).PkContraintName).First();
        }

        public string[] Keys
        {
            get
            {
                return this.keys;
            }
        }

        public string ConstraintName
        {
            get { return this.constraintName; }
        }

        public int Length { get { return this.keys.Length; } }

        public override string ToString()
        {
            return string.Join(" + ", keys);
        }

    }
}

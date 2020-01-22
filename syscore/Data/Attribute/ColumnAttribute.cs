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
using System.Text;
using System.Data;
using Sys.Data;

namespace Sys.Data
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        private string columnName;
        private CType ctype;

        public string ColumnNameSaved;
        public object DefaultValue = null;
        
        public bool Primary = false;
        public bool Identity = false;
        public bool Computed = false;
        public bool Saved = true;

        public short Length = 0;
        public bool Nullable = false;
        public byte Precision = 0;
        public byte Scale = 0;


        public string Caption;

        public ColumnAttribute()
            : this(String.Empty, CType.Auto)
        {
        }

        public ColumnAttribute(string columnName)
            : this(columnName, CType.Auto)
        {
        }

        public ColumnAttribute(string columnName, CType ctype)
        {
            this.columnName = columnName;
            this.Caption = columnName;
            this.ColumnNameSaved = columnName;
            this.ctype = ctype;

            switch (ctype)
            {
                case CType.Char:
                case CType.VarChar:
                case CType.NChar:
                case CType.NVarChar:
                    Length = -1;
                    break;

                default:
                    Length = 0;
                    break;
            }
            
        }

     
        public string ColumnName
        {
            get { return columnName; }
            set { this.columnName = value; }
        }

        public CType CType
        {
            get { return ctype;  }
            set { this.ctype = value; }
        }


        public Type Type
        {
            get { return ctype.ToType(); }
        }

        public override string ToString()
        {
            return string.Format("{0}(Type={1}, Null={2}, Length={3})",columnName, ctype, Nullable, Length);
        }
    }
}

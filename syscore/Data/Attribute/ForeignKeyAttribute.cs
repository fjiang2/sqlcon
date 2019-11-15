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

namespace Sys.Data
{
    [System.AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConstraintForeignKeyAttribute : Attribute
    {
        public readonly Type PK_Table;
        public readonly string PK_Column;
        public readonly string FK_Column;

        /// <summary>
        /// CONSTRAINT FOREIGN KEY (pkColumn) REFERENCES pkTable(pkColumn)
        /// </summary>
        /// <param name="fkColumn"></param>
        /// <param name="pkTable"></param>
        /// <param name="pkColumn"></param>
        public ConstraintForeignKeyAttribute(string fkColumn, Type pkTable, string pkColumn)
        {
            this.FK_Column = fkColumn;
            this.PK_Table = pkTable;
            this.PK_Column = pkColumn;
        }

        public override string ToString()
        {
            return string.Format("CONSTRAINT FOREIGN KEY ({0}) REFERENCES {1}({2})", this.FK_Column, this.PK_Table.TableName(), this.PK_Column);
        }
    }


    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        /// <summary>
        /// PK Dpo class type for this table
        /// </summary>
        public readonly Type PK_Table;

        /// <summary>
        /// PK column name
        /// </summary>
        public readonly string PK_Column;

        /// <summary>
        /// FOREIGN KEY (this) REFERENCES pkTable(pkColumn)
        /// </summary>
        /// <param name="pkTableType"></param>
        /// <param name="pkColumnName"></param>
        public ForeignKeyAttribute(Type pkTableType, string pkColumnName)
        {
            this.PK_Table = pkTableType;
            this.PK_Column = pkColumnName;
        }

        /// <summary>
        /// SQL reference description
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("REFERENCES {0}({1})", this.PK_Table.TableName(), this.PK_Column);
        }
    }
}

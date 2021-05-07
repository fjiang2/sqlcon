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

namespace Sys.Data
{
    public class SqlTableName
    {
        private string tableName;

        public SqlTableName(TableName tableName)
        {
            this.tableName = tableName.FullName;
        }

        public SqlTableName(string tableName)
        {
            this.tableName = tableName;
        }

        public static implicit operator SqlTableName(string tableName)
        {
            return new SqlTableName(tableName);
        }

        public static implicit operator SqlTableName(TableName tableName)
        {
            return new SqlTableName(tableName);
        }

        public static implicit operator SqlTableName(DPObject dpo)
        {
            return new SqlTableName(dpo.TableName);
        }

        public static implicit operator SqlTableName(Type dpoType)
        {
            return new SqlTableName(dpoType.TableName());
        }

        public override string ToString()
        {
            return tableName;
        }

    }
}

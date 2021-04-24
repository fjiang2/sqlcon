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
    sealed class ClassTableName : TableName
    {

        public ClassTableName(DatabaseName databaseName, string tableName)
            : this(new TableName(databaseName, dbo, tableName))
        {
        }

        public ClassTableName(TableName tname)
            : base(new DatabaseName(tname.Provider, tname.DatabaseName.Name), tname.SchemaName, tname.Name)
        {
        }

        public DpoOption Option { get; set; } = new DpoOption();

        public string SubNamespace
        {
            get { return ident.Identifier(this.DatabaseName.Name); }
        }


        public string ClassName
        {
            get { return this.ToClassName(Option.ClassNameRule); }
        }


    

     
    }
}

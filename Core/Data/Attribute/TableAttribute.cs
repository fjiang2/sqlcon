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
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        private string tableName;
        private Level level;
        public int Provider = (int)ConnectionProvider.DEFAULT_HANDLE;

        public bool DefaultValueUsed = false;
        public bool Pack = true;
       

        public TableAttribute(string tableName, Level level)
        {
            this.tableName = tableName;
            this.level = level;
        }


        public Level Level
        {
            get { return this.level; }
        }

        public TableName TableName
        {
            get 
            {
                TableName tname;
                ConnectionProvider dataProvider = ConnectionProviderManager.Instance.GetProvider(this.Provider);
                switch (this.Level)
                {
                    case Level.System:
                        tname = new TableName(new DatabaseName(dataProvider, Const.DB_SYSTEM), "dbo", this.tableName);
                        break;

                    case Level.Application:
                        tname = new TableName(new DatabaseName(dataProvider, Const.DB_APPLICATION), "dbo", this.tableName);
                        break; 

                    default:
                        tname = new TableName(dataProvider, this.tableName);
                        break;
                }

                return tname;
            }
        }

    }
}

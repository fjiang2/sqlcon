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
using Sys.Data;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Sys.Data.Log;

namespace Sys.Data.Manager
{
    public static class ManagerExtension
    {

        #region Logger

        /// <summary>
        /// register user defined transaction logee
        /// </summary>
        /// <param name="transactionType"></param>
        /// <param name="logee"></param>
        public static void Register(this TransactionLogeeType transactionType, ITransactionLogee logee)
        {
            LogManager.Instance.Register(transactionType, logee);
        }

        /// <summary>
        /// register user defined record/row logee
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="logee"></param>
        public static void Register(this TableName tableName, IRowLogee logee)
        {
            LogManager.Instance.Register(tableName, logee);
        }


        #endregion



    }
}

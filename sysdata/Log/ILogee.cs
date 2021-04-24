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

namespace Sys.Data.Log
{
    public interface IRowLogee
    {
        /// <summary>
        /// Log a record/row
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="tableName"></param>
        /// <param name="tableId"></param>
        /// <param name="rowID"></param>
        /// <param name="state"></param>
        /// <param name="row1">row exists</param>
        /// <param name="row2">row is about to update/insert</param>
        /// <returns>return log row Id, return -1 if don't support Log column</returns>
        int LogRow(Transaction transaction, TableName tableName, int tableId, int rowID, ObjectState state, DataRow row1, DataRow row2);

        /// <summary>
        /// Log a column value
        /// </summary>
        /// <param name="log_row_id"></param>
        /// <param name="tableName"></param>
        /// <param name="tableId"></param>
        /// <param name="columnName"></param>
        /// <param name="v1">value from</param>
        /// <param name="v2">value to</param>
        /// <returns>log column id</returns>
        int LogColumn(int log_row_id, TableName tableName, int tableId, string columnName, object v1, object v2);
    }

    public interface ITransactionLogee
    {
    
        /// <summary>
        /// create transaction Id
        /// </summary>
        /// <param name="transactionType"></param>
        /// <param name="userID"></param>
        /// <returns>Transaction Id,return 0 if don't support log transaction</returns>
        Transaction LogTransaction(TransactionType type, int userID);

        /// <summary>
        /// Used to clear transaction record when nothing is logged during transaction 
        /// </summary>
        /// <param name="transaction"></param>
        void RemoveTransaction(Transaction transaction);
    }
}

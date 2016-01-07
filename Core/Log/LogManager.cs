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

namespace Sys.Data.Log
{
    class LogManager
    {
        static LogManager manager;

        Dictionary<TableName, IRowLogee> rowLogees = new Dictionary<TableName, IRowLogee>();
        Dictionary<TransactionLogeeType, ITransactionLogee> transactionLogees = new Dictionary<TransactionLogeeType, ITransactionLogee>();

        private LogManager()
        { 
        
        }

        public static LogManager Instance
        {
            get
            {
                if (manager == null)
                    manager = new LogManager();

                return manager;
            }
        }

        public IRowLogee RowLogee(TableName tableName)
        {
            if (rowLogees.ContainsKey(tableName))
                return rowLogees[tableName];
            else
                throw new NotImplementedException();
        }


        public IRowLogee RowLogee(DPObject dpo)
        {
           return RowLogee(dpo.TableName);
        }


        /// <summary>
        /// Register Logee Implement
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableId"></param>
        /// <param name="logee"></param>
        public void Register(TableName tableName, IRowLogee logee)
        {
            if (rowLogees.ContainsKey(tableName))
                rowLogees.Remove(tableName);

            rowLogees.Add(tableName, logee);
        }


        public ITransactionLogee TransactionLogee(TransactionLogeeType transactionType)
        {
            if (transactionLogees.ContainsKey(transactionType))
                return transactionLogees[transactionType];
            else
            {
#if DEBUG
                throw new Sys.MessageException("Logee type {0} is defined", transactionType);
#else
                //return new DefaultLogee();  //use default logee
                throw new Sys.MessageException("Logee type {0} is defined", transactionType);
#endif
            }
        }

        public ITransactionLogee TransactionLogee()
        {
            throw new NotImplementedException(); 
            //return new DefaultLogee();
        }


        public void Register(TransactionLogeeType transactionType, ITransactionLogee logee)
        {
            if (transactionLogees.ContainsKey(transactionType))
                transactionLogees.Remove(transactionType);

            transactionLogees.Add(transactionType, logee);
        }


     
    }
}

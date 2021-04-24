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

namespace Sys.Data.Log
{
    public class LogTransaction
    {
        public readonly Transaction transaction = new Transaction();

        private List<ILogable> logList = new List<ILogable>();
        private ITransactionLogee logee;

        public LogTransaction(Transaction transaction, ITransactionLogee logee)
        {
            this.transaction = transaction;
            this.logee = logee;
        }

        public void Add(ILogable log)
        {
            log.AddLog(this);
            this.logList.Add(log);
        }


        public void Remove(ILogable log)
        {
            log.RemoveLog();
            this.logList.Remove(log);
        }


        public void RemoveAll()
        {
            foreach (ILogable log in logList)
                log.RemoveLog();
            
            logList = new List<ILogable>();
        }


        public void EndTransaction()
        {
            bool logged = false;
            foreach (ILogable log in logList)
            {
                if (log.Logged())
                {
                    logged = true;
                    break;
                }
            }

            RemoveAll();

            if (!logged)
            {
                logee.RemoveTransaction(this.transaction);
            }
        }


        /// <summary>
        /// Use Default Transaction Logee
        /// </summary>
        /// <param name="formName"></param>
        /// <returns></returns>
        public static LogTransaction BeginTransaction(TransactionType formName)
        {
            ITransactionLogee logee = LogManager.Instance.TransactionLogee();

            Transaction transaction = logee.LogTransaction(formName, ActiveAccount.Account.UserID);
            return new LogTransaction(transaction, logee);
        }


        public static LogTransaction BeginTransaction(TransactionLogeeType typeName, TransactionType formName)
        {
            ITransactionLogee logee = LogManager.Instance.TransactionLogee(typeName);

            Transaction transaction = logee.LogTransaction(formName, ActiveAccount.Account.UserID);
            return new LogTransaction(transaction, logee);
        }

    }
}

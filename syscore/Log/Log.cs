using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Data.Log
{
    public class Log
    {
        private LogTransaction logTransaction = null;
        private TransactionType type;

        public Log(TransactionType type)
        {
            this.type = type;
        }

        /// <summary>
        /// Begin Log Transaction, all DPOs in Form are logged as a Transaction
        /// </summary>
        public void BeginLog()
        {
            this.logTransaction = LogTransaction.BeginTransaction(type);
        }

        public void BeginLog(TransactionLogeeType typeName)
        {
            this.logTransaction = LogTransaction.BeginTransaction(typeName, type);
        }

     

        /// <summary>
        /// Begin log Transaction, and register DPO into Logger
        /// </summary>
        /// <param name="logs"></param>
        public void BeginLog(params ILogable[] logs)
        {
            BeginLog();
            AddLog(logs);
        }

        public void BeginLog(TransactionLogeeType typeName, params ILogable[] logs)
        {
            BeginLog(typeName);
            AddLog(logs);
        }

        public void AddLog(params ILogable[] logs)
        {
            foreach (ILogable log in logs)
                this.logTransaction.Add(log);
        }

        public void RemoveLog(params ILogable[] logs)
        {
            foreach (ILogable log in logs)
                this.logTransaction.Remove(log);
        }

        /// <summary>
        /// Close log transaction, it can be closed many times
        /// </summary>
        public void EndLog()
        {
            if (this.logTransaction == null)
                return;

            this.logTransaction.EndTransaction();
            this.logTransaction = null;
        }


        public LogTransaction Transaction
        {
            get { return this.logTransaction; }
        }

        public override string ToString()
        {
            return string.Format("Log: {0}", type.ToString());
        }
    }
}

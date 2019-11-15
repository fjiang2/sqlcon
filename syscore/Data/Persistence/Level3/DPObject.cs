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
using System.Data;
using System.ComponentModel;
using Sys.Data.Log;

namespace Sys.Data
{
    /// <summary>
    /// represent a abstract record of table, which supports log, record locking, editable
    /// </summary>
    public abstract class DPObject : PersistentObject, ILogable, ILockable, IEditableObject
    {
        private Logger logger = null;

        /// <summary>
        /// default constructor, empty record 
        /// </summary>
        public DPObject()
        {
        }

        /// <summary>
        /// instantiate record by data row
        /// </summary>
        /// <param name="dataRow"></param>
        public DPObject(DataRow dataRow)
            : base(dataRow)
        { 
        
        }

        /// <summary>
        /// row id used in log, and record locking
        /// </summary>
        protected virtual int DPObjectId  
        {
            get
            {
                return -1;
            }
        }


        /// <summary>
        /// return row id
        /// </summary>
        public int RowId
        { 
            get 
            {
                return this.DPObjectId; 
            } 
        }

        //public abstract int TableId { get; }
        //public abstract string CreateTableString { get; }

        
        /// <summary>
        /// make this dpo record loggable
        /// </summary>
        /// <param name="transaction"></param>
        public void AddLog(LogTransaction transaction)
        {
            AddLog(transaction.transaction);
        }


        /// <summary>
        /// log transaction
        /// </summary>
        /// <param name="transaction"></param>
        protected void AddLog(Transaction transaction)
        {
            if (DPObjectId == -1)
                throw new ApplicationException(string.Format("must override DPObjectId{get} before log at {0}.", this.GetType().FullName));

            this.logger = new Logger(transaction, this);
        }


        /// <summary>
        /// remove log
        /// </summary>
        public void RemoveLog()
        {
            this.logger = null;
        }

        /// <summary>
        /// check logged or not
        /// </summary>
        /// <returns></returns>
        public bool Logged()
        {
            if (this.logger != null)
                return this.logger.Logged;
            else
                return false;
        }


        /// <summary>
        /// save current record and log
        /// </summary>
        /// <param name="logTransaction"></param>
        public virtual void SaveAndLog(LogTransaction logTransaction)
        {
            logTransaction.Add(this);
            this.Save();
            //logTransaction.Remove(this);
        }

        /// <summary>
        /// fire RowChanged event when row changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void RowChanged(object sender, RowChangedEventArgs e)
        {
            if (this.logger == null) return;

            logger.RowChanged(sender, e);
        }

        /// <summary>
        /// fire ValueChanged event when any values changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (this.logger == null) return;

            logger.ValueChanged(sender, e);
        }

        /// <summary>
        /// return record locking type
        /// </summary>
        public int LockType
        {
            get
            {
                return this.TableId;
            }
        }

        /// <summary>
        /// return lock id
        /// </summary>
        public int LockID
        {
            get
            {
                return DPObjectId;
            }
        }

        /// <summary>
        /// return description of this record, table name and row id
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("TableName={0} RowID={1}", this.TableName, this.DPObjectId);
        }

        
        public string SQL_CREATE_TABLE_STRING
        {
            get
            {
                return CreateTableString;
            }
        }


        #region IEditableObject

        //private void OnListChanged()
        //{
        //    collection.OnItemChanged(this);
        //}


        bool committed = false;

        /// <summary>
        /// begin to edit record
        /// </summary>
        public void BeginEdit()
        {
        }


        /// <summary>
        /// cancel editing 
        /// </summary>
        public void CancelEdit()
        {
            if (!committed)
            {
                collection.Remove(this);
            }
        }

        /// <summary>
        /// end of edit record
        /// </summary>
        public void EndEdit()
        {
            committed = true;
            this.Save();
        }

        #endregion

        public static SqlExpr AllColumnNames(string alias = null)
        {
            return SqlExpr.AllColumnNames(alias);
        }
    
    }
}

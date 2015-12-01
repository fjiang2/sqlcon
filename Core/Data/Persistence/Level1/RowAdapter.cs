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
using System.Data;
using System.Linq;

namespace Sys.Data
{
    public class RowAdapter : BaseRowAdapter
    {
        #region DataRow Changed Handler
        public event RowChangedHandler RowChanged;


        private bool OnRowChanged(ObjectState state, bool saved)
        {
            if (RowChanged != null)
            {
                RowChangedEventArgs args = new RowChangedEventArgs(this, state, saved);
                RowChanged(this, args);
                return args.confirmed;
            }

            return true;
        }

        #endregion

        
        protected DataRow dataRow;

       

        public RowAdapter(TableName tname, Locator locator, DataRow dataRow)
            :base(tname, locator)
        {
            this.dataRow = dataRow;
            //UpdateColumnValue(dataRow);       //columns.Count == 0 in this moment
        }

  
        //used for PersistentTable
        internal RowAdapter(DataFieldCollection fields, ColumnAdapterCollection columns, TableName tname, Locator locator)
            : base(tname, locator)
        {
            this.columns = columns;
            this.fields = fields;
        }

        public void UpdateColumnValue(DataRow dataRow)
        {
            this.dataRow = dataRow;
            columns.UpdateColumnValue(dataRow);
        }


        public DataRow CopyFrom(DataRow src)
        {
            foreach (DataColumn c in dataRow.Table.Columns)
            {
                dataRow[c] = src[c.ColumnName];
            }

            return src;
        }

        public DataRow CopyTo(DataRow dst)
        {
            foreach (DataColumn c in dataRow.Table.Columns)
            {
                dst[c.ColumnName] = dataRow[c];
            }

            return dst;
        }

   
      



        public DataRow Row
        {
            get { return dataRow; }
        }

     

        #region Communicate with WinControls
        protected virtual void Collect()
        {
            foreach (ColumnAdapter column in columns)
            {
                column.Collect();
                column.UpdateDataRow(dataRow);
            }
        }

        public virtual void Fill()
        {
            foreach (ColumnAdapter column in columns)
            {
                column.UpdateValue(this.dataRow);
                column.Fill();
            }
        }

        public virtual void Clear()
        {
            if (dataRow.RowState == DataRowState.Deleted)
                dataRow.RejectChanges();

            foreach (ColumnAdapter column in columns)
            {
                dataRow[column.Field.Name] = System.DBNull.Value;
            }
        }

        private void UpdateOriginValue(DataRow dataRow)
        {
            foreach (ColumnAdapter column in columns)
            {
                column.UpdateOriginValue(dataRow);
            }
        }

      

        #endregion




        #region Object Persistant Procedure


  

        public virtual bool Load()
        {
            this.dataRow = base.LoadRecord();
            return base.Exists;
        }





        public bool Insert()
        {
            if (!OnRowChanged(ObjectState.Added, false))
                return false;

            string SQL = insertQuery();

#if DEBUG
            Validate();
#endif

            SqlCmd sqlCmd = new SqlCmd(this.TableName.Provider, SQL);
            foreach (ColumnAdapter column  in columns)
            {
                if (column.Field.Identity)
                {
                    if (!this.InsertIdentityOn)
                        column.AddIdentityParameter(sqlCmd);
                    else
                        column.AddParameter(sqlCmd);
                }
                else if (column.Field.Saved || column.Field.Primary)
                {
                    column.AddParameter(sqlCmd);

                    if (column.IsValueChanged)
                        column.OnVauleChanged();
                }
            }

            if (this.transaction != null)
                transaction.Add(sqlCmd);

            sqlCmd.ExecuteNonQuery();

            //Identity Columns
            if (!this.InsertIdentityOn)
            {
                bool hasIdentity = false;
                foreach (ColumnAdapter column in columns)
                {
                    if (column.Field.Identity)
                    {
                        hasIdentity = true;
                        dataRow[column.Field.Name] = sqlCmd.GetReturnValue(column.Field.ParameterName);
                        column.UpdateValue(dataRow);
                    }
                }
                if (hasIdentity)
                    OnRowChanged(ObjectState.Added, true);
            }
           
            return true;
        }

        public bool Update()
        {
            RefreshRow();
       
            UpdateOriginValue(this.Row1);

            if (this.Row1.EqualTo(dataRow))
                return false; //Nothing is changed

            if (!OnRowChanged(ObjectState.Modified, false))
                return false;

            string SQL = updateQuery();
#if DEBUG
            Validate();
#endif

            SqlCmd sqlCmd = new SqlCmd(this.TableName.Provider, SQL);
            foreach (ColumnAdapter column in columns)
            {
                if (column.Field.Saved || column.Field.Identity || column.Field.Primary)
                {
                    column.AddParameter(sqlCmd);

                    if (column.IsValueChanged)
                        column.OnVauleChanged();
                }
            }

            if (this.transaction != null)
                transaction.Add(sqlCmd);

            sqlCmd.ExecuteNonQuery();

            //Identity Columns
            foreach (ColumnAdapter column in columns)
            {
                if (column.Field.Identity)
                {
                    dataRow[column.Field.Name] = this.Row1[column.Field.Name];
                    column.UpdateValue(dataRow);
                }
            }

            return true;
        }


        public virtual bool Save()
        {
            if (Exists)
                return Update();
            else
                return Insert();
        }
        

    
        public virtual bool Delete()
        {
            DataRow r = this.Row1;

            if (!Exists)
                return false;
            else
                UpdateOriginValue(r);

            if (!OnRowChanged(ObjectState.Deleted, false))
                return false;


            SqlCmd sqlCmd = new SqlCmd(this.TableName.Provider, deleteQuery());
            foreach (ColumnAdapter column in columns)
            {
                column.AddParameter(sqlCmd);
            }

            if (this.transaction != null)
                transaction.Add(sqlCmd);

            sqlCmd.ExecuteNonQuery();


            Clear();
            dataRow.Delete();


            return true;
        }



        #endregion




        protected ColumnAdapter Bind(ColumnAdapter column)
        {
            columns.Bind(column);

            column.UpdateValue(this.dataRow);
            return column;
        }

  
    }
}

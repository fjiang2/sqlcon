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
using System.Collections;
using System.Text;
using System.Reflection;
using System.Data;

namespace Sys.Data
{
    public enum ObjectPermission
    {
        AllowUpdateObject,
        DenyUpdateObject
    }
   
    public abstract class PersistentCollection<T> : IDPCollection, IEnumerable<T>, IEnumerable 
        where T : class,  IDPObject, new()
    {

        public event PersistentHandler ObjectChanged;
    
        protected DataTable dataTable;
        protected ObjectPermission objectPermission = ObjectPermission.AllowUpdateObject;
    
        private object sender;      //who changed datatable?
        private Dictionary<IDPObject, DataRow> mapping;

        public PersistentCollection(DataTable dataTable)
        {
            this.Table = dataTable;
        }

        public PersistentCollection()
        {
            DataTable dataTable;
            if (TableName.Exists())
            {
                dataTable = DataExtension.FillDataTable("SELECT TOP 1 * FROM {0}", TableName);
                dataTable.TableName = TableName.Name;
                dataTable.Clear();
            }
            else
                dataTable = Reflex.GetEmptyDataTable<T>();

            this.Table = dataTable;
        }

        public override string ToString()
        {
            return string.Format("{0} #{1}",typeof(T).FullName, dataTable.Rows.Count);
        }


        public void BeginRowChanged()
        {
            this.objectPermission = ObjectPermission.DenyUpdateObject;
        }

        public void EndRowChanged()
        {
            this.objectPermission = ObjectPermission.AllowUpdateObject;
        }

        #region Add/InsertAt/InsertAfter/Remove/Swap

        public bool Add(T t)
        {
            objectPermission = ObjectPermission.DenyUpdateObject;
            t.SetCollection(this);

            DataRow newRow = dataTable.NewRow();
            t.Collect(newRow);
            dataTable.Rows.Add(newRow);
            mapping.Add(t, newRow);

            OnEvent(sender, t, newRow);
            objectPermission = ObjectPermission.AllowUpdateObject;
            return true;
        }
        
        public bool InsertAt(T t, int pos)
        {
            objectPermission = ObjectPermission.DenyUpdateObject;
            t.SetCollection(this);

            DataRow newRow = dataTable.NewRow();
            t.Collect(newRow);
            dataTable.Rows.InsertAt(newRow, pos);
            mapping.Add(t, newRow);

            OnEvent(sender, t, newRow);
            objectPermission = ObjectPermission.AllowUpdateObject;
            return true;
        }
        
        public bool InsertAfter(T t1, T t2)
        {
            if (t2 == null)
                return Add(t1);

            objectPermission = ObjectPermission.DenyUpdateObject;
            t1.SetCollection(this);

            DataRow dataRow2 = mapping[t2];
            int pos = dataTable.Rows.IndexOf(dataRow2);
            InsertAt(t1, pos+1);

            objectPermission = ObjectPermission.AllowUpdateObject;
            return true;
        }

 
        public bool Remove(T t)
        {
            return remove(t, true);
        }
        
        private bool remove(T t, bool fired)
        {
            if (mapping.ContainsKey(t))
            {
                objectPermission = ObjectPermission.DenyUpdateObject;

                mapping[t].AcceptChanges();
                mapping[t].Delete();
                
                if(fired)
                    OnEvent(sender, t, mapping[t]);
                
                mapping.Remove(t);

                objectPermission = ObjectPermission.AllowUpdateObject;

                return true;
            }

            return false;
        }

        public void Swap(T t1, T t2)
        {
            objectPermission = ObjectPermission.DenyUpdateObject;

            DataRow dataRow1 = mapping[t1];
            DataRow dataRow2 = mapping[t2];
         
            t1.Collect(dataRow2);
            t2.Collect(dataRow1);

            mapping[t1] = dataRow2;
            mapping[t2] = dataRow1;

            OnEvent(sender, t1, dataRow2 ,t2, dataRow1);
            objectPermission = ObjectPermission.AllowUpdateObject;
        }
        
        #endregion


        public void UpdateDataRow(T t)
        {
            objectPermission = ObjectPermission.DenyUpdateObject;

            DataRow dataRow = mapping[t];
            t.Collect(dataRow);

            OnEvent(sender, t, dataRow);
            objectPermission = ObjectPermission.AllowUpdateObject;
        }

        /// <summary>
        ///  Update this.DataTable by PersistentObject
        /// </summary>
        public void AcceptChanges()
        {
            foreach (IDPObject t in mapping.Keys)
                UpdateDataRow(t);
        }

        public IPersistentObject GetObject(int index) { return this[index]; }
        public IPersistentObject GetObject(DataRow dataRow) { return this[dataRow]; }
        public void Add(IPersistentObject p) { Add((T)p); }
        public bool InsertAfter(IPersistentObject p1, IPersistentObject p2) { return InsertAfter((T)p1, (T)p2);}
        public void Remove(IPersistentObject p) { Remove((T)p); }
        public void UpdateDataRow(IPersistentObject p) { UpdateDataRow((T)p); }
        public void Swap(IPersistentObject p1, IPersistentObject p2) { Swap((T)p1, (T)p2);}
        public IPersistentObject NewInstance()
        {
            T t = new T();
            t.SetCollection(this);
            return t; 
        }


        #region Properties

        public object Sender 
        { 
          set 
            { 
                this.sender = value; 
            } 
        }
        
        public DataTable Table
        {
            get
            {
                return this.dataTable;
            }
            set
            {
                this.dataTable = value;

                this.Table.RowChanged += RowChanged;
                this.Table.RowDeleted += RowChanged;

                this.mapping = new Dictionary<IDPObject, DataRow>();
            }

        }

      

        public TableName TableName
        {
            get
            {
                return new T().TableName;
            }
        }

        public int Count
        {
            get
            {
                return dataTable.Rows.Count;
            }
        }


        private IDPObject Search(DataRow dataRow)
        {
            foreach (KeyValuePair<IDPObject, DataRow> kvp in mapping)
            {
                if (kvp.Value == dataRow)
                    return kvp.Key;
            }
            return null;
        }



        public T this[int index]
        {
            get
            {
                DataRow dataRow = dataTable.Rows[index];
                return this[dataRow];
            }
            set
            {
                DataRow dataRow = dataTable.Rows[index];
                this[dataRow] = value;
            }
        }



        
        //On-demand Loading.....
        public T this[DataRow dataRow]
        {
            get 
            {
                if (dataRow.RowState == DataRowState.Deleted)
                    return null;

                IPersistentObject x = Search(dataRow);

                if (x == null)
                {
                    T t = new T();
                    t.SetCollection(this);
                    t.Fill(dataRow);
                    mapping.Add(t, dataRow);
                    return t;
                }
                else
                {
                    T t = (T)x;
                    t.Fill(dataRow);    //??? trick: update object by dataRow
                    return t;
                }
            }
            set
            {
                T t = value;
                t.Collect(dataRow);

                if (mapping.ContainsKey(t))
                    mapping.Remove(t);
                mapping.Add(t, dataRow);

            }
        }

  

        #endregion


      


        #region Saving


        private RowAdapter NewSqlRow(Selector columnNames)
        {
            T p = new T();
            p.SetCollection(this);
            return p.NewRowAdapter(columnNames);
        }

        public virtual bool Save()
        {
            return Save(new Selector(), null,null);
        }
        
        public virtual bool Save(string[] columnNames)
        {
            return Save(new Selector(columnNames), null, null);
        }

        public virtual bool Save(Selector columnNames, RowChangedHandler rowHandler, ValueChangedHandler columnHandler)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return false;

            //update this.dataTable
           // this.AcceptChanges();

            objectPermission = ObjectPermission.DenyUpdateObject;
            RowAdapter d = this.NewSqlRow(columnNames);

            d.RowChanged += RowChanged;

            if (rowHandler != null)
                d.RowChanged += rowHandler;

            if (columnHandler != null)
                d.ValueChangedHandler = columnHandler;
            else
                d.ValueChangedHandler = ValueChanged;

            int count = 0;
            foreach (DataRow dataRow in dataTable.Rows)
            {
                BeforeSave(dataRow);

                if (dataRow.RowState != DataRowState.Deleted) 
                {
                    if (dataRow.RowState != DataRowState.Unchanged)
                    {
                        d.CopyFrom(dataRow);
                        d.Fill();
                        d.Save();
                        if (dataRow.RowState == DataRowState.Added)   //in case of existing identity columns
                        {
                            d.CopyTo(dataRow);
                            UpdateObject(dataRow);
                        }

                        //slow version
                        //T t = this[dataRow];
                        //t.Save(d);
                        count++;
                    }
                }
                else
                {
                    dataRow.RejectChanges();
                    d.CopyFrom(dataRow);
                    d.Fill();
                    d.Delete();
                    
                    //slow version
                    //T t = this[dataRow];
                    //t.Delete();
                    dataRow.Delete();
                }

                AfterSave(dataRow);
            }

            dataTable.AcceptChanges();

            objectPermission = ObjectPermission.AllowUpdateObject;
            return true;
        }

        protected virtual void BeforeSave(DataRow dataRow)
        {

        }

        protected virtual void AfterSave(DataRow dataRow)
        {

        }


        protected virtual void RowChanged(object sender, RowChangedEventArgs e)
        {

        }

        protected virtual void ValueChanged(object sender, ValueChangedEventArgs e)
        {

        }

        #endregion


        #region Event Update Object

        public bool OnEvent(object sender, T t, DataRow dataRow)
        {
            if (ObjectChanged != null)
            {
                PersistentEventArgs args = new PersistentEventArgs(sender, t, dataRow);
                ObjectChanged(this, args);
                return true;
            }

            return false;
        }

        public bool OnEvent(object sender, T t1, DataRow dataRow1, T t2, DataRow dataRow2)
        {
            if (ObjectChanged != null)
            {
                PersistentEventArgs args = new PersistentEventArgs(sender, t1, dataRow1, t2, dataRow2);
                ObjectChanged(this, args);
                return true;
            }

            return false;
        }


        private void UpdateObject(DataRow dataRow)
        {
           // objectPermission = ObjectPermission.DenyUpdateObject;
            T t;
            switch (dataRow.RowState)
            {
                case DataRowState.Modified:
                case DataRowState.Added:
                    t = this[dataRow];
                    OnEvent(dataTable, t, dataRow);
                    break;

                case DataRowState.Deleted:
                    dataRow.RejectChanges();
                    t = this[dataRow];
                    this.remove(t,false);
                    OnEvent(dataTable, t, dataRow);  //event fired in function Remove() 
                    break;
            }

            //objectPermission = ObjectPermission.AllowUpdateObject;
        }

        private void RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action != DataRowAction.Change && e.Action != DataRowAction.Add && e.Action != DataRowAction.Delete)
                return;

            if (this.objectPermission != ObjectPermission.AllowUpdateObject)   
                return;

            this.UpdateObject(e.Row);
            return;
        }

        #endregion


        #region IEnumerable<T>, IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return new DpcEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DpcEnumerator(this);
        }

        private class DpcEnumerator : IDisposable, IEnumerator<T>, IEnumerator
            //where T : class, IDataPersistentObject, new()
        {
            PersistentCollection<T> dpc;
            int cursor = -1;

            public DpcEnumerator(PersistentCollection<T> dpc)
            {
                this.dpc = dpc;
            }

            public T Current
            {
                get
                {
                    if (cursor > dpc.Count - 1)
                        throw new InvalidOperationException("Enumeration already finished");

                    if (cursor == -1)
                        throw new InvalidOperationException("Enumeration not started");

                    return this.dpc[cursor];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                   return Current;
                }
            }

            public bool MoveNext()
            {
                cursor++;

                if (cursor > dpc.Count-1)
                    return false;
                else
                    return true;
            }


            public void Reset()
            {
                cursor = -1;
            }

            public void Dispose()
            {
                cursor = -1;
            }
        }


        
        #endregion
    }











   


    //public class DataPersistentCollectionSaveWork : Callback
    //{
    //    IDataPersistentCollection collection;
    //    string[] columnNames;

    //    public DataPersistentCollectionSaveWork(System.Windows.Forms.Control sender, IDataPersistentCollection collection, string[] columnNames)
    //        : base(sender)
    //    {
    //        this.collection = collection;
    //        this.columnNames = columnNames;
    //    }

    //    protected override object DoWorkFunction(object args)
    //    {
    //        try
    //        {
    //            object result = true;

    //            DataTable dataTable = collection.DataTable;

    //            JDataRow d = collection.Bind(columnNames);

    //            int i = 0;
    //            foreach (DataRow dataRow in dataTable.Rows)
    //            {
    //                if (dataRow.RowState != DataRowState.Deleted)
    //                {
    //                    if (dataRow.RowState != DataRowState.Unchanged)
    //                    {
    //                        d.CopyFrom(dataRow);
    //                        d.Fill();
    //                        d.Save();
    //                    }
    //                }
    //                else
    //                {
    //                    dataRow.RejectChanges();
    //                    d.CopyFrom(dataRow);
    //                    d.Fill();
    //                    d.Delete();
    //                    dataRow.Delete();
    //                }

    //                object middleResult = string.Format("{0}/{1}", i++, dataTable.Rows.Count);
    //                WorkDoingFunction(middleResult);
    //            }

    //            dataTable.AcceptChanges();
    //            return result;
    //        }
    //        catch (Exception e)
    //        {
    //            WorkCancelled = true;
    //            return e;
    //        }
    //    }




  
//    }

}

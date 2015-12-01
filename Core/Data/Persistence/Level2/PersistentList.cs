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
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.ComponentModel;

namespace Sys.Data
{
    public abstract class PersistentList<T> : BindingList<T>, IPersistentCollection, IEnumerable<T>, IEnumerable
        where T: class,  IDPObject, new()
    {
        protected PersistentList()
        {
            this.AllowNew = true;
            this.AllowRemove = true;
            this.AllowEdit = true;

            this.AddingNew += new AddingNewEventHandler(PersistentList_AddingNew);
        }

        private void PersistentList_AddingNew(object sender, AddingNewEventArgs e)
        {
            T t = new T();
            e.NewObject = t;
        }

        /// <summary>
        /// override Fill(DataRow) to initialize varibles in this class, if typeof(T).BaseType != typeof(DPObject)
        /// </summary>
        /// <param name="dataTable"></param>
        protected PersistentList(DataTable dataTable)
            :this()
        {
            this.dataTable = dataTable;

            foreach (DataRow dataRow in dataTable.Rows)
            {
                T t = new T();
                t.Fill(dataRow);
                this.Add(t);
            }
        }

        protected PersistentList(IEnumerable<T> records)
            :this()
        {
            foreach (T t in records)
            {
                this.Add(t);
            }
        }
      
        public F[] ToArray<F>(string fieldName)
        {
            FieldInfo fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public| BindingFlags.NonPublic);

            if (fieldInfo != null)
            {
                F[] values = new F[this.Count];
                int i = 0;
                foreach (T t in this)
                {
                    values[i++] = (F)fieldInfo.GetValue(t);
                }
                return values;
            }

            PropertyInfo propertyInfo = typeof(T).GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propertyInfo != null)
            {
                F[] values = new F[this.Count];
                int i = 0;
                foreach (T t in this)
                {
                    values[i++] = (F)propertyInfo.GetValue(t, null);
                }

                return values;
            }

            return null;
        }

        public void Save()
        {
            foreach (T t in this)
            {
                t.Save();
            }
        }


        private DataTable dataTable = null;
        public DataTable Table
        {
            get
            {
                if (dataTable == null)
                    dataTable = Reflex.GetEmptyDataTable<T>();
                else
                    dataTable.Rows.Clear();

                foreach (T t in this)
                {
                    DataRow newRow = dataTable.NewRow();
                    t.UpdateDataRow(newRow);
                    dataTable.Rows.Add(newRow);
                }

                return dataTable;
            }
            set
            {
                //cannot use it here
                throw new NotImplementedException();
            }
        }




        public void Add(IPersistentObject value)
        {
            this.Items.Add((T)value);
        }



        public void Remove(IPersistentObject value)
        {
            this.Items.Remove((T)value);
            
        }


        public void UpdateDataRow(IPersistentObject value)
        {
            throw new NotSupportedException(); 
        }


        public override string ToString()
        {
            return string.Format("{0}=#{1}", typeof(T).FullName, this.Count);
        }
    }
}

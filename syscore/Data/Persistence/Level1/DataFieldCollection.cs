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

namespace Sys.Data
{
    public class DataFieldCollection : List<DataField>
    {
        internal DataFieldCollection()
        {
        }

     
        public void UpdatePrimaryIdentity(TableName tableName)
        {
            TableSchema meta = tableName.GetTableSchema();

            UpdatePrimaryIdentity(meta.PrimaryKeys, meta.Identity);
        }

        
        public void UpdatePrimaryIdentity(IPrimaryKeys primary, IIdentityKeys identity)
        {
            
            foreach (string key in identity.ColumnNames)
            {
                DataField field = FindField(key); 
                if (field != null)
                    field.Identity = true;
            }

            foreach (string key in primary.Keys)
            {
                DataField field = FindField(key);
                if (field != null)
                    field.Primary = true;

            }
        }

        public int IndexOf(string fieldName)
        {
            DataField field = FindField(fieldName);
            return this.IndexOf(field);
        }

        public DataField FindField(string fieldName)
        {
            DataField field = this.Find(col => col.Name == fieldName);
            return field;
        }

        public bool ContainsField(string fieldName)
        {
            foreach (DataField field in this)
                if (field.Name.Equals(fieldName))
                    return true;

            return false;
        }

        private bool AddItem(DataField field)
        {
        
            if (this.ContainsField(field.Name))
                return false;;

            base.Add(field);
            
            return true;
        }

        /// <summary>
        /// Add all columns of table
        /// </summary>
        /// <param name="tname"></param>
        /// <returns></returns>
        public DataFieldCollection Add(TableName tname)
        {
            var meta = tname.GetTableSchema();      //some columns in [this.dataTable] may be from other tables
            Add(meta.NewRow());

            UpdatePrimaryIdentity(tname);

            return this;
        }


   
        public DataFieldCollection Add(DataTable dataTable)
        {
            foreach (DataColumn dataColumn in dataTable.Columns)
                Add(dataColumn);
            
            return this;
        }

        public DataFieldCollection Add(DataRow dataRow)
        {
            foreach (DataColumn dataColumn in dataRow.Table.Columns)
                Add(dataColumn);

            return this;
        }


        public DataField Add(DataColumn column)
        {
            DataField field = Add(column.ColumnName, column.DataType);
            field.Caption = column.Caption;

            return field;
        }

        public new DataField Add(DataField field)
        {
            if (this.ContainsField(field.Name))
                return this.FindField(field.Name);

            this.AddItem(field);

            return field;
        }
       
        public DataField Add(string columnName, Type dbType)
        {
            DataField field = new DataField(columnName, dbType);
            this.AddItem(field);

            return field;
        }

        
    }
}

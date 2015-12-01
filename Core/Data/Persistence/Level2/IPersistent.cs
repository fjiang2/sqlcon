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
using System.Data;

namespace Sys.Data
{
    #region Persistent Object

    public interface IPersistentObject
    {
        DataRow Load();
        DataRow Save();
        DataRow Save(string[] columnNames);
        
        bool Delete();
        void Clear();

        TableName TableName { get; }
        Locator Locator { get; }
    }


    public interface IDPObject : IPersistentObject
    {
        void UpdateDataRow(DataRow dataRow);
        void UpdateObject(DataRow dataRow);

        void Fill(DataRow dataRow);
        void Collect(DataRow dataRow);
        void FillIdentity(DataRow dataRow);

        RowAdapter NewRowAdapter(Selector columnNames);

        void SetCollection(IPersistentCollection collection);
    }

    #endregion



    #region Persistent Collection



    /// <summary>
    /// mainly used by class PersistentObject
    /// </summary>
    public interface IPersistentCollection : IEnumerable
    {
        DataTable Table { get; set; }
        void UpdateDataRow(IPersistentObject p);

        void Add(IPersistentObject p);
        void Remove(IPersistentObject p);
    }

 
    public interface IDPCollection :  IPersistentCollection
    {

        int Count { get; }
        bool Save();
        TableName TableName { get; }

        event PersistentHandler ObjectChanged;
        object Sender { set; }

        IPersistentObject GetObject(int index);
        IPersistentObject GetObject(DataRow dataRow);
        bool InsertAfter(IPersistentObject p1, IPersistentObject p2);
        void Swap(IPersistentObject p1, IPersistentObject p2);

        void AcceptChanges();
        IPersistentObject NewInstance();

    }


    #endregion



  
  
}

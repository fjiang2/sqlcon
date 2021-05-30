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

namespace Sys.Data
{
    public delegate void PersistentHandler(object sender, PersistentEventArgs e);
    
    public enum PersistentObjectState
    { 
        Added,
        Deleted,
        Modified,
        Swapped,
        Unchanged
    }

    public class PersistentEventArgs : EventArgs
    {
        public readonly PersistentObjectState ObjectState;
        public readonly object Sender;
        public readonly object Object1;
        public readonly DataRow Row1;
        public readonly object Object2;
        public readonly DataRow Row2;

        public PersistentEventArgs(object sender, object obj, DataRow dataRow)
        {
            switch (dataRow.RowState)
            { 
                case DataRowState.Added:
                    ObjectState = PersistentObjectState.Added;
                    break;
                
                case DataRowState.Deleted :
                    ObjectState = PersistentObjectState.Deleted;
                    break;

                case DataRowState.Modified:
                    ObjectState = PersistentObjectState.Modified ;
                    break;
                
                case DataRowState.Unchanged:
                    ObjectState = PersistentObjectState.Unchanged;
                    break;
            }

            this.Sender = sender;
            this.Object1 = obj;
            this.Row1 = dataRow;
            this.Object2 = null;
            this.Row2 = null;
        }

        public PersistentEventArgs(object sender, object obj1, DataRow row1, object obj2, DataRow row2)
        {
            ObjectState = PersistentObjectState.Swapped;
            this.Sender = sender;
            this.Object1 = obj1;
            this.Row1 = row1;
            this.Object2 = obj2;
            this.Row2 = row2;
        }
    }
}

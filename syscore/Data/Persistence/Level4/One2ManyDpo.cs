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
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Data;
//using System.ComponentModel;

//namespace Sys.Data
//{
//    public class OneManyObject<TMany> : DPObject 
//        where TMany: class, IDPObject, new()
//    {
//        public event AddingNewEventHandler AddingNew;

//        DPList<TMany> many;

//        public OneManyObject()
//        {
//            this.many = new DPList<TMany>();
//        }

//        public OneManyObject(DataRow dataRow)
//            : base(dataRow)
//        {
//        }

//        public override void Fill(DataRow dataRow)
//        {
//            base.Fill(dataRow);
            
//            if (AddingNew != null)
//            {
//                var args = new AddingNewEventArgs();
//                AddingNew(this, args);
//                this.many = (DPList<TMany>)args.NewObject;
//            }
//            else
//                this.many = new DPList<TMany>(new TableReader<TMany>(new ColumnValue("", this.DPObjectId)));
//        }

//        public override DataRow Save()
//        {
//            DataRow row = base.Save();
//            this.many.Save();

//            return row;
//        }
//    }
//}

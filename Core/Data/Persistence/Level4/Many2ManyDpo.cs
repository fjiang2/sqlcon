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
//    public class Many2ManyDpo<TMany, TMap> : DPObject 
//        where TMany : class, IDPObject, new()
//        where TMap : class, IDPObject, IMany2ManyMapping, new()
//    {
//        private DPList<TMany> many;
//        private DPList<TMap> maps;

//        public Many2ManyDpo()
//        { 
        
//        }

//        public override void Fill(DataRow dataRow)
//        {
//            base.Fill(dataRow);
//            TMap map = new TMap();

//            object id1 = dataRow[map.ColumnName1];
//            this.maps = new DPList<TMap>(new TableReader<TMap>(new ColumnValue(map.MapName1, id1)));

//            object[] Id2 = this.maps.ToArray<object>(map.MapName2);
//            this.many = new DPList<TMany>(new TableReader<TMany>(new ColumnValue(map.ColumnName2, Id2)));
//        }

//        public override DataRow Save()
//        {
//            DataRow row = base.Save();
//            this.many.Save();

//            return row;
//        }

//        public DPList<TMany> Many
//        {
//            get { return this.many; }
//        }
//    }
//}

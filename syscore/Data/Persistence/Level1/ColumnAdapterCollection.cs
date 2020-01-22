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
    public class ColumnAdapterCollection : List<ColumnAdapter>
    {
        
        internal ColumnAdapterCollection()
        {
        }

        public void UpdateColumnValue(DataRow dataRow)
        {
            foreach (ColumnAdapter column in this)
            {
                column.UpdateValue(dataRow);
                column.Fill();
            }

        }

        public void UpdateDataRow(DataRow dataRow)
        {
            foreach (ColumnAdapter column in this)
            {
                column.Collect();
                column.UpdateDataRow(dataRow);
            }
        }

       

        public ColumnAdapter Bind(ColumnAdapter column)
        {
            foreach (ColumnAdapter adapter in this)
                if (adapter.Field.Name.Equals(column.Field.Name))
                    return adapter;

            base.Add(column);

            return column;
        }
       


     
    }
}

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
using Sys.Data;

namespace Sys.Data
{
    /// <summary>
    /// write records of data table into database, meta table is given by typeof(T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TableWriter<T> where T : class,  IDPObject, new()
    {
        private DataTable dataTable;

        /// <summary>
        /// write data table
        /// </summary>
        /// <param name="dataTable"></param>
        public TableWriter(DataTable dataTable)
        {
            this.dataTable = dataTable;
        }

        /// <summary>
        /// writer table reader
        /// </summary>
        /// <param name="reader"></param>
        public TableWriter(TableReader<T> reader)
            :this(reader.Table)
        {
        }

        /// <summary>
        /// write a collection of records
        /// </summary>
        /// <param name="collection"></param>
        public TableWriter(IEnumerable<T> collection)
            :this(collection.ToTable<T>())
        {
        }


        /// <summary>
        /// write a list of records
        /// </summary>
        /// <param name="list"></param>
        public TableWriter(DPList<T> list)
            :this(list.Table)
        {
        }


        private TableName TableName
        {
            get
            {
                return typeof(T).TableName();
            }
        }


        /// <summary>
        /// return data table
        /// </summary>
        public DataTable Table
        {
            get
            {
                return this.dataTable;
            }
        }


        /// <summary>
        /// save records into database
        /// </summary>
        public void Save()
        {
            T dpo = new T();
            TableAdapter.WriteDataTable(dataTable, dpo.TableName, dpo.Locator, null, null, null);
        }


        /// <summary>
        /// return description TableWriter
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("TableWriter<{0}> Count={1}", typeof(T).FullName, this.dataTable.Rows.Count);
        }
    }
}

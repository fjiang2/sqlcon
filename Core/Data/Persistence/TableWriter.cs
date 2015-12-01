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
    /// <summary>
    /// write records of data table into database
    /// </summary>
    public class TableWriter
    {
        private DataTable dataTable;
        private TableName tableName;
        private Locator locator;

        /// <summary>
        /// use default locator to save records into database, primary keys must be defined
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dataTable"></param>
        public TableWriter(TableName tableName, DataTable dataTable)
        {
            this.tableName = tableName;
            this.dataTable = dataTable;

            IPrimaryKeys primary = this.tableName.GetTableSchema().PrimaryKeys;
            if (primary.Length != 0)
                this.locator = new Locator(primary);
            else
                throw new MessageException("There is no locator defined.");
        }

        /// <summary>
        /// use user defined locator to save records into database
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="locator"></param>
        /// <param name="dataTable"></param>
        public TableWriter(TableName tableName, Locator locator, DataTable dataTable)
        {
            this.tableName = tableName;
            this.dataTable = dataTable;
            this.locator = locator;

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
            TableAdapter.WriteDataTable(dataTable, this.tableName, this.locator, null, null, null);
        }

        /// <summary>
        /// return TableWriter description
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("TableWriter<{0}> Count={1}", this.tableName.FullName, this.dataTable.Rows.Count);
        }
    }
}

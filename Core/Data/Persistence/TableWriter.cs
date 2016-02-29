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
        private Locator locator;
        private TableSchema schema;

        /// <summary>
        /// use default locator to save records into database, primary keys must be defined
        /// </summary>
        /// <param name="tableName"></param>
        public TableWriter(TableName tableName)
        {
            this.schema = tableName.GetTableSchema();

            IPrimaryKeys primary = schema.PrimaryKeys;
            if (primary.Length != 0)
                this.locator = new Locator(primary);

        }

        /// <summary>
        /// use user defined locator to save records into database
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="locator"></param>
        public TableWriter(TableName tableName, Locator locator)
        {
            this.schema = tableName.GetTableSchema();
            this.locator = locator;
        }

        public TableName TableName
        {
            get { return this.schema.TableName; }
        }


        TableScript tableScript = null;
        public void Insert(DataRow row)
        {
            if (tableScript == null)
                tableScript = new TableScript(schema);

            string sql = tableScript.INSERT(row);

            new SqlCmd(TableName.Provider, sql).ExecuteNonQuery();
        }

        /// <summary>
        /// save records into database
        /// </summary>
        public void Save(DataTable table)
        {
            TableAdapter.WriteDataTable(table, TableName, this.locator, null, null, null);
        }

        /// <summary>
        /// return TableWriter description
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("TableWriter<{0}>", TableName.FullName);
        }
    }
}


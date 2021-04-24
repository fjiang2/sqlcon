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
using System.Collections.Generic;
using System.Data;

namespace Sys.Data
{
    class TableSchemaManager
    {
        private Dictionary<TableName, DataTable> dict = new Dictionary<TableName, DataTable>();
        private TableSchemaManager()
        {
        }

        /// <summary>
        /// Create table schema by rows in data table. It is used to create a table by DataTable 
        /// </summary>
        /// <param name="dt"></param>
        private void AddTableSchema(TableName tname, DataTable dt)
        {
            DataSet ds = dt.DataSet;
            if (ds == null)
            {
                ds = new DataSet { DataSetName = tname.DatabaseName.Name, };
                ds.Tables.Add(dt);
            }

            DbSchemaBuilder dbb = new DbSchemaBuilder();
            dbb.AddSchema(ds);

            Add(tname, dbb.DbSchmea.Tables[0]);
        }

        private void Add(TableName tname, DataTable dtSchema)
        {
            if (dict.ContainsKey(tname))
                dict[tname] = dtSchema;
            else
                dict.Add(tname, dtSchema);
        }

        private DataTable TableSchema(TableName tname)
        {
            if (dict.ContainsKey(tname))
                return dict[tname];

            var dtSchema = tname.Provider.Schema.GetTableSchema(tname);
            Add(tname, dtSchema);
            return dtSchema;
        }

        private static TableSchemaManager mgr = mgr ?? new TableSchemaManager();

        public static DataTable GetTableSchema(TableName tname)
        {
            return mgr.TableSchema(tname);
        }

        public static void SetTableSchema(TableName tname, DataTable dt)
        {
            mgr.AddTableSchema(tname, dt);
        }
    }
}

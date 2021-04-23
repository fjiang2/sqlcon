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
    /// read records from database by DPO class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TableReader<T> where T : class,  IDPObject, new()
    {
        TableReader reader;

        /// <summary>
        /// read all records in the database
        /// </summary>
        public TableReader()
        {
            this.reader = new TableReader(TableName);
        }

        /// <summary>
        /// read records by filter
        /// </summary>
        /// <param name="where"></param>
        public TableReader(SqlExpr where)
        {
            this.reader = new TableReader(TableName, new SqlBuilder().SELECT().COLUMNS().FROM(TableName).WHERE(where).Clause);
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
                return this.reader.Table;
            }
        }

        /// <summary>
        /// override T.Fill(DataRow) to initialize varibles in the class T, if typeof(T).BaseType != typeof(DPObject)
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            //List<T> list = new List<T>();
            //foreach (DataRow dataRow in Table.Rows)
            //{
            //    T t = new T();
            //    t.Fill(dataRow);
            //    list.Add(t);
            //}
            //return this.list;

            return new DPList<T>(this).ToList();
        }


        /// <summary>
        /// returns the SQL clause
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.reader.ToString();
        }
    }
}

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
using System.Data.Common;
using System.Threading;
using Sys.Data;

namespace Sys.Data
{
    /// <summary>
    /// Read records from data base server into data table 
    /// </summary>
    public class TableReader
    {
        internal SqlCmd cmd;

        private string sql;
        private TableName tableName;
        private DataTable table;

        internal TableReader(TableName tableName, string sql)
        {
            this.sql = sql;
            this.tableName = tableName;
            this.cmd = new SqlCmd(tableName.Provider, sql);
        }


        /// <summary>
        /// read all records in the table defined
        /// </summary>
        /// <param name="tableName"></param>
        public TableReader(TableName tableName)
            : this(tableName, string.Format("SELECT * FROM {0}", tableName))
        {
        }


        /// <summary>
        /// read records by filter
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="where"></param>
        public TableReader(TableName tableName, SqlExpr where)
            : this(tableName, new SqlBuilder().SELECT.COLUMNS().FROM(tableName).WHERE(where).Clause)
        {
        }


        public int Count
        {
            get
            {
                var items = this.sql.ToUpper().Split(new string[] { "SELECT", "FROM" }, StringSplitOptions.RemoveEmptyEntries);
                string query = sql.Replace(items[0], " COUNT(*) ");
                object obj = new SqlCmd(tableName.Provider, query).ExecuteScalar();
                return (int)obj;
            }
        }

        /// <summary>
        /// return data table retrieved from data base server
        /// </summary>
        public DataTable Table
        {
            get
            {
                if (table == null)
                {
                    this.table = cmd.FillDataTable();
                }

                return this.table;
            }
        }

        /// <summary>
        /// override T.Fill(DataRow) to initialize varibles in the class T, if typeof(T).BaseType != typeof(DPObject)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> ToList<T>() where T : class, IDPObject, new()
        {
            return new DPList<T>(this).ToList();
        }


        /// <summary>
        /// returns SQL clause
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.sql;
        }

        public DataTable Read(CancellationTokenSource cts, IProgress<int> progress)
        {
            DataTable table = new DataTable();
            Action<DbDataReader> export = reader =>
            {
                table = BuildTable(reader);

                int step = 0;
                while (reader.Read())
                {
                    DataRow row;
                    while (reader.Read())
                    {
                        step++;

                        progress?.Report(step);

                        row = table.NewRow();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader.GetValue(i);
                        }

                        table.Rows.Add(row);

                        if (cts.IsCancellationRequested)
                            break;
                    }

                    table.AcceptChanges();
                }
            };

            cmd.Execute(export);

            return table;
        }

        internal static DataTable BuildTable(DbDataReader reader)
        {
            DataTable table = new DataTable();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                DataColumn column = new DataColumn(reader.GetName(i), reader.GetFieldType(i));
                table.Columns.Add(column);
            }

            table.AcceptChanges();

            return table;
        }
    }


}

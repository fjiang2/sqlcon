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

namespace Sys.Data
{
    /// <summary>
    /// Modeling SQL WHERE clause
    /// </summary>
    public class Locator : IDataPath
    {
        private string where;
        bool unique = true;

        /// <summary>
        /// WHERE [column1]=@column1 AND [column2]=@column2 AND ... 
        /// </summary>
        /// <param name="columns"></param>
        public Locator(string[] columns)
        {
            this.where = string.Join(" AND ", columns.Select(key => string.Format("[{0}]=@{0}", key)));
        }

        public Locator(IPrimaryKeys primary)
            :this(primary.Keys)
        {
        }

        public Locator(Locator locator)
        {
            this.where = locator.where;
        }

        public Locator(string any)
        {
            this.where = any;
        }

        public Locator(TableName tname)
            :this(tname.GetTableSchema().PrimaryKeys)
        {
        }

        public Locator(SqlExpr expression)
        {
            this.where = expression.ToString();
        }


        /// <summary>
        /// name of locator
        /// </summary>
        public string Name { get; set; }

        public void And(Locator locator)
        {
            this.where = string.Format("({0}) AND ({1})", this.where, locator.where);
        }

        public void Or(Locator locator)
        {
            this.where = string.Format("({0}) OR ({1})", this.where, locator.where);
        }

        /// <summary>
        /// One record is operated when [Unique] is true; Treat all records like one record when [Unique] is false
        /// </summary>
        public bool Unique
        {
            get { return this.unique; }
            internal set { this.unique = value; }
        }

        public static bool operator!(Locator locator)
        {
            return string.IsNullOrEmpty(locator.where);
        }

        public string Path
        {
            get { return where; }
        }

        public override string ToString()
        {
            return this.where;
        }

    }
}

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

namespace Sys.Data.Persistence.Level4
{
    /// <summary>
    /// n..n table mapping(many-to-many)
    /// T1: mapping table, e.g. Table UserRoles
    /// T2: many table, e.g. Table Roles
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class TableReader<T1, T2>
        where T1 : class,  IDPObject, new() 
        where T2 : class,  IDPObject, new() 
    {

        DataSet dataset;

        public TableReader(MappedColumn column1, MappedColumn column2, int value)
        {

            SqlBuilder relationships = new SqlBuilder()
                .SELECT.COLUMNS().FROM<T1>().WHERE(column1.RelationName.ColumnName() == value);

            SqlBuilder many = new SqlBuilder()
                .SELECT.COLUMNS()
                .FROM<T2>()
                .WHERE(column2.Name.ColumnName()
                    .IN(
                         new SqlBuilder()
                            .SELECT
                            .COLUMNS(column2.RelationName)
                            .FROM<T1>()
                            .WHERE(column1.RelationName.ColumnName() == value)
                        )
                    );


            this.dataset = (relationships + many).SqlCmd.FillDataSet();
            
        }

        public DataTable ManyTable
        {
            get { return this.dataset.Tables[0]; }
        }

        public DataTable MapTable
        {
            get { return this.dataset.Tables[0]; }
        }

    }


  
}

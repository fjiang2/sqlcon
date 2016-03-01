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
using Sys.Data.Comparison;

namespace Sys.Data
{
    class DatabaseClause
    {
        DatabaseName databaseName;

        public DatabaseClause(DatabaseName databaseName)
        {
            this.databaseName = databaseName;
        }

        public void CreateDatabase()
        {
            new SqlCmd(databaseName.Provider, $"CREATE DATABASE {databaseName.Name}").ExecuteNonQuery();
        }


        public string GenerateClause()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GenerateDropTableClause());
            builder.Append(GenerateScript_());
            return builder.ToString();
        }



        private string GenerateScript_()
        {
            StringBuilder builder = new StringBuilder();
            TableName[] history = databaseName.GetDependencyTableNames();

            foreach (var tableName in history)
            {
                Console.WriteLine("generate CREATE TABLE {0}", tableName.FormalName);
                try
                {
                    builder.AppendLine(tableName.GenerateCluase());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("failed to generate CREATE TABLE {0},{1}", tableName.FormalName, ex.Message);
                }
            }

            return builder.ToString();
        }

        private string GenerateDropTableClause()
        {
            TableName[] history = databaseName.GetDependencyTableNames();
            StringBuilder builder = new StringBuilder();
            foreach (var tableName in history.Reverse())
            {
                builder.AppendLine(new TableClause(new TableSchema(tableName)).IF_EXISTS_DROP_TABLE())
                    .AppendLine(TableClause.GO);
            }

            return builder.ToString();
        }

    }
}

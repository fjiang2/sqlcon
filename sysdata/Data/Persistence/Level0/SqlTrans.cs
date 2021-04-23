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
using System.Data.SqlClient;
using System.Data.Common;

namespace Sys.Data
{
   
    public sealed class SqlTrans
    {
        private SqlConnection connection;
        private SqlTransaction sqlTransaction;

        public SqlTrans()
        {
            this.connection = (SqlConnection)ConnectionProviderManager.DefaultProvider.NewDbConnection;
            this.connection.Open();
            this.sqlTransaction = connection.BeginTransaction();
        }


        
        public void Add(SqlCmd cmd)
        {
            if (cmd.DbProvider.DbCommand.Connection.State != ConnectionState.Closed)
                cmd.DbProvider.DbCommand.Connection.Close();


            cmd.DbProvider.DbCommand.Transaction = sqlTransaction;
            cmd.DbProvider.DbCommand.Connection = this.connection;
     
        }

        public void Add(PersistentObject dpo)
        {
            dpo.SetTransaction(this);
        }

        public void Add(RowAdapter row)
        {
            row.SetTransaction(this);
        }



        public void Commit()
        {
            try
            {
                sqlTransaction.Commit();
            }
            catch (Exception e)
            {
                try
                {
                    sqlTransaction.Rollback();
                }
                catch (SqlException ex)
                {
                    if (sqlTransaction.Connection != null)
                    {
                        throw new MessageException("An exception of type " + ex.GetType() + " was encountered while attempting to roll back the transaction.");
                    }
                }

                throw new MessageException(e.Message);
            }
            finally
            {
                connection.Close();
            }

        }


      

        public static void SqlCmdExample()
        {
            var transaction = new SqlTrans();
            var cmd1 = new SqlCmd("UPDATE ... ");
            var cmd2 = new SqlCmd("INSERT ...");

            transaction.Add(cmd1);
            transaction.Add(cmd2);

            cmd1.ExecuteNonQuery();
            cmd2.ExecuteNonQuery();

            transaction.Commit();
        }
    }
}

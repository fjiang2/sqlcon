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
using System.Text;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using Tie;

namespace Sys.Data
{
    public class SqlCmd : DbCmd
    {

        public SqlCmd(ConnectionProvider provider, string script, object parameters)
            : base(provider, script)
        {
            if (parameters == null)
                return;

            if (parameters is VAL)
            {
                foreach (var parameter in (VAL)parameters)
                {
                    AddParameter("@" + (string)parameter[0], parameter[1].HostValue);
                }
            }
            else
                foreach (var propertyInfo in parameters.GetType().GetProperties())
                {
                    AddParameter("@" + propertyInfo.Name, propertyInfo.GetValue(parameters));
                }

        }

        public SqlCmd(ConnectionProvider provider, string script)
            : base(provider, script)
        {
        }


        public SqlCmd(string script)
            : this(ConnectionProviderManager.DefaultProvider, script)
        {
        }

        public SqlCmd(ISqlBuilder builder)
            : this(builder.Provider, builder.Clause)
        { 
        
        }


        public void ChangeConnection(string userName, string password)
        {
            string serverName = "";
            string initialCatalog = "";
            string[] L1 = connection.ConnectionString.Split(new char[] { ';' });
            foreach (string s1 in L1)
            {
                string[] L2 = s1.Split(new char[] { '=' });
                if (L2[0] == "data source")
                    serverName = L2[1];
                else if (L2[0] == "initial catalog")
                    initialCatalog = L2[1];

            }

            string connectionString = string.Format("data source={0};initial catalog={1};user id={2};password={3};persist security info=True;packet size=4096", 
                serverName,
                initialCatalog, 
                userName, 
                password);

            ChangeConnection(new ConnectionProvider(base.provider.Handle, serverName, ConnectionProviderType.SqlServer, connectionString));
        }

  

     

        public DbParameter AddParameter(string parameterName, object value)
        {
            DbParameter param = dbProvider.AddParameter(parameterName, value);
            return param;
        }


        internal DbProvider DbProvider
        {
            get { return this.dbProvider; }
        }

    
        public override DataSet FillDataSet(DataSet dataSet)
        {
            try
            {
                connection.Open();
                dbProvider.FillDataSet(dataSet);
                return dataSet;
            }
            catch (Exception ex)
            {
                OnError(new SqlExceptionEventArgs(command, ex));
            }
            finally
            {
                connection.Close();
            }

            return null;
        }


        public override DataTable FillDataTable(DataSet dataSet, string tableName)
        {
            try
            {
                connection.Open();
                dbProvider.FillDataTable(dataSet, tableName);
            }
            catch (Exception ex)
            {
                OnError(new SqlExceptionEventArgs(command, ex));
            }
            finally
            {
                connection.Close();
            }

            return dataSet.Tables[tableName];
        }



        public override DataTable FillDataTable(DataTable table)
        {
            try
            {
                connection.Open();
                dbProvider.FillDataTable(table);
                return table;
            }
            catch (Exception ex)
            {
                OnError(new SqlExceptionEventArgs(command, ex));
            }
            finally
            {
                connection.Close();
            }

            return null;
        }


      
        public int ExecuteNonQueryTransaction()
        {
            string splitter = TableScript.GO + "\r\n";
            string[] clauses = base.script.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
            return ExecuteNonQueryTransaction(clauses);
        }

        public int ExecuteNonQueryTransaction(IEnumerable<string> clauses)
        {
            int count = -1;
            using (SqlConnection connection = (SqlConnection)provider.NewDbConnection)
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                SqlCommand command = connection.CreateCommand();
                command.Transaction = transaction;

                try
                {
                    foreach (string clause in clauses)
                    {
                        command.CommandText = clause;
                        count = command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    OnError(new SqlExceptionEventArgs(command, ex));
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception exRollback)
                    {
                        OnError(new SqlExceptionEventArgs(command, exRollback));
                    }
                }
            }

            return count;
        }

    }
}


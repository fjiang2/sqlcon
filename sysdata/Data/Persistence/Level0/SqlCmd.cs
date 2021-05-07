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
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using Tie;

namespace Sys.Data
{
    public sealed class SqlCmd : DbCmd
    {

        public SqlCmd(ConnectionProvider provider, string script)
            : base(provider, script)
        {
        }

        /// <summary>
        /// use default connection provider, use ConnectionProviderManager.RegisterDefaultProvider() to register
        /// </summary>
        /// <param name="script"></param>
        public SqlCmd(string script)
            : this(ConnectionProviderManager.DefaultProvider, script)
        {
        }

        /// <summary>
        /// use default connection provider, use ConnectionProviderManager.RegisterDefaultProvider() to register
        /// parameters can be VAL, Dictionary, Json, DbParameter, and anonymous class
        /// </summary>
        /// <param name="script"></param>
        /// <param name="parameters"></param>
        public SqlCmd(string script, object parameters)
         : this(ConnectionProviderManager.DefaultProvider, script)
        {
            ParseParameters(parameters);
        }

        public SqlCmd(ISqlBuilder builder)
            : this(builder.Provider, builder.Query)
        {
        }

        /// <summary>
        /// parameters can be VAL, Dictionary, Json, DbParameter, and anonymous class
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SqlCmd ParseParameters(object parameters)
        {
            if (parameters == null)
                return this;

            if (parameters is VAL)
            {
                foreach (var parameter in (VAL)parameters)
                {
                    AddParameter((string)parameter[0], parameter[1].HostValue);
                }
            }
            //Dictionary
            else if (parameters is IEnumerable<KeyValuePair<string, object>>)
            {
                var args = (IEnumerable<KeyValuePair<string, object>>)parameters;
                foreach (var kvp in args)
                {
                    AddParameter(kvp.Key, kvp.Value);
                }
            }
            //JSON
            else if (parameters is string)
            {
                string args = (string)parameters;
                if (string.IsNullOrEmpty(args))
                    return this;

                VAL val = Script.Evaluate(args);
                if (val.IsAssociativeArray())
                {
                    foreach (var element in val)
                    {
                        if (element[0].Value is string)
                        {
                            string name = (string)element[0];
                            object value = element[1].HostValue;

                            AddParameter(name, value);
                        }
                    }
                }
                else
                {
                    throw new Exception($"invalid json parameters: {parameters}");
                }
            }
            //DbParameters
            else if (parameters is IEnumerable<IDataParameter>)
            {
                var args = (IEnumerable<IDataParameter>)parameters;
                foreach (var arg in args)
                {
                    var p = AddParameter(arg.ParameterName, arg.Value);
                    p.DbType = arg.DbType;
                    p.Direction = arg.Direction;
                }
            }
            else
            {
                var args = parameters.GetType().GetProperties().Where(p => p.CanRead);
                foreach (var propertyInfo in args)
                {
                    AddParameter(propertyInfo.Name, propertyInfo.GetValue(parameters));
                }
            }

            return this;
        }



        public DbParameter AddParameter(string parameterName, object value)
        {
            if (!parameterName.StartsWith("@"))
                parameterName = "@" + parameterName;

            if (value == null)
                value = DBNull.Value;

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
            string splitter = SqlScript.GO + Environment.NewLine;
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


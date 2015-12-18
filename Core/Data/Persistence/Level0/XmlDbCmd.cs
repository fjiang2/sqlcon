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
    public class XmlDbCmd : DbCmd
    {

        public XmlDbCmd(ConnectionProvider provider, string script, object parameters)
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

        public XmlDbCmd(ConnectionProvider provider, string script)
            : base(provider, script)
        {
        }


        public XmlDbCmd(string script)
            : this(ConnectionProviderManager.DefaultProvider, script)
        {
        }

        public XmlDbCmd(ISqlBuilder builder)
            : this(builder.Provider, builder.Clause)
        { 
        
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


    
    }
}


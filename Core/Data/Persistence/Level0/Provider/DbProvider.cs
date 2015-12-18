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
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace Sys.Data
{
    public abstract class DbProvider
    {
        protected readonly ConnectionProvider provider;
        protected readonly string script;

   

        public DbProvider(string script, ConnectionProvider provider)
        {
            this.script = script;
            this.provider = provider;

            this.DbConnection = this.provider.NewDbConnection;
            this.DbCommand = NewDbCommand();

            if (this.script.Contains(" "))  //Stored Procedure Name does not contain a space letter
                this.DbCommand.CommandType = CommandType.Text;
            else
                this.DbCommand.CommandType = CommandType.StoredProcedure;
        }

        public DbConnection DbConnection { get; private set; }
        public DbCommand DbCommand { get; private set; }

        public DbProviderType DbType
        {
            get { return this.provider.DpType; }
        }

        protected abstract DbDataAdapter NewDbDataAdapter();
        protected abstract DbCommand NewDbCommand();

        public void FillDataSet(DataSet dataSet)
        {
            DbDataAdapter adapter = NewDbDataAdapter();
            adapter.Fill(dataSet);
        }

        public void FillDataTable(DataSet dataSet, string tableName)
        {
            DbDataAdapter adapter = NewDbDataAdapter();
            adapter.Fill(dataSet, tableName);
        }

        public void FillDataTable(DataTable table)
        {
            DbDataAdapter adapter = NewDbDataAdapter();
            adapter.Fill(table);
        }


        public abstract DbParameter AddParameter(string parameterName, Type type);
        public abstract DbParameter AddParameter(string parameterName, object value);
        

        public static DbProvider Factory(string script, ConnectionProvider provider)
        {
            switch (provider.DpType)
            {
                case DbProviderType.SqlDb:
                    return new SqlProvider(script, provider);

                case DbProviderType.OleDb:
                    return new OleDbProvider(script, provider);

                case  DbProviderType.XmlDb:
                    return new XmlDbProvider(script, provider);
            }

            throw new NotImplementedException();
        }
        
    }
}

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
using System.Data.SQLite;
using System.Data;

namespace Sys.Data
{
    class SqliteProvider : DbProvider
    {
        public SqliteProvider(string script, ConnectionProvider provider)
            : base(script, provider)
        { 
        }

      

        protected override DbDataAdapter NewDbDataAdapter()
        {
            SQLiteDataAdapter adapter = new SQLiteDataAdapter();
            adapter.SelectCommand = (SQLiteCommand)base.DbCommand;
            return adapter;
        }

        protected override DbCommand NewDbCommand()
        {
            return new SQLiteCommand(script, (SQLiteConnection)DbConnection);
        }

        public override DbParameter AddParameter(string parameterName, Type type)
        {
            SQLiteParameter param = new SQLiteParameter(parameterName, type.ToCType().ToSqlDbType());
            DbCommand.Parameters.Add(param);
            return param;
        }
      

        public override DbParameter AddParameter(string parameterName, object value)
        {
            SqlDbType dbType = SqlDbType.NVarChar;
            if (value is int)
                dbType = SqlDbType.Int;
            else if (value is short)
                dbType = SqlDbType.SmallInt;
            else if (value is long)
                dbType = SqlDbType.BigInt;
            else if (value is byte)
                dbType = SqlDbType.TinyInt;
            else if (value is DateTime)
                dbType = SqlDbType.DateTime;
            else if (value is DateTimeOffset)
                dbType = SqlDbType.DateTimeOffset;
            else if (value is double)
                dbType = SqlDbType.Float;
            else if (value is float)
                dbType = SqlDbType.Float;
            else if (value is decimal)
                dbType = SqlDbType.Decimal;
            else if (value is bool)
                dbType = SqlDbType.Bit;
            else if (value is string && ((string)value).Length > 4000)
                dbType = SqlDbType.NText;
            else if (value is string)
                dbType = SqlDbType.NVarChar;
            else if (value is byte[])
                dbType = SqlDbType.Binary;
            else if (value is Guid)
                dbType = SqlDbType.UniqueIdentifier;

            SQLiteParameter param = new SQLiteParameter(parameterName, dbType);
            param.Value = value;
            param.Direction = ParameterDirection.Input;
            DbCommand.Parameters.Add(param);
            return param;
        }

    }
}

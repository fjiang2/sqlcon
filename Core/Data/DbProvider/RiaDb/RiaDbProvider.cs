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
    class RiaDbProvider : SqlDbProvider
    {
        public RiaDbProvider(string script, ConnectionProvider provider)
            : base(script, provider)
        { 
        }

      

        protected override DbDataAdapter NewDbDataAdapter()
        {
            RiaDbDataAdapter adapter = new RiaDbDataAdapter();
            adapter.SelectCommand = (RiaDbCommand)base.DbCommand;
            return adapter;
        }

        protected override DbCommand NewDbCommand()
        {
            return new RiaDbCommand(script, (RiaDbConnection)DbConnection);
        }

      

    }
}

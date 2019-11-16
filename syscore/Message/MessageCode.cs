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

namespace Sys
{
    /// <summary>
    /// system message code
    /// </summary>
    public enum MessageCode
    {
        /// <summary>
        /// nothing
        /// </summary>
        None = 0,

        /// <summary>
        /// password expired
        /// </summary>
        PasswordExpried =1001,

        /// <summary>
        /// user account closed
        /// </summary>
        AccountClosed = 1002,

        /// <summary>
        /// account is locked
        /// </summary>
        AccountLocked = 1003,

        /// <summary>
        /// record is overwritten
        /// </summary>
        RecordOverwritten =2011,

        /// <summary>
        /// table does not exist
        /// </summary>
        TableNotExist = 2012
    }

}

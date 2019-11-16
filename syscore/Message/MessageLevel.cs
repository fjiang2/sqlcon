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
    /// message level
    /// </summary>
    public enum MessageLevel
    {
        /// <summary>
        /// no error
        /// </summary>
        None = 0,

        /// <summary>
        /// information level
        /// </summary>
        Information = 1,

        /// <summary>
        /// warning level
        /// </summary>
        Warning = 2,

        /// <summary>
        /// error level
        /// </summary>
        Error = 3,

        /// <summary>
        /// fatal error level
        /// </summary>
        Fatal = 4,

        /// <summary>
        /// used to confirm the error message
        /// </summary>
        Confirmation = 5
    }
}

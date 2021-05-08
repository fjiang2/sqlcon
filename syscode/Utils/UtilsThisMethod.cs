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

namespace Sys.CodeBuilder
{
    [Flags]
    public enum UtilsThisMethod
    {
        Undefined = 0x00,
        Copy = 0x01,
        Clone = 0x02,
        Compare = 0x04,
        ToString = 0x08,
        Equals = 0x10,
        GetHashCode = 0x20,
        Map = 0x40,
        ToDictionary = 0x80,
    }
}

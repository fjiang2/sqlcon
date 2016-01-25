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

namespace Sys.CodeBuilder
{
    [Flags]
    public enum AccessModifier
    {
        Public = 0x01,
        Internal = 0x02,
        Protected = 0x04,
        Private = 0x08,
        Const = 0x10,
        Static = 0x20,
        Virtual = 0x40,
        Override = 0x80,
        Abstract = 0x100,
        Sealed = 0x200,
        Readonly = 0x400,
        Event = 0x800,
        Extern = 0x10000,
        Unsafe = 0x20000,
        Volatile = 0x40000,
        Partial = 0x80000
    }
}

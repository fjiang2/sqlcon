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

namespace Sys.Data.Manager
{
    public static class Setting
    {
        public const string DPO_CLASS_SUB_NAMESPACE = "DpoClass";
        public const string DPO_CLASS_SUFFIX_CLASS_NAME = "Dpo";
        public const string DPO_CLASS_PATH = "MgCode\\DpoClass";

        public const string DPO_PACKAGE_SUB_NAMESPACE = "DpoPackage";
        public const string DPO_PACKAGE_SUFFIX_CLASS_NAME = "Package";
        public const string DPO_PACKAGE_PATH = "MgCode\\DpoPackage";


        public const string ENUM_SUB_NAMESPACE = "DataEnum";
        public const string ENUM_SUFFIX_STRUCT_NAME = "Enum";
        public const string ENUM_PATH = "MgCode\\DataEnum";

    }
}

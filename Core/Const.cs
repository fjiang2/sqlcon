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
using System.IO;
using System.Reflection;

namespace Sys.Data
{
    public class Const
    {
        public static string CONNECTION_STRING = "data source=localhost\\SQLEXPRESS;initial catalog=master;integrated security=SSPI;packet size=4096";

 
        public static string DB_APPLICATION = "";
        public static string DB_SYSTEM = DB_APPLICATION;
        public static int DB_SYSTEM_ID = 1;


        public static int DB_REVISION = 1;

       
    }

}
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
using System.Data;
using Sys.Data;

namespace Sys.Data.Manager
{
    class SpParam
    {
        SpParamDpo param;
        CType dbType;

        string name;
        string type;

        public SpParam(SpParamDpo param)
        {
            this.param = param;
            this.name = param.name.Substring(1);
            this.type = ColumnSchema.GetFieldType(param.type, false);
            this.dbType = ColumnSchema.GetCType(param.type);

        }


        public string signuture1()
        {
            if(param.is_output)
                return string.Format("ref {0} {1}", type, name);
            else
                return string.Format("{0} {1}", type, name);
        }


        public string signuture2()
        {
            if (param.is_output)
                return string.Format("ref {0}", name);
            else
                return name;
        }

        public string param1()
        {
            if (param.is_output)
                return string.Format("cmd.AddParameter(\"{0}\", CType.{1}, {2});\r\n", param.name, dbType, param.max_length);
            else
                return string.Format("cmd.AddParameter(\"{0}\", CType.{1}, {2}, {3});\r\n", param.name, dbType, param.max_length, name);
        }

        public string param2()
        {
            if (param.is_output)
                return string.Format("{0} = ({1})cmd[\"{2}\"];\r\n", name, type, param.name);

            return "";
        }

        
    }


  
}

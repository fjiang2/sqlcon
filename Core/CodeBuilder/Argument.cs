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
    public class Argument
    {
        Type type;
        string name;

        public Argument(Type type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public Argument(string name)
        {
            this.type = null;
            this.name = name;
        }


        public string Text
        {
            get
            {
                if (type == null)
                    return name;

                return string.Format("{0} {1}", new TypeInfo(type).Text, name);
            }
        }

        public override string ToString()
        {
            return Text;
        }
    }
}

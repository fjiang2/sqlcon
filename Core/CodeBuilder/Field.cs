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
    public class Field : Declare, ICodeBlock
    {
        private object value;

        public Field(TypeInfo type, string fieldName)
            :this(type, fieldName, null)
        {
        }


        public Field(TypeInfo type, string fieldName, object value)
            :base(fieldName)
        {
            this.type = type;
            this.value = value;
        }


        public override string ToString()
        {
            return GetBlock().ToString();
        }

        public CodeBlock GetBlock()
        {
            CodeBlock block = new CodeBlock();
            if (value != null)
            {
                block.AppendFormat("{0} = {1};", Signture, value);
            }
            else
            {
                block.AppendLine(Signture);
            }

            return block;
        }
    }
}

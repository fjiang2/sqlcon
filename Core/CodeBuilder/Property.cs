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
    public class Property  : Declare, ICodeBlock
    {
        public Statement gets { get; } = new Statement();
        public Statement sets { get; } = new Statement();

        public Property(TypeInfo returnType, string propertyName)
            :base(propertyName)
        {
            this.type = returnType;
        }


        public override string ToString()
        {
            return GetBlock().ToString();
        }

        public CodeBlock GetBlock()
        {
            CodeBlock block = new CodeBlock();
            if (gets.Count == 0 && sets.Count == 0)
            {
                block.AppendFormat("{0} {{get; set; }}", Signture);
            }
            else
            {
                block.AppendLine(Signture);
                block.AppendLine("{");
                if (gets.Count != 0)
                {
                    block.AppendLine("get", 1);
                    block.AppendWrap(gets);
                }

                if (sets.Count != 0)
                {
                    block.AppendLine("set" ,1);
                    block.AppendWrap(sets);
                }

                block.AppendLine("}");
            }

            return block;
        }
    }
}

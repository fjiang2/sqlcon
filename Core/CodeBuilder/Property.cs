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
        private object value;

        public Statement gets { get; } = new Statement();
        public Statement sets { get; } = new Statement();

        public Property(TypeInfo returnType, string propertyName)
            : this(returnType, propertyName, null)
        {
        }

        public Property(TypeInfo returnType, string propertyName, object value)
            :base(propertyName)
        {
            this.type = returnType;
            this.value = value;
        }


        protected override CodeBlock BuildBlock()
        {
            CodeBlock block = base.BuildBlock();

            if (gets.Count == 0 && sets.Count == 0)
            {
                block.AppendFormat("{0}{1}", $"{Signture} {{ get; set; }}", comment);
            }
            else
            {
                block.AppendLine(Signture + comment);
                block.Begin();
                if (gets.Count != 0)
                {
                    block.AppendLine("get");
                    block.AddWithBeginEnd(gets);
                }

                if (sets.Count != 0)
                {
                    block.AppendLine("set");
                    block.AddWithBeginEnd(sets);
                }

                block.End();
            }

            return block;
        }
    }
}

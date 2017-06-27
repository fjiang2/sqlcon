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
    public class Enum : Declare, ICodeBlock
    {
        public Statement statements { get; } = new Statement();


        public Enum(string enumName)
            :base(enumName)
        {
            type = new TypeInfo { userType = "enum" };
        }

        public void Add(string feature)
        {
            statements.AppendLine($"{feature},");
        }

        public void Add(string feature, int value)
        {
            statements.AppendLine($"{feature} = {value},");
        }

        public void Add(string feature, int value, string label)
        {
            statements.AppendLine($"[DataEnum(\"{label}\")]");
            statements.AppendLine($"{feature} = {value},");
            statements.AppendLine();
        }

        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            block.AppendLine(Signature);
            block.AddWithBeginEnd(statements);
        }
    }
}

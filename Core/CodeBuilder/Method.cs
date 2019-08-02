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
    public class Method  : Declare, ICodeBlock
    {
        public Statement Statement { get; } = new Statement();

        public Arguments Args { get; set; } = new Arguments();

        public bool IsExtensionMethod { get; set; } = false;

        public Method(TypeInfo returnType, string methodName)
            :base(methodName)
        {
            base.Type = returnType;
        }

        public Method(string methodName)
            :base(methodName)
        {
        }

        protected string signature
        {
            get
            {
                if (IsExtensionMethod)
                {
                    return string.Format("{0}(this {1})", Signature, Args);
                }
                else
                {
                    return string.Format("{0}({1})", Signature, Args);
                }
            }
        }

        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            block.AppendLine(signature);
            block.AddWithBeginEnd(Statement);
        }
    }
}

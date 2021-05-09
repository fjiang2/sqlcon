//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        syscode(C# Code Builder)                                                                  //
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
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Directive : Buildable
    {
        private CodeBlock code = new CodeBlock();

        public Directive(string line)
        {
            code.AppendLine(line);
        }

        public Directive Add(string line)
        {
            code.AppendLine(line);
            return this;
        }

        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            block.Add(code);
        }

    }
}

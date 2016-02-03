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
    public class Constructor : ICodeBlock
    {
        public Arguments args { get; set; } = new Arguments();
        public Arguments baseAgrs { get; set; } = new Arguments();

        public Statement statements { get; } = new Statement();

        public AccessModifier modifier { get; set; } = AccessModifier.Public;
        private string constructorName;

        public Constructor(string constructorName )
        {
            this.constructorName = constructorName;
        }

    
        public override string ToString()
        {
            return GetBlock().ToString();
        }

        public CodeBlock GetBlock()
        {
            CodeBlock block = new CodeBlock();

            string _constructor = string.Format("{0}{1}({2})", new Modifier(modifier), constructorName, args);
            string _base = string.Format(":base({0})", baseAgrs);

            block.AppendLine(_constructor);
            if (!baseAgrs.IsEmpty)
            {
                block.AppendLine(_base, 1);
            }

            block.AppendWrap(statements);
            return block;
        }
        

    }
}

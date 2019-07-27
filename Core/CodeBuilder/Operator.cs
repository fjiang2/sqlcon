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
    public class Operator : Declare, ICodeBlock
    {
        public Arguments args { get; set; } = new Arguments();

        public Statement statements { get; } = new Statement();


        public Operator(TypeInfo returnType, string operation)
            : base("operator " + operation)
        {
            base.modifier = Modifier.Public | Modifier.Static;
            base.type = returnType;
        }

        public static Operator Implicit(TypeInfo operation, Argument arg)
        {
            Operator opr = new Operator(null, operation.ToString())
            {
                modifier = Modifier.Public | Modifier.Static | Modifier.Implicit,
            };
            opr.args.Add(arg);

            return opr;
        }

        public static Operator Explicit(TypeInfo operation, Argument arg)
        {
            Operator opr = new Operator(null, operation.ToString())
            {
                modifier = Modifier.Public | Modifier.Static | Modifier.Explicit,
            };
            opr.args.Add(arg);

            return opr;
        }

        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            block.AppendFormat("{0}({1})", Signature, args);

            block.AddWithBeginEnd(statements);
        }
    }
}

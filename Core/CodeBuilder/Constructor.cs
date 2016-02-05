﻿//--------------------------------------------------------------------------------------------------//
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
    public class Constructor : Declare, ICodeBlock
    {
        public Arguments args { get; set; } = new Arguments();
        public string[] baseArgs { get; set; }

        public Statement statements { get; } = new Statement();


        public Constructor(string constructorName )
            :base(constructorName)
        {
            base.modifier = Modifier.Public;
            base.type = null;
        }

        protected override CodeBlock BuildBlock()
        {
            CodeBlock block = base.BuildBlock();

            block.AppendFormat("{0}({1})", Signture, args);
            if (baseArgs != null)
            {
                block.Indent().AppendFormat(":base({0})", string.Join(",", baseArgs)).Unindent();
            }

            block.AddWithBeginEnd(statements);
            return block;
        }
        

    }
}

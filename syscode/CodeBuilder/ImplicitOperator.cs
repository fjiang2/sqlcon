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
    public class ImplicitOperator : Member, IBuildable
    {

        public ImplicitOperator(TypeInfo returnType, string operation)
            : base("operator " + operation)
        {
            base.Modifier = Modifier.Public | Modifier.Static;
            base.Type = returnType;
        }

        public static ImplicitOperator Implicit(TypeInfo operation, Parameter parameter)
        {
            ImplicitOperator opr = new ImplicitOperator(null, operation.ToString())
            {
                Modifier = Modifier.Public | Modifier.Static | Modifier.Implicit,
            };
            opr.Params.Add(parameter);

            return opr;
        }

        public static ImplicitOperator Explicit(TypeInfo operation, Parameter parameter)
        {
            ImplicitOperator opr = new ImplicitOperator(null, operation.ToString())
            {
                Modifier = Modifier.Public | Modifier.Static | Modifier.Explicit,
            };
            opr.Params.Add(parameter);

            return opr;
        }


        protected override string signature => $"{Signature}({Params})";

    }
}

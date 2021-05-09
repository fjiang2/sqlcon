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
    /// <summary>
    ///  public static implicit operator Expression(int value)
    ///  {
    ///       return new Expression(value);
    ///  }
    /// </summary>
    class ImplicitOperator : Member, IBuildable
    {
        public ImplicitOperator(TypeInfo operation, Parameter parameter)
            : base("operator " + operation)
        {
            base.Modifier = Modifier.Public | Modifier.Static | Modifier.Implicit;
            base.Type = null;
            Params.Add(parameter);
        }

        protected override string signature => $"{Signature}({Params})";
    }
}

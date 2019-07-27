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
    sealed class ModifierString
    {
        Modifier modifier;

        public ModifierString(Modifier modifier)
        {
            this.modifier = modifier;
        }


        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            if ((modifier & Modifier.Public) == Modifier.Public)
                s.Append("public ");
            else if ((modifier & Modifier.Private) == Modifier.Private)
                s.Append("private ");
            else if ((modifier & Modifier.Internal) == Modifier.Internal)
                s.Append("internal ");
            else if ((modifier & Modifier.Protected) == Modifier.Protected)
                s.Append("protected ");

            if ((modifier & Modifier.Static) == Modifier.Static)
                s.Append("static ");
            if ((modifier & Modifier.Partial) == Modifier.Partial)
                s.Append("partial ");


            if ((modifier & Modifier.Const) == Modifier.Const)
                s.Append("const ");
            else if ((modifier & Modifier.Readonly) == Modifier.Readonly)
                s.Append("readonly ");

            if ((modifier & Modifier.Virtual) == Modifier.Virtual)
                s.Append("virtual ");
            else if ((modifier & Modifier.Override) == Modifier.Override)
                s.Append("override ");

            if ((modifier & Modifier.Abstract) == Modifier.Abstract)
                s.Append("abstract ");
            else if ((modifier & Modifier.Sealed) == Modifier.Sealed)
                s.Append("sealed ");

            if ((modifier & Modifier.Event) == Modifier.Event)
                s.Append("event ");


            if ((modifier & Modifier.Implicit) == Modifier.Implicit)
                s.Append("implicit ");
            if ((modifier & Modifier.Explicit) == Modifier.Explicit)
                s.Append("explicit ");
            if ((modifier & Modifier.Operator) == Modifier.Operator)
                s.Append("operator ");

            return s.ToString().TrimEnd();
        }
    }
}

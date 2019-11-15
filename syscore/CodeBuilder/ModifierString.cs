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
        private readonly Modifier modifier;

        public ModifierString(Modifier modifier)
        {
            this.modifier = modifier;
        }

        private bool Has(Modifier feature)
        {
            return (this.modifier & feature) == feature;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            if (Has(Modifier.Public))
                s.Append("public ");
            else if (Has(Modifier.Private))
                s.Append("private ");
            else if (Has(Modifier.Internal))
                s.Append("internal ");
            else if (Has(Modifier.Protected))
                s.Append("protected ");

            if (Has(Modifier.Static))
                s.Append("static ");
            if (Has(Modifier.Partial))
                s.Append("partial ");


            if (Has(Modifier.Const))
                s.Append("const ");
            else if (Has(Modifier.Readonly))
                s.Append("readonly ");

            if (Has(Modifier.Virtual))
                s.Append("virtual ");
            else if (Has(Modifier.Override))
                s.Append("override ");

            if (Has(Modifier.Abstract))
                s.Append("abstract ");
            else if (Has(Modifier.Sealed))
                s.Append("sealed ");

            if (Has(Modifier.Event))
                s.Append("event ");


            if (Has(Modifier.Implicit))
                s.Append("implicit ");
            else if (Has(Modifier.Explicit))
                s.Append("explicit ");

            if (Has(Modifier.Operator))
                s.Append("operator ");

            return s.ToString().TrimEnd();
        }
    }
}

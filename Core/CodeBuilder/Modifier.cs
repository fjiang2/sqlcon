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
    sealed class Modifier
    {
        public const string CRLF = "\r\n";

        AccessModifier modifier;

        public Modifier(AccessModifier modifier)
        {
            this.modifier = modifier;
        }

        public AccessModifier ModifierType
        {
            get { return this.modifier; }
        }

        public string Text
        {
            get { return this.ToString(); }
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            if ((modifier & AccessModifier.Public) == AccessModifier.Public)
                s.Append("public ");
            else if ((modifier & AccessModifier.Private) == AccessModifier.Private)
                s.Append("private ");
            else if ((modifier & AccessModifier.Internal) == AccessModifier.Internal)
                s.Append("internal ");
            else if ((modifier & AccessModifier.Protected) == AccessModifier.Protected)
                s.Append("protected ");


            if ((modifier & AccessModifier.Const) == AccessModifier.Const)
                s.Append("const ");
            else if ((modifier & AccessModifier.Static) == AccessModifier.Static)
                s.Append("static ");
            else if ((modifier & AccessModifier.Readonly) == AccessModifier.Readonly)
                s.Append("readonly ");

            
            if ((modifier & AccessModifier.Virtual) == AccessModifier.Virtual)
                s.Append("virtual ");
            else if ((modifier & AccessModifier.Override) == AccessModifier.Override)
                s.Append("override ");

            if ((modifier & AccessModifier.Abstract) == AccessModifier.Abstract)
                s.Append("abstract ");
            else if ((modifier & AccessModifier.Sealed) == AccessModifier.Sealed)
                s.Append("sealed ");

            if ((modifier & AccessModifier.Event) == AccessModifier.Event)
                s.Append("event ");

            return s.ToString();
        }
    }
}

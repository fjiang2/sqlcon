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
    public class Field
    {
        string fieldSignature;

        public Field(Type type, string fieldName)
            : this(AccessModifier.Private, type, fieldName)
        {
        }


        public Field(AccessModifier modifier, Type type, string fieldName)
        {
            this.fieldSignature = string.Format("{0}{1} {2};",
                new Modifier(modifier),
                new TypeInfo(type),
                fieldName);
        }


        public Field(AccessModifier modifier, Type type, string fieldName, object value)
        {
            this.fieldSignature = string.Format("{0}{1} {2} = {3};",
                new Modifier(modifier),
                new TypeInfo(type),
                fieldName, 
                value);
        }


        public string Text
        {
            get { return this.fieldSignature; }
        }

        public override string ToString()
        {
            return this.fieldSignature;
        }
    }
}

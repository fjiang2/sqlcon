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
    public class Method : Format
    {
        private string signature;

        Statements statements = new Statements(2);

        
        //internal string Method(....)
        public Method(AccessModifier modifier, Type returnType, string methodName)
            : this(modifier, returnType, methodName, new Argument[] { })
        {
        }

        //protected void Method(....)
        public Method(AccessModifier modifier, Type returnType, string methodName, Argument[] args)
        {
            this.signature = string.Format("{0}{1} {2}({3})", 
                new Modifier(modifier), 
                returnType == null ? "void" : new TypeInfo(returnType).Text, 
                methodName,
                string.Join(", ", args.Select(arg=>arg.Text))
                );
        }

      
        public Method(AccessModifier modifier, string methodName)
            : this(modifier, methodName, new Argument[] { })
        {
        }

        public Method(AccessModifier modifier, string methodName,  Argument[] args)
        {
            this.signature = string.Format("{0}void {1}({2})",
                new Modifier(modifier), 
                methodName,
                string.Join(", ", args.Select(arg => arg.Text))
                );
        }

        public Method AddStatements(string format, params object[] args)
        {
            statements.Add(string.Format(format, args));
            return this;
        }

        public Method AddStatements()
        {
            statements.Add("");
            return this;
        }

        public Method AddField(Field field)
        {
            statements.Add(field.ToString());
            return this;
        }



        public string Text
        {
            get 
            {
                this.tab = 2;

                this.Add(this.signature);
                code.Append(statements.Code);

                return code.ToString();
            }
        }

        public override string ToString()
        {
            return Text;
        }
        

    }
}

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
    public class Method : Member
    {
        private CompoundStatement statements = new CompoundStatement(2);

        public Arguments args { get; set; } = new Arguments();

        //protected void Method(....)
        public Method(string methodName)
            :base(methodName)
        {
        }

        protected override string signature
        {
            get
            {
                var line = string.Format("{0} {1} {2}({3})", new Modifier(modifier), type, name, args);
                return line;
            }
        }


        public Method AddStatement(string format, params object[] args)
        {
            statements.Add(string.Format(format, args));
            return this;
        }

        public Method AddStatement(Statement statment)
        {
            statements.Add(statment.ToString());
            return this;
        }

        public Method AppendLine()
        {
            statements.Add("");
            return this;
        }

        public Method AddField(Field field)
        {
            statements.Add(field.ToString());
            return this;
        }



        public override string ToString()
        {
            this.tab = 2;

            this.Add(this.signature);
            code.Append(statements.Code);

            return code.ToString();
        }
    }
}

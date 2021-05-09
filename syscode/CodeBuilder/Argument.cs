//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        syscode(C# Code Builder)                                                                  //
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
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Argument
    {
        public string NamedArg { get; set; }
        public Expression Arg { get; set; }

        public Argument(Expression arg)
        {
            this.Arg = arg;
        }

        public Argument(string namedArg, Expression arg)
        {
            this.NamedArg = namedArg;
            this.Arg = arg;
        }

        public static implicit operator Argument(Expression expr)
        {
            return new Argument(expr);
        }

        public static implicit operator Argument(string argument)
        {
            return new Argument(argument);
        }

        public override string ToString()
        {
            if (NamedArg != null)
                return $"{NamedArg} : {Arg}";
            else
                return Arg.ToString();
        }
    }
}

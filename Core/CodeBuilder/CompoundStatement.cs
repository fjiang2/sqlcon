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
    public class Statements : CodeBlock
    {
        public Statements()
        {
        }

        public int Count
        {
            get { return this.list.Count; }
        }

        public Statements COMPOUND(Statements statements)
        {
            Statements statement = new Statements();
            statement.IsCompound = true;
            statement.Append(statements);
            return this;
        }


        public Statements IF(string exp, Statements sent)
        {
            AppendLine($"if({exp})");
            Append(sent, 1);
            return this;
        }

        public Statements IF(string exp, Statements sent1, Statements sent2)
        {
            AppendLine($"if({exp})");
            Append(sent1, 1);
            AppendLine("else");
            Append(sent2, 1);
            return this;
        }

        public Statements FOREACH(string exp1, string exp2, Statements sent)
        {
            AppendLine($"foreach({exp1} in {exp2})");
            Append(sent, 1);
            return this;
        }


        public Statements WHILE(string exp, Statements sent)
        {
            AppendLine($"while({exp})");
            Append(sent, 1);
            return this;
        }

        public Statements RETURN(string exp)
        {
            AppendLine($"return {exp};");
            return this;
        }


    }
}

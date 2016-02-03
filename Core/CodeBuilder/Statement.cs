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
    public class Statement : CodeBlock
    {
        public Statement()
        {
        }

        public Statement COMPOUND(Statement statements)
        {
            AddBeginEnd(statements);
            return this;
        }


        public Statement IF(string exp, Statement sent)
        {
            AppendLine($"if({exp})");
            Add(sent, 1);
            return this;
        }

        public Statement IF(string exp, Statement sent1, Statement sent2)
        {
            AppendLine($"if({exp})");
            Add(sent1, 1);
            AppendLine("else");
            Add(sent2, 1);
            return this;
        }

        public Statement FOREACH(string exp1, string exp2, Statement sent)
        {
            AppendLine($"foreach({exp1} in {exp2})");
            Add(sent, 1);
            return this;
        }


        public Statement WHILE(string exp, Statement sent)
        {
            AppendLine($"while({exp})");
            Add(sent, 1);
            return this;
        }

        public Statement RETURN(string exp)
        {
            AppendLine($"return {exp};");
            return this;
        }


    }
}

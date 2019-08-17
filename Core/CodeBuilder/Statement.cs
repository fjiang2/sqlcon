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

        public Statement ASSIGN(string variable, string className, Expression[] assignments)
        {
            AppendLine($"{variable} = new {className}");
            Begin();
            foreach (Expression assign in assignments)
            {
                AppendLine($"{assign},");
            }
            End(";");
            return this;
        }

        public Statement ASSIGN(string variable, string className, Expression[] parameters, Expression[] assignments)
        {
            string args = string.Join(", ", (IEnumerable<Expression>)parameters);
            AppendLine($"{variable} = new {className}({args})");
            Begin();
            foreach (Expression assign in assignments)
            {
                AppendLine($"{assign},");
            }
            End(";");
            return this;
        }

        public Statement ASSIGN(string variable, Expression exp)
        {
            AppendLine($"{variable} = {exp};");
            return this;
        }

        public Statement IF(Expression exp, CodeBlock sent)
        {
            AppendLine($"if ({exp})");
            AddWithBeginEnd(sent);
            return this;
        }

        public Statement IF(Expression exp, CodeBlock sent1, CodeBlock sent2)
        {
            AppendLine($"if ({exp})");
            AddWithBeginEnd(sent1);
            AppendLine("else");
            AddWithBeginEnd(sent2);
            return this;
        }

        public Statement FOR(Expression exp1, Expression exp2, Expression exp3, CodeBlock sent)
        {
            AppendLine($"for ({exp1}; {exp2}; {exp3})");
            AddWithBeginEnd(sent);
            return this;
        }

        public Statement FOREACH(Expression exp1, Expression exp2, CodeBlock sent)
        {
            AppendLine($"foreach ({exp1} in {exp2})");
            AddWithBeginEnd(sent);
            return this;
        }

        public Statement WHILE(Expression exp, CodeBlock sent)
        {
            AppendLine($"while ({exp})");
            AddWithBeginEnd(sent);
            return this;
        }

        public Statement DOWHILE(CodeBlock sent, Expression exp)
        {
            AppendLine($"do");
            AddWithBeginEnd(sent);
            AppendLine($"while ({exp})");
            return this;
        }

        public Statement SWITCH(Expression exp, CodeBlock sent)
        {
            AppendLine($"switch ({exp})");
            Begin();
            Add(sent);
            End();
            return this;
        }

        public Statement CASE(Expression exp, CodeBlock sent)
        {
            AppendLine($"case {exp}:");
            Indent();
            Add(sent);
            AppendLine($"break;");
            Unindent();
            return this;
        }

        public Statement DEFAULT(CodeBlock sent)
        {
            AppendLine("default:");
            Indent();
            Add(sent);
            AppendLine("break;");
            Unindent();
            return this;
        }

        public Statement RETURN(Expression exp)
        {
            AppendLine($"return {exp};");
            return this;
        }

        public Statement BREAK()
        {
            AppendLine("break;");
            return this;
        }

        public Statement CONTINUE()
        {
            AppendLine("continue;");
            return this;
        }


    }
}

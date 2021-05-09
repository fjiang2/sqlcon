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

namespace Sys.CodeBuilder
{
    public class Statement : CodeBlock
    {
        public Statement()
        {
        }

        public static explicit operator string(Statement sent)
        {
            return sent.ToString();
        }

        public static implicit operator Statement(string sent)
        {
            var statement = new Statement();
            statement.AppendLine(sent);
            return statement;
        }

        public Statement Assign(string variable, Expression exp)
        {
            AppendLine($"{variable} = {exp};");
            return this;
        }

        public Statement If(Expression exp, Statement sent)
        {
            AppendLine($"if ({exp})");
            AddWithBeginEnd(sent);
            return this;
        }

        public Statement If(Expression exp, Statement sent1, Statement sent2)
        {
            AppendLine($"if ({exp})");
            AddWithBeginEnd(sent1);
            AppendLine("else");
            AddWithBeginEnd(sent2);
            return this;
        }

        public Statement For(Expression exp1, Expression exp2, Expression exp3, Statement sent)
        {
            AppendLine($"for ({exp1}; {exp2}; {exp3})");
            AddWithBeginEnd(sent);
            return this;
        }

        public Statement For(Expression exp, Statement sent)
        {
            AppendLine($"for ({exp})");
            AddWithBeginEnd(sent);
            return this;
        }

        public Statement Foreach(Expression exp1, Expression exp2, Statement sent)
        {
            AppendLine($"foreach ({exp1} in {exp2})");
            AddWithBeginEnd(sent);
            return this;
        }

        public Statement While(Expression exp, Statement sent)
        {
            AppendLine($"while ({exp})");
            AddWithBeginEnd(sent);
            return this;
        }

        public Statement DoWhile(Statement sent, Expression exp)
        {
            AppendLine($"do");
            AddWithBeginEnd(sent);
            AppendLine($"while ({exp})");
            return this;
        }

        public Statement Swith(Expression exp, params Statement[] sents)
        {
            AppendLine($"switch ({exp})");
            Begin();
            foreach (var sent in sents)
                Add(sent);
            End();
            return this;
        }

        public Statement Case(Expression exp)
        {
            AppendLine($"case {exp}:");
            return this;
        }

        public Statement Case(Expression exp, Statement sent)
        {
            AppendLine($"case {exp}:");
            Indent();
            Add(sent);
            AppendLine($"break;");
            Unindent();
            return this;
        }

        public Statement Default(Statement sent)
        {
            AppendLine("default:");
            Indent();
            Add(sent);
            AppendLine("break;");
            Unindent();
            return this;
        }

        public Statement Return(Expression exp)
        {
            AppendLine($"return {exp};");
            return this;
        }

        public Statement Return()
        {
            AppendLine($"return;");
            return this;
        }

        public Statement Break()
        {
            AppendLine("break;");
            return this;
        }

        public Statement Continue()
        {
            AppendLine("continue;");
            return this;
        }


    }
}

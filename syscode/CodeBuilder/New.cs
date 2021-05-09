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
    public class New : Buildable
    {
        private TypeInfo type;
        private Arguments args;
        private List<Expression> expressions { get; } = new List<Expression>();

        public ValueOutputFormat Format { get; set; } = ValueOutputFormat.SingleLine;

        public New(TypeInfo type)
          : this(type, null, null)
        {
        }

        public New(TypeInfo type, Arguments args)
            : this(type, args, null)
        {
        }

        public New(TypeInfo type, IEnumerable<Expression> expressions)
            : this(type, null, expressions)
        {
        }


        public New(TypeInfo type, Arguments args, IEnumerable<Expression> expressions)
        {
            this.type = type;
            this.args = args;

            if (expressions != null)
                this.expressions.AddRange(expressions);
        }

        /// <summary>
        /// Add into dictionary<,>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public New AddKeyValue(Expression key, Expression value)
        {
            var expr = new Expression($"[{key}] = {value}");
            return Add(expr);
        }

        /// <summary>
        /// Add into properties
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public New AddProperty(Identifier propertyName, Expression value)
        {
            var expr = new Expression(propertyName, value);
            return Add(expr);
        }

        public New Add(Expression expr)
        {
            expressions.Add(expr);
            return this;
        }

        protected override void BuildBlock(CodeBlock block)
        {
            if (expressions == null || expressions.Count() == 0)
            {
                if (args != null)
                    block.Append($"new {type}({args})");
                else
                    block.Append($"new {type}()");

                return;
            }

            if (args != null)
                block.Append($"new {type}({args})");
            else
                block.Append($"new {type}");

            OutputExpressions(block);
        }

        private void OutputExpressions(CodeBlock block)
        {
            switch (Format)
            {
                case ValueOutputFormat.SingleLine:
                    block.Append(" { ");
                    expressions.ForEach(
                         expr =>
                         {
                             block.Append(expr);
                         },
                         _ => block.Append(", ")
                         );

                    block.Append(" }");
                    break;

                default:
                    block.Begin();
                    expressions.ForEach(
                          expr =>
                          {
                              block.AppendLine();
                              block.Append(expr);
                          },
                           _ => block.Append(",")
                        );

                    block.End();
                    break;
            }
        }
      
    }
}

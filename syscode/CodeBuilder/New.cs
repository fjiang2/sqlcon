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
          : this(type, null)
        {
        }

        public New(TypeInfo type, Arguments args)
        {
            this.type = type;
            this.args = args;
        }

        public New(TypeInfo type, Arguments args, IEnumerable<Expression> expressions)
        {
            this.type = type;
            this.args = args;
            
            if (expressions != null)
                this.expressions.AddRange(expressions);
        }


        public void AddProperty(Identifier propertyName, Value value)
        {
            var property = new Expression(propertyName, value);
            AddProperty(property);
        }

        public void AddProperty(Expression property)
        {
            expressions.Add(property);
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

            WriteProperties(block);
        }

        private void WriteProperties(CodeBlock block)
        {
            switch (Format)
            {
                case ValueOutputFormat.SingleLine:
                    block.Append(" { ");
                    expressions.ForEach(
                         expr =>
                         {
                             block.Append($"{expr}");
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
                              block.Append($"{expr}");
                          },
                           _ => block.Append(",")
                        );

                    block.End();
                    break;
            }
        }
    }
}

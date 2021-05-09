using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Expression : Buildable
    {
        private object expr;

        private Expression(object expr)
        {
            this.expr = expr;
        }

        /// <summary>
        /// Argument expr is code. Use new Value(string) to create a string
        /// </summary>
        /// <param name="expr"></param>
        public Expression(string expr)
        {
            this.expr = expr;
        }

        public Expression(Identifier variable, Expression expr)
        {
            this.expr = $"{variable} = {expr}";
        }

        public Expression this[Expression index]
        {
            get => new Expression($"{this}[{index}]");
            set => this.expr = $"{this}[{index}]";
        }

        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            switch (expr)
            {
                case string value:
                    block.Append(value);
                    break;

                case Value value:
                    block.Add(value);
                    break;

                default:
                    block.Append(expr.ToString());
                    break;
            }
        }


        public static Expression ANDAND(params Expression[] exp)
        {
            return new Expression(string.Join(" && ", (IEnumerable<Expression>)exp));
        }

        public static Expression OROR(params Expression[] exp)
        {
            return new Expression(string.Join(" || ", (IEnumerable<Expression>)exp));
        }

        public static Expression NOT(Expression expr)
        {
            return new Expression($"!{expr}");
        }

        //public static explicit operator string(Expression expr)
        //{
        //    return expr.expr;
        //}


        public static implicit operator Expression(Identifier ident)
        {
            return new Expression(ident.ToString());
        }

        /// <summary>
        /// Here expr is code. Use new Value(string) to create a string
        /// </summary>
        /// <param name="expr"></param>
        public static implicit operator Expression(string expr)
        {
            return new Expression(expr);
        }

        public static implicit operator Expression(CodeString value)
        {
            return new Expression(value.ToString());
        }

        public static implicit operator Expression(Value value)
        {
            return new Expression(value.ToString());
        }

        public static implicit operator Expression(bool value)
        {
            return new Expression(value ? "true" : "false");
        }


        public static implicit operator Expression(char value)
        {
            return new Expression($"'{value}'");
        }

        public static implicit operator Expression(byte value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(sbyte value)
        {
            return new Expression(value);
        }


        public static implicit operator Expression(int value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(short value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(ushort value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(uint value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(long value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(ulong value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(float value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(DateTime value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(DBNull value)
        {
            return new Expression("DBNull");
        }

        public static implicit operator Expression(Enum value)
        {
            string type = value.GetType().Name;
            return new Expression($"{type}.{value}");
        }

        public static implicit operator Expression(TypeInfo value)
        {
            return new Expression($"typeof({value})");
        }

        public static implicit operator Expression(New value)
        {
            return new Expression(value);
        }

        public static Expression operator >(Expression exp1, Expression exp2) => new Expression($"{exp1} > {exp2}");
        public static Expression operator >=(Expression exp1, Expression exp2) => new Expression($"{exp1} >= {exp2}");
        public static Expression operator <(Expression exp1, Expression exp2) => new Expression($"{exp1} < {exp2}");
        public static Expression operator <=(Expression exp1, Expression exp2) => new Expression($"{exp1} <= {exp2}");
        public static Expression operator ==(Expression exp1, Expression exp2) => new Expression($"{exp1} == {exp2}");
        public static Expression operator !=(Expression exp1, Expression exp2) => new Expression($"{exp1} != {exp2}");

    }
}

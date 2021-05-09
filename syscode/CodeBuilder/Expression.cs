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
            this.expr = expr.ToString();
        }

        public Expression(string expr)
        {
            this.expr = expr;
        }

        public Expression(Value value)
        {
            this.expr = value;
        }

        public Expression(Identifier variable, Expression expr)
        {
            this.expr = $"{variable} = {expr}";
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


        public static implicit operator Expression(string value)
        {
            return new Expression(value);
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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Expression
    {
        private string expr;

        public Expression(string expr)
        {
            this.expr = expr;
        }

        public Expression(object expr)
        {
            this.expr = expr.ToString();
        }


        public static Expression ASSIGN(string variable, Expression exp)
        {
            return new Expression($"{variable} = {exp}");
        }

        public static Expression AND(params Expression[] exp)
        {
            return new Expression(string.Join(" && ", (IEnumerable<Expression>)exp));
        }

        public static Expression OR(params Expression[] exp)
        {
            return new Expression(string.Join(" || ", (IEnumerable<Expression>)exp));
        }

        public static Expression NOT(Expression expr)
        {
            return new Expression($"!{expr}");
        }

        public static explicit operator string(Expression expr)
        {
            return expr.expr;
        }


        public static implicit operator Expression(ident ident)
        {
            return new Expression(ident.ToString());
        }


        public static implicit operator Expression(string value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(bool value)
        {
            return new Expression(value);
        }


        public static implicit operator Expression(char value)
        {
            return new Expression(value);
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

        public override string ToString()
        {
            return expr;
        }
    }
}

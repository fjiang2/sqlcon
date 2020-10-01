using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Sys.Data.Linq
{
    class QueryTranslator : ExpressionVisitor
    {
        private StringBuilder builder;

        public QueryTranslator()
        {
            this.builder = new StringBuilder();
        }

        public string Translate(Expression expression)
        {
            this.Visit(expression);
            return this.builder.ToString();
        }

        private static Expression StripQuotes(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Quote)
            {
                expr = ((UnaryExpression)expr).Operand;
            }
            return expr;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expr)
        {
            if (expr.Method.DeclaringType == typeof(Queryable) && expr.Method.Name == "Where")
            {
                this.Visit(expr.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(expr.Arguments[1]);
                this.Visit(lambda.Body);
                return expr;
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", expr.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Not:
                    builder.Append(" NOT ");
                    this.Visit(expr.Operand);
                    break;
                case ExpressionType.Convert:
                    this.Visit(expr.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", expr.NodeType));
            }
            return expr;
        }


        protected override Expression VisitBinary(BinaryExpression expr)
        {
            builder.Append("(");
            this.Visit(expr.Left);

            switch (expr.NodeType)
            {
                case ExpressionType.And:
                    builder.Append(" AND ");
                    break;

                case ExpressionType.AndAlso:
                    builder.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    builder.Append(" OR ");
                    break;

                case ExpressionType.OrElse:
                    builder.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(expr.Right))
                    {
                        builder.Append(" IS ");
                    }
                    else
                    {
                        builder.Append(" = ");
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(expr.Right))
                    {
                        builder.Append(" IS NOT ");
                    }
                    else
                    {
                        builder.Append(" <> ");
                    }
                    break;

                case ExpressionType.LessThan:
                    builder.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    builder.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    builder.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    builder.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", expr.NodeType));

            }

            this.Visit(expr.Right);
            builder.Append(")");
            return expr;
        }

        protected override Expression VisitConstant(ConstantExpression expr)
        {
            IQueryable q = expr.Value as IQueryable;

            if (q == null && expr.Value == null)
            {
                builder.Append("NULL");
            }
            else if (q == null)
            {
                switch (Type.GetTypeCode(expr.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        builder.Append(((bool)expr.Value) ? 1 : 0);
                        break;

                    case TypeCode.String:
                        builder.Append("'");
                        builder.Append(expr.Value.ToString().Replace("'", "''"));
                        builder.Append("'");
                        break;

                    case TypeCode.DateTime:
                        builder.Append("'");
                        builder.Append(expr.Value);
                        builder.Append("'");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", expr.Value));

                    default:
                        builder.Append(expr.Value);
                        break;
                }
            }

            return expr;
        }

        protected override Expression VisitMember(MemberExpression expr)
        {
            if (expr.Expression != null && expr.Expression.NodeType == ExpressionType.Parameter)
            {
                builder.Append(expr.Member.Name);
                return expr;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", expr.Member.Name));
        }

        protected bool IsNullConstant(Expression expression)
        {
            return (expression.NodeType == ExpressionType.Constant && ((ConstantExpression)expression).Value == null);
        }
    }
}

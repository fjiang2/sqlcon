using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

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
            if (expression == null)
                return null;

            this.builder.Clear();
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
            if (expr.Method.DeclaringType != typeof(Queryable) && expr.Method.DeclaringType != typeof(Enumerable))
                throw new NotSupportedException(string.Format("The method '{0}' is not supported", expr.Method.Name));

            if (expr.Method.Name == "Where")
            {
                this.Visit(expr.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(expr.Arguments[1]);
                this.Visit(lambda.Body);
                return expr;
            }

            if (expr.Method.Name == "Contains")
            {
                this.Visit(expr.Arguments[1]);
                builder.Append(" IN ");
                this.Visit(expr.Arguments[0]);
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
                object obj = GetSqlConstant(expr.Value);
                builder.Append(obj);
            }

            return expr;
        }

        protected override Expression VisitMember(MemberExpression expr)
        {
            if (expr.Expression != null)
            {
                switch (expr.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        builder.Append(expr.Member.Name);
                        return expr;

                    case ExpressionType.Constant:
                        builder.Append(GetSqlConstant(GetValue(expr)));
                        return expr;

                    case ExpressionType.MemberAccess:
                        builder.Append(GetSqlConstant(GetValue(expr)));
                        return expr;
                }
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", expr.Member.Name));
        }

        private static object GetSqlConstant(object value)
        {
            if (value == null)
                return "NULL";

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    return ((bool)value) ? 1 : 0;

                case TypeCode.String:
                    return $"'{value.ToString().Replace("'", "''")}'";

                case TypeCode.DateTime:
                    return $"'{value}'";

                case TypeCode.Object:
                    return new SqlValue(value).ToString("N");

                default:
                    return value;
            }
        }

        protected static bool IsNullConstant(Expression expression)
        {
            return (expression.NodeType == ExpressionType.Constant && ((ConstantExpression)expression).Value == null);
        }


        private static object GetValue(Expression expression)
        {
            object GetMemberValue(MemberInfo memberInfo, object container = null)
            {
                switch (memberInfo)
                {
                    case FieldInfo fieldInfo:
                        return fieldInfo.GetValue(container);

                    case PropertyInfo propertyInfo:
                        return propertyInfo.GetValue(container);

                    default:
                        return null;
                }
            }

            switch (expression)
            {
                case ConstantExpression constantExpression:
                    return constantExpression.Value;

                case MemberExpression memberExpression when memberExpression.Expression is ConstantExpression constantExpression:
                    return GetMemberValue(memberExpression.Member, constantExpression.Value);

                case MemberExpression memberExpression when memberExpression.Expression is null: // static
                    return GetMemberValue(memberExpression.Member);

                case MemberExpression memberExpression:
                    return GetMemberValue(memberExpression.Member, GetValue(memberExpression.Expression));

                case MethodCallExpression methodCallExpression:
                    return Expression.Lambda(methodCallExpression).Compile().DynamicInvoke();

                case null:
                    return null;
            }

            throw new NotSupportedException();
        }

    }
}

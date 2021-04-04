using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Sys.Data.Linq
{
    class PropertyTranslator : ExpressionVisitor
    {
        private List<string> properties = new List<string>();

        public PropertyTranslator()
        {
        }

        public List<string> Translate(Expression expression)
        {
            this.Visit(expression);
            return this.properties;
        }

        protected override Expression VisitMember(MemberExpression expr)
        {
            if (expr.Expression != null)
            {
                switch (expr.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        properties.Add(expr.Member.Name);
                        return expr;
                }
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", expr.Member.Name));
        }

       

    }
}

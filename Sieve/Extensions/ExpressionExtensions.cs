namespace RzsSieve.Extensions
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    public static class ExpressionExtensions
    {
        public static string GetPropertyPath(this Expression expression)
        {
            Contract.Requires(expression != null);

            var path = new StringBuilder();

            var member = GetMemberExpression(expression);

            if (member == null) return string.Empty;

            do
            {
                if (path.Length > 0)
                {
                    path.Insert(0, ".");
                }

                path.Insert(0, member.Member.Name);

                member = GetMemberExpression(member.Expression);
            }
            while (member != null);

            return path.ToString();
        }

        private static MemberExpression GetMemberExpression(Expression expression)
        {
            if (expression is MemberExpression) return (MemberExpression)expression;

            if (expression is LambdaExpression)
            {
                var lambda = expression as LambdaExpression;

                if (lambda.Body is MemberExpression)
                {
                    return (MemberExpression)lambda.Body;
                }
                else if (lambda.Body is UnaryExpression)
                {
                    return ((MemberExpression)((UnaryExpression)lambda.Body).Operand);
                }
            }

            return null;
        }

        public static Expression GetPropertyOrField(Expression expression, string propertyName)
        {
            // Try to resolve as a property or field directly
            try
            {
                return Expression.PropertyOrField(expression, propertyName);
            }
            catch (ArgumentException)
            {
                // If it fails, check for interfaces
                var type = expression.Type;
                var property = type.GetInterfaces()
                    .SelectMany(i => i.GetProperties())
                    .FirstOrDefault(p => p.Name == propertyName);

                if (property != null)
                {
                    return Expression.Property(expression, property);
                }

                throw; // Rethrow if property not found
            }
        }
    }
}

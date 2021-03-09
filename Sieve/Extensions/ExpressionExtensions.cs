namespace Sieve.Extensions
{
    using System.Diagnostics.Contracts;
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
    }
}

using System;
using System.Linq;
using System.Linq.Expressions;
using RzsSieve.Services;
using RzsSieveTests.Entities;

namespace RzsSieveTests.Services
{
    public class SieveCustomFilterMethods : ISieveCustomFilterMethods
    {
        public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string[] values)
            => source.Where(p => p.LikeCount < 100 && p.CommentCount < 5);

        public IQueryable<T> IsNullOrEmpty<T>(IQueryable<T> source, string op, string[] values) where T : class
        {
            if (values.Length != 1)
            {
                throw new ArgumentException("Invalid number of arguments");
            }

            string propertyPath = values[0];
            var parameter = Expression.Parameter(typeof(T), "e");

            // Split the property path into parts
            var propertyNames = propertyPath.Split('.');

            // Build the expression to access the final property
            Expression propertyExpression = parameter;
            foreach (var propertyName in propertyNames)
            {
                propertyExpression = Expression.Property(propertyExpression, propertyName);
            }

            // Build the expression to check if the property is null or an empty string
            var nullCheck = Expression.Equal(propertyExpression, Expression.Constant(null));
            var emptyCheck = Expression.Equal(propertyExpression, Expression.Constant(string.Empty));

            // Combine the checks
            var combinedCheck = Expression.OrElse(nullCheck, emptyCheck);

            // Build the lambda expression
            var lambda = Expression.Lambda<Func<T, bool>>(combinedCheck, parameter);

            return source.Where(lambda);
        }

    }
}

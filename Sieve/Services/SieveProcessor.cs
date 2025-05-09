﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Options;
using RzsSieve.Attributes;
using RzsSieve.Exceptions;
using RzsSieve.Extensions;
using RzsSieve.Models;

namespace RzsSieve.Services
{
    public class SieveProcessor : SieveProcessor<SieveModel, FilterTerm, SortTerm>, ISieveProcessor
    {
        public SieveProcessor(IOptions<SieveOptions> options) : base(options)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods) : base(options, customSortMethods)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomFilterMethods customFilterMethods) : base(options, customFilterMethods)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods, ISieveCustomFilterMethods customFilterMethods) : base(options, customSortMethods, customFilterMethods)
        {
        }
    }

    public class SieveProcessor<TFilterTerm, TSortTerm> : SieveProcessor<SieveModel<TFilterTerm, TSortTerm>, TFilterTerm, TSortTerm>, ISieveProcessor<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        public SieveProcessor(IOptions<SieveOptions> options) : base(options)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods) : base(options, customSortMethods)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomFilterMethods customFilterMethods) : base(options, customFilterMethods)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods, ISieveCustomFilterMethods customFilterMethods) : base(options, customSortMethods, customFilterMethods)
        {
        }
    }

    public class SieveProcessor<TSieveModel, TFilterTerm, TSortTerm> : ISieveProcessor<TSieveModel, TFilterTerm, TSortTerm>
        where TSieveModel : class, ISieveModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        private const string nullFilterValue = "null";
        private readonly IOptions<SieveOptions> _options;
        private readonly ISieveCustomSortMethods _customSortMethods;
        private readonly ISieveCustomFilterMethods _customFilterMethods;
        private SievePropertyMapper _mapper = new SievePropertyMapper();

        private bool _isMapped;

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods)
        {
            _options = options;
            _customSortMethods = customSortMethods;
            _customFilterMethods = customFilterMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods)
        {
            _options = options;
            _customSortMethods = customSortMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomFilterMethods customFilterMethods)
        {
            _options = options;
            _customFilterMethods = customFilterMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options)
        {
            _options = options;
        }

        /// <summary>
        /// Apply filtering, sorting, and pagination parameters found in `model` to `source`
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="model">An instance of ISieveModel</param>
        /// <param name="source">Data source</param>
        /// <param name="dataForCustomMethods">Additional data that will be passed down to custom methods</param>
        /// <param name="applyFiltering">Should the data be filtered? Defaults to true.</param>
        /// <param name="applySorting">Should the data be sorted? Defaults to true.</param>
        /// <param name="applyPagination">Should the data be paginated? Defaults to true.</param>
        /// <returns>Returns a transformed version of `source`</returns>
        public IQueryable<TEntity> Apply<TEntity>(
            TSieveModel model,
            IQueryable<TEntity> source,
            object[] dataForCustomMethods = null,
            bool applyFiltering = true,
            bool applySorting = true,
            bool applyPagination = true)
        {
            var result = source;

            if (model == null)
            {
                return result;
            }

            try
            {
                // Filter
                if (applyFiltering)
                {
                    result = ApplyFiltering(model, result, dataForCustomMethods);
                }

                // Sort
                if (applySorting)
                {
                    result = ApplySorting(model, result, dataForCustomMethods);
                }

                // Paginate
                if (applyPagination)
                {
                    result = ApplyPagination(model, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (_options.Value.ThrowExceptions)
                {
                    if (ex is SieveException)
                    {
                        throw;
                    }

                    throw new SieveException(ex.Message, ex);
                }
                else
                {
                    return result;
                }
            }
        }

        private IQueryable<TEntity> ApplyFiltering<TEntity>(
            TSieveModel model,
            IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            if (model?.GetFiltersParsed() == null)
            {
                return result;
            }

            Expression outerExpression = null;
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            foreach (var filterTerm in model.GetFiltersParsed())
            {
                Expression innerExpression = null;
                foreach (var filterTermName in filterTerm.Names)
                {
                    var (fullPropertyName, property) = GetSieveProperty<TEntity>(false, true, filterTermName);
                    if (property != null)
                    {
                        Expression propertyValue = parameter;
                        Expression nullCheck = null;
                        var names = fullPropertyName.Split('.');
                        for (var i = 0; i < names.Length; i++)
                        {
                            propertyValue = GetPropertyOrField(propertyValue, names[i]);

                            if (i != names.Length - 1 && propertyValue.Type.IsNullable() && filterTerm.OperatorIsNegated)
                            {
                                nullCheck = GenerateFilterNullCheckExpression(propertyValue, nullCheck);
                            }
                        }

                        if (filterTerm.Values == null) continue;

                        var converter = TypeDescriptor.GetConverter(property.PropertyType);
                        foreach (var filterTermValue in filterTerm.Values)
                        {
                            var isFilterTermValueNull = filterTermValue.ToLower() == nullFilterValue;
                            var filterValue = isFilterTermValueNull
                                ? Expression.Constant(null, property.PropertyType)
                                : ConvertStringValueToConstantExpression(filterTermValue, property, converter);

                            if (filterTerm.OperatorIsCaseInsensitive)
                            {
                                propertyValue = Expression.Call(propertyValue,
                                    typeof(string).GetMethods()
                                    .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));

                                filterValue = Expression.Call(filterValue,
                                    typeof(string).GetMethods()
                                    .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));
                            }

                            var expression = GetExpression(filterTerm, filterValue, propertyValue);

                            if (filterTerm.OperatorIsNegated)
                            {
                                expression = Expression.Not(expression);
                            }

                            var filterValueNullCheck = !isFilterTermValueNull && propertyValue.Type.IsNullable()
                                ? GenerateFilterNullCheckExpression(propertyValue, nullCheck)
                                : nullCheck;

                            if (filterValueNullCheck != null)
                            {
                                expression = Expression.AndAlso(filterValueNullCheck, expression);
                            }

                            if (innerExpression == null)
                            {
                                innerExpression = expression;
                            }
                            else
                            {
                                innerExpression = Expression.Or(innerExpression, expression);
                            }
                        }
                    }
                    else
                    {
                        result = ApplyCustomMethod(result, filterTermName, _customFilterMethods,
                            new object[] {
                                            result,
                                            filterTerm.Operator,
                                            filterTerm.Values
                            }, dataForCustomMethods);

                    }
                }
                if (outerExpression == null)
                {
                    outerExpression = innerExpression;
                    continue;
                }
                if (innerExpression == null)
                {
                    continue;
                }
                outerExpression = Expression.And(outerExpression, innerExpression);
            }
            return outerExpression == null
                ? result
                : result.Where(Expression.Lambda<Func<TEntity, bool>>(outerExpression, parameter));
        }

        private static Expression GetPropertyOrField(Expression expression, string propertyName)
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

        private static Expression GenerateFilterNullCheckExpression(Expression propertyValue, Expression nullCheckExpression)
        {
            return nullCheckExpression == null
                ? Expression.NotEqual(propertyValue, Expression.Default(propertyValue.Type))
                : Expression.AndAlso(nullCheckExpression, Expression.NotEqual(propertyValue, Expression.Default(propertyValue.Type)));
        }

        private Expression ConvertStringValueToConstantExpression(string value, PropertyInfo property, TypeConverter converter)
        {
            dynamic constantVal = converter.CanConvertFrom(typeof(string))
                ? converter.ConvertFrom(value)
                : Convert.ChangeType(value, property.PropertyType);

            return GetClosureOverConstant(constantVal, property.PropertyType);
        }

        static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static Expression GetExpression(TFilterTerm filterTerm, dynamic filterValue, dynamic propertyValue)
        {
            var type = filterValue.Type;
            var underlyingType = Nullable.GetUnderlyingType((Type)filterValue.Type) ?? (Type)filterValue.Type;

            if (new[] { 
                    FilterOperator.DateEquals, 
                    FilterOperator.DateGreaterThan, 
                    FilterOperator.DateLessThan,
                    FilterOperator.DateGreaterThanOrEqualTo,
                    FilterOperator.DateLessThanOrEqualTo
                }.Contains(filterTerm.OperatorParsed))
            {
                if (IsNullableType(type))
                {
                    filterValue = Expression.Convert(filterValue, underlyingType);
                }
                if (IsNullableType(type))
                {
                    propertyValue = Expression.Convert(propertyValue, underlyingType);
                }
            }

            switch (filterTerm.OperatorParsed)
            {
                case FilterOperator.Equals:
                    return Expression.Equal(propertyValue, filterValue);
                case FilterOperator.NotEquals:
                    return Expression.NotEqual(propertyValue, filterValue);
                case FilterOperator.GreaterThan:
                    return Expression.GreaterThan(propertyValue, filterValue);
                case FilterOperator.LessThan:
                    return Expression.LessThan(propertyValue, filterValue);
                case FilterOperator.GreaterThanOrEqualTo:
                    return Expression.GreaterThanOrEqual(propertyValue, filterValue);
                case FilterOperator.LessThanOrEqualTo:
                    return Expression.LessThanOrEqual(propertyValue, filterValue);
                case FilterOperator.Contains:
                    return Expression.Call(propertyValue,
                        typeof(string).GetMethods()
                        .First(m => m.Name == "Contains" && m.GetParameters().Length == 1),
                        filterValue);
                case FilterOperator.StartsWith:
                    return Expression.Call(propertyValue,
                        typeof(string).GetMethods()
                        .First(m => m.Name == "StartsWith" && m.GetParameters().Length == 1),
                        filterValue);
                case FilterOperator.DateEquals:
                    var nextDayExpression = AddDaysExpression(filterValue, underlyingType);
                    return Expression.And(
                        Expression.GreaterThanOrEqual(propertyValue, filterValue),
                        Expression.LessThan(propertyValue, nextDayExpression)
                    );
                case FilterOperator.DateGreaterThanOrEqualTo:
                    return Expression.GreaterThanOrEqual(propertyValue, filterValue);
                case FilterOperator.DateLessThanOrEqualTo:
                    nextDayExpression = AddDaysExpression(filterValue, underlyingType);
                    return Expression.LessThan(propertyValue, nextDayExpression);
                case FilterOperator.DateGreaterThan:
                    nextDayExpression = AddDaysExpression(filterValue, underlyingType);
                    return Expression.GreaterThanOrEqual(propertyValue, nextDayExpression);
                case FilterOperator.DateLessThan:
                    return Expression.LessThan(propertyValue, filterValue);
                default:
                    return Expression.Equal(propertyValue, filterValue);
            }
        }

        private static MethodCallExpression AddDaysExpression(Expression dateExpression, Type type)
        {
            var addDaysMethod = type
                .GetMethods()
                .First(m => m.Name == "AddDays" && m.GetParameters().Length == 1);

            var argument = type == typeof(DateOnly)
                ? Expression.Constant(1, typeof(int))
                : Expression.Constant(1d, typeof(double));

            return Expression.Call(dateExpression, addDaysMethod, argument);
        }

        // Workaround to ensure that the filter value gets passed as a parameter in generated SQL from EF Core
        // See https://github.com/aspnet/EntityFrameworkCore/issues/3361
        // Expression.Constant passed the target type to allow Nullable comparison
        // See http://bradwilson.typepad.com/blog/2008/07/creating-nullab.html
        private Expression GetClosureOverConstant<T>(T constant, Type targetType)
        {
            return Expression.Constant(constant, targetType);
        }

        private IQueryable<TEntity> ApplySorting<TEntity>(
            TSieveModel model,
            IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            if (model?.GetSortsParsed() == null)
            {
                return result;
            }

            var useThenBy = false;
            foreach (var sortTerm in model.GetSortsParsed())
            {
                var (fullName, property) = GetSieveProperty<TEntity>(true, false, sortTerm.Name);

                if (property != null)
                {
                    result = result.OrderByDynamic(fullName, property, sortTerm.Descending, useThenBy);
                }
                else
                {
                    result = ApplyCustomMethod(result, sortTerm.Name, _customSortMethods,
                        new object[]
                        {
                        result,
                        useThenBy,
                        sortTerm.Descending
                        }, dataForCustomMethods);
                }
                useThenBy = true;
            }

            return result;
        }

        private IQueryable<TEntity> ApplyPagination<TEntity>(
            TSieveModel model,
            IQueryable<TEntity> result)
        {
            var page = model?.Page ?? 1;
            var pageSize = model?.PageSize ?? _options.Value.DefaultPageSize;
            var maxPageSize = _options.Value.MaxPageSize > 0 ? _options.Value.MaxPageSize : pageSize;

            if (pageSize > 0)
            {
                model.PrePaginationCount = result.Count();

                result = result.Skip((page - 1) * pageSize);
                result = result.Take(Math.Min(pageSize, maxPageSize));
            }

            return result;
        }

        protected virtual SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            return mapper;
        }

        private (string, PropertyInfo) GetSieveProperty<TEntity>(
            bool canSortRequired,
            bool canFilterRequired,
            string name)
        {
            if (!_isMapped)
            {
                _mapper = MapProperties(_mapper);

                _isMapped = true;
            }

            var property = _mapper.FindProperty<TEntity>(canSortRequired, canFilterRequired, name, _options.Value.CaseSensitive);
            if (property.Item1 == null)
            {
                var prop = FindPropertyBySieveAttribute<TEntity>(canSortRequired, canFilterRequired, name, _options.Value.CaseSensitive);
                return (prop?.Name, prop);
            }
            return property;

        }

        private PropertyInfo FindPropertyBySieveAttribute<TEntity>(
            bool canSortRequired,
            bool canFilterRequired,
            string name,
            bool isCaseSensitive)
        {
            return Array.Find(typeof(TEntity).GetProperties(), p =>
            {
                return p.GetCustomAttribute(typeof(SieveAttribute)) is SieveAttribute sieveAttribute
                && (!canSortRequired || sieveAttribute.CanSort)
                && (!canFilterRequired || sieveAttribute.CanFilter)
                && (sieveAttribute.Name ?? p.Name).Equals(name, isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
            });
        }

        private IQueryable<TEntity> ApplyCustomMethod<TEntity>(IQueryable<TEntity> result, string name, object parent, object[] parameters, object[] optionalParameters = null)
        {
            var customMethod = parent?.GetType()
                .GetMethodExt(name,
                _options.Value.CaseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance,
                typeof(IQueryable<TEntity>));


            if (customMethod == null)
            {
                // Find generic methods `public IQueryable<T> Filter<T>(IQueryable<T> source, ...)`
                var genericCustomMethod = parent?.GetType()
                .GetMethodExt(name,
                _options.Value.CaseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance,
                typeof(IQueryable<>));

                if (genericCustomMethod != null &&
                    genericCustomMethod.ReturnType.IsGenericType &&
                    genericCustomMethod.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                {
                    var genericBaseType = genericCustomMethod.ReturnType.GenericTypeArguments[0];
                    var constraints = genericBaseType.GetGenericParameterConstraints();
                    if (constraints == null || constraints.Length == 0 || constraints.All((t) => t.IsAssignableFrom(typeof(TEntity))))
                    {
                        customMethod = genericCustomMethod.MakeGenericMethod(typeof(TEntity));
                    }
                }
            }

            if (customMethod != null)
            {
                try
                {
                    result = customMethod.Invoke(parent, parameters)
                        as IQueryable<TEntity>;
                }
                catch (TargetParameterCountException)
                {
                    if (optionalParameters != null)
                    {
                        result = customMethod.Invoke(parent, parameters.Concat(optionalParameters).ToArray())
                            as IQueryable<TEntity>;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                var incompatibleCustomMethods = parent?
                                                    .GetType()
                                                    .GetMethods
                                                    (
                                                        _options.Value.CaseSensitive
                                                            ? BindingFlags.Default
                                                            : BindingFlags.IgnoreCase | BindingFlags.Public |
                                                              BindingFlags.Instance
                                                    )
                                                    .Where(method => string.Equals(method.Name, name,
                                                        _options.Value.CaseSensitive
                                                            ? StringComparison.InvariantCulture
                                                            : StringComparison.InvariantCultureIgnoreCase))
                                                    .ToList()
                                                ??
                                                new List<MethodInfo>();

                if (incompatibleCustomMethods.Any())
                {
                    var incompatibles =
                        from incompatibleCustomMethod in incompatibleCustomMethods
                        let expected = typeof(IQueryable<TEntity>)
                        let actual = incompatibleCustomMethod.ReturnType
                        select new SieveIncompatibleMethodException(name, expected, actual,
                            $"{name} failed. Expected a custom method for type {expected} but only found for type {actual}");

                    var aggregate = new AggregateException(incompatibles);

                    throw new SieveIncompatibleMethodException(aggregate.Message, aggregate);
                }
                else
                {
                    throw new SieveMethodNotFoundException(name, $"{name} not found.");
                }
            }

            return result;
        }
    }
}

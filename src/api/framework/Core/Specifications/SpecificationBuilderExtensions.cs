using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Globalization;
using Ardalis.Specification;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Core.Paging;

namespace FSH.Framework.Core.Specifications;

// See https://github.com/ardalis/Specification/issues/53
public static class SpecificationBuilderExtensions
{
    public static ISpecificationBuilder<T> SearchBy<T>(this ISpecificationBuilder<T> query, BaseFilter filter) =>
        query
            .SearchByKeyword(filter.Keyword)
            .AdvancedSearch(filter.AdvancedSearch)
            .AdvancedFilter(filter.AdvancedFilter);

    public static ISpecificationBuilder<T> PaginateBy<T>(this ISpecificationBuilder<T> query, PaginationFilter filter)
    {
        if (filter.PageNumber <= 0)
        {
            filter.PageNumber = 1;
        }

        if (filter.PageSize <= 0)
        {
            filter.PageSize = 10;
        }

        if (filter.PageNumber > 1)
        {
            query = query.Skip((filter.PageNumber - 1) * filter.PageSize);
        }

        return query
            .Take(filter.PageSize)
            .OrderBy(filter.OrderBy);
    }

    public static ISpecificationBuilder<T> SearchByKeyword<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        string? keyword) =>
        string.IsNullOrEmpty(keyword) 
            ? specificationBuilder 
            : specificationBuilder.AdvancedSearch(new Search { Keyword = keyword });

    public static ISpecificationBuilder<T> AdvancedSearch<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Search? search)
    {
        if (!string.IsNullOrEmpty(search?.Keyword))
        {
            if (search.Fields?.Count > 0)
            {
                // search selected fields (can contain deeper nested fields)
                foreach (string field in search.Fields)
                {
                    var paramExpr = Expression.Parameter(typeof(T));
                    MemberExpression propertyExpr = GetPropertyExpression(field, paramExpr);

                    specificationBuilder.AddSearchPropertyByKeyword(propertyExpr, paramExpr, search.Keyword);
                }
            }
            else
            {
                // search all fields (only first level)
                foreach (var property in typeof(T).GetProperties()
                    .Where(prop => (Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType) is { } propertyType
                        && !propertyType.IsEnum
                        && Type.GetTypeCode(propertyType) != TypeCode.Object))
                {
                    var paramExpr = Expression.Parameter(typeof(T));
                    var propertyExpr = Expression.Property(paramExpr, property);

                    specificationBuilder.AddSearchPropertyByKeyword(propertyExpr, paramExpr, search.Keyword);
                }
            }
        }

        return specificationBuilder;
    }

    private static void AddSearchPropertyByKeyword<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Expression propertyExpr,
        ParameterExpression paramExpr,
        string keyword,
        string operatorSearch = FilterOperator.CONTAINS)
    {
        if (propertyExpr is not MemberExpression memberExpr || memberExpr.Member is not PropertyInfo property)
        {
            throw new ArgumentException("propertyExpr must be a property expression.", nameof(propertyExpr));
        }

        string searchTerm = operatorSearch switch
        {
            FilterOperator.STARTSWITH => $"{keyword.ToLower(CultureInfo.InvariantCulture)}%",
            FilterOperator.ENDSWITH => $"%{keyword.ToLower(CultureInfo.InvariantCulture)}",
            FilterOperator.CONTAINS => $"%{keyword.ToLower(CultureInfo.InvariantCulture)}%",
            _ => throw new ArgumentException("operatorSearch is not valid.", nameof(operatorSearch))
        };

        // Generate lambda [ x => x.Property ] for string properties
        // or [ x => ((object)x.Property) == null ? null : x.Property.ToString() ] for other properties
        Expression selectorExpr =
            property.PropertyType == typeof(string)
                ? propertyExpr
                : Expression.Condition(
                    Expression.Equal(Expression.Convert(propertyExpr, typeof(object)), Expression.Constant(null, typeof(object))),
                    Expression.Constant(null, typeof(string)),
                    Expression.Call(propertyExpr, "ToString", null, null));

        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        Expression callToLowerMethod = Expression.Call(selectorExpr, toLowerMethod!);

        var selector = Expression.Lambda<Func<T, string>>(callToLowerMethod, paramExpr);

        ((List<SearchExpressionInfo<T>>)specificationBuilder.Specification.SearchCriterias)
            .Add(new SearchExpressionInfo<T>(
                (Expression<Func<T, string?>>)(object)selector, 
                searchTerm, 
                1));
    }

    public static ISpecificationBuilder<T> AdvancedFilter<T>(
        this ISpecificationBuilder<T> query,
        Filter? filter)
    {
        if (filter == null) return query;

        if (filter.Logic?.ToLower() == "and")
        {
            if (filter.Filters != null)
            {
                foreach (var filterItem in filter.Filters)
                {
                    query = query.AdvancedFilter(filterItem);
                }
            }
        }
        else if (filter.Logic?.ToLower() == "or")
        {
            query = query.AdvancedFilter(filter.Filters, FilterLogic.OR);
        }
        else if (filter.Logic?.ToLower() == "not")
        {
            query = query.AdvancedFilter(filter.Filters, FilterLogic.NOT);
        }
        else if (filter.Logic?.ToLower() == "nor")
        {
            query = query.AdvancedFilter(filter.Filters, FilterLogic.NOR);
        }
        else if (filter.Filters is not null)
        {
            query = query.AdvancedFilter(filter.Filters, FilterLogic.AND);
        }
        else
        {
            // Single filter case
            var parameter = Expression.Parameter(typeof(T));
            var filterExpr = CreateFilterExpression<T>(filter, parameter);
            if (filterExpr != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(filterExpr, parameter);
                query = query.Where(lambda);
            }
        }

        return query;
    }
    
    public static ISpecificationBuilder<T> AdvancedFilter<T>(
        this ISpecificationBuilder<T> query,
        IEnumerable<Filter>? filters,
        string logic = FilterLogic.AND)
    {
        if (filters?.Any() != true) return query;
        
        var parameter = Expression.Parameter(typeof(T));
        Expression? combinedExpression = null;
        
        foreach (var filter in filters)
        {
            var filterExpr = CreateFilterExpression<T>(filter, parameter);
            if (filterExpr == null) continue;
                
            combinedExpression = combinedExpression == null 
                ? filterExpr 
                : CombineFilter<T>(logic, combinedExpression, filterExpr);
        }

        if (combinedExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    private static Expression? CreateFilterExpression<T>(Filter filter, ParameterExpression parameter)
    {
        if (filter == null) return null;
        
        if (!string.IsNullOrEmpty(filter.Logic))
        {
            if (filter.Filters == null) 
                throw new CustomException("The Filters attribute is required when declaring a logic");
                
            return CreateFilterExpression<T>(filter.Logic, filter.Filters, parameter);
        }
        else
        {
            var filterValid = GetValidFilter(filter);
            return CreateFilterExpression(filterValid.Field!, filterValid.Operator!, filterValid.Value, parameter);
        }
    }

    private static Expression CreateFilterExpression<T>(
        string logic,
        IEnumerable<Filter> filters,
        ParameterExpression parameter)
    {
        Expression? filterExpression = null;

        foreach (var filter in filters)
        {
            var filterExpr = CreateFilterExpression<T>(filter, parameter);
            if (filterExpr != null)
            {
                filterExpression = filterExpression == null 
                    ? filterExpr 
                    : CombineFilter<T>(logic, filterExpression, filterExpr);
            }
        }

        return filterExpression ?? Expression.Constant(true);
    }

    private static Expression CreateFilterExpression(
        string field,
        string filterOperator,
        object? value,
        ParameterExpression parameter)
    {
        var propertyExpresion = GetPropertyExpression(field, parameter);
        var valueExpresion = GeValuetExpression(field, value, propertyExpresion.Type);
        return CreateFilterExpression(propertyExpresion, valueExpresion, filterOperator);
    }

    private static Expression CreateFilterExpression(
        Expression memberExpression,
        Expression constantExpression,
        string filterOperator)
    {
        if (memberExpression.Type == typeof(string))
        {
            constantExpression = Expression.Call(constantExpression, "ToLower", null);
            memberExpression = Expression.Call(memberExpression, "ToLower", null);
        }

        return filterOperator switch
        {
            FilterOperator.EQ => Expression.Equal(memberExpression, constantExpression),
            FilterOperator.NEQ => Expression.NotEqual(memberExpression, constantExpression),
            FilterOperator.LT => Expression.LessThan(memberExpression, constantExpression),
            FilterOperator.LTE => Expression.LessThanOrEqual(memberExpression, constantExpression),
            FilterOperator.GT => Expression.GreaterThan(memberExpression, constantExpression),
            FilterOperator.GTE => Expression.GreaterThanOrEqual(memberExpression, constantExpression),
            FilterOperator.CONTAINS => Expression.Call(memberExpression, "Contains", null, constantExpression),
            FilterOperator.STARTSWITH => Expression.Call(memberExpression, "StartsWith", null, constantExpression),
            FilterOperator.ENDSWITH => Expression.Call(memberExpression, "EndsWith", null, constantExpression),
            _ => throw new CustomException("Filter Operator is not valid."),
        };
    }

    private static Expression CombineFilter<T>(
        string filterOperator,
        Expression bExpresionBase,
        Expression bExpresion)
    {
        return filterOperator switch
        {
            FilterLogic.AND => Expression.AndAlso(bExpresionBase, bExpresion),
            FilterLogic.OR => Expression.OrElse(bExpresionBase, bExpresion),
            FilterLogic.NOT => Expression.Not(bExpresionBase),
            FilterLogic.NOR => Expression.Not(Expression.OrElse(bExpresionBase, bExpresion)),
            _ => throw new ArgumentException("Operador lógico no soportado")
        };
    }

    private static MemberExpression GetPropertyExpression(
        string propertyName,
        ParameterExpression parameter)
    {
        Expression propertyExpression = parameter;
        foreach (string member in propertyName.Split('.'))
        {
            propertyExpression = Expression.PropertyOrField(propertyExpression, member);
        }

        return (MemberExpression)propertyExpression;
    }

    private static string GetStringFromJsonElement(object value)
        => ((JsonElement)value).GetString()!;

    private static ConstantExpression GeValuetExpression(
        string field,
        object? value,
        Type propertyType)
    {
        if (value == null) return Expression.Constant(null, propertyType);

        if (propertyType.IsEnum)
        {
            string? stringEnum = GetStringFromJsonElement(value);

            if (!Enum.TryParse(propertyType, stringEnum, true, out object? valueparsed)) throw new CustomException(string.Format("Value {0} is not valid for {1}", value, field));

            return Expression.Constant(valueparsed, propertyType);
        }

        if (propertyType == typeof(Guid))
        {
            string? stringGuid = GetStringFromJsonElement(value);

            if (!Guid.TryParse(stringGuid, out Guid valueparsed)) throw new CustomException(string.Format("Value {0} is not valid for {1}", value, field));

            return Expression.Constant(valueparsed, propertyType);
        }

        if (propertyType == typeof(string))
        {
            string? text = GetStringFromJsonElement(value);

            return Expression.Constant(text, propertyType);
        }

        if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
        {
            string? text = GetStringFromJsonElement(value);
            return Expression.Constant(ChangeType(text, propertyType), propertyType);
        }

        return Expression.Constant(ChangeType(((JsonElement)value).GetRawText(), propertyType), propertyType);
    }

    public static dynamic? ChangeType(object value, Type conversion)
    {
        var t = conversion;

        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            if (value == null)
            {
                return null;
            }

            t = Nullable.GetUnderlyingType(t);
        }

        return Convert.ChangeType(value, t!);
    }

    private static Filter GetValidFilter(Filter filter)
    {
        if (string.IsNullOrEmpty(filter.Field)) throw new CustomException("The field attribute is required when declaring a filter");
        if (string.IsNullOrEmpty(filter.Operator)) throw new CustomException("The Operator attribute is required when declaring a filter");
        return filter;
    }

    public static ISpecificationBuilder<T> OrderBy<T>(
        this ISpecificationBuilder<T> query,
        string[]? orderByFields)
    {
        if (orderByFields is not null)
        {
            foreach (var field in orderByFields)
            {
                var orderBy = field.Trim().Split(' ');
                var orderByField = orderBy[0];
                if (orderBy.Length > 1 && orderBy[1].ToUpper() == "DESC")
                {
                    query = query.OrderBy(new[] { $"{orderByField} DESC" });
                }
                else
                {
                    query = query.OrderBy(new[] { orderByField });
                }
            }
        }

        return query;
    }

    private static Dictionary<string, OrderTypeEnum> ParseOrderBy(string[] orderByFields) =>
        new(orderByFields.Select((orderByfield, index) =>
        {
            string[] fieldParts = orderByfield.Split(' ');
            string field = fieldParts[0];
            bool descending = fieldParts.Length > 1 && fieldParts[1].StartsWith("Desc", StringComparison.OrdinalIgnoreCase);

            OrderTypeEnum orderBy;
            if (index == 0)
            {
                orderBy = descending ? OrderTypeEnum.OrderByDescending : OrderTypeEnum.OrderBy;
            }
            else
            {
                orderBy = descending ? OrderTypeEnum.ThenByDescending : OrderTypeEnum.ThenBy;
            }

            return new KeyValuePair<string, OrderTypeEnum>(field, orderBy);
        }));
}

public static class FilterLogic
{
    public const string AND = "and";
    public const string OR = "or";
    public const string NOT = "not";
    public const string NOR = "nor";
}

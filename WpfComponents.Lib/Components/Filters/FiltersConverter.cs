using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WpfComponents.Lib.Components.Filters.Data;

namespace WpfComponents.Lib.Components.Filters
{
    public class FiltersConverter
    {
        public static Expression<Func<TTypeCible, bool>> GetExpression<TTypeCible>(IList<Data.Filter> filters)
        {
            if (filters == null || filters.Count == 0)
                return (_) => true;

            ParameterExpression parameterExpression = Expression.Parameter(typeof(TTypeCible), "racine");
            Expression expression = FiltresToExpression(parameterExpression, filters);
            if (expression == null)
                return (_) => true;

            return Expression.Lambda<Func<TTypeCible, bool>>(expression, parameterExpression);
        }

        // Permet de convertir un groupe de filtre en une expression
        private static Expression FiltresToExpression(Expression parent, IList<Data.Filter> filters, int depth = 0)
        {
            Expression expression = null;

            foreach (var filter in filters)
            {
                Expression filterExpression = null;
                Expression targetProp;

                // Gérer une propriété null (dans le cas d'un groupe logique) et prendre la propriété du parent
                if (filter.Property == null)
                    targetProp = parent;
                else
                    targetProp = Expression.Property(parent, filter.Property);

                // Gestion des listes
                if (filter.Expression is FilterGroup groupList && Utils.IsList(targetProp.Type))
                    filterExpression = ListeToExpression(targetProp, groupList, depth);
                // Gestion groupes
                else if (filter.Expression is FilterGroup groupObj)
                    filterExpression = ObjetToExpression(targetProp, groupObj, depth);
                else if (filter.Expression is FilterValue filterValue)
                    filterExpression = ValueToExpression(targetProp, filterValue);

                // Conjonction
                if (expression == null)
                    expression = filterExpression;
                else
                {
                    switch (filter.Conjunction)
                    {
                        case EnumConjunctionFilter.And:
                            expression = Expression.AndAlso(expression, filterExpression);
                            break;
                        case EnumConjunctionFilter.Or:
                            expression = Expression.OrElse(expression, filterExpression);
                            break;
                    }
                }
            }
            return expression;
        }

        private static Expression ObjetToExpression(Expression targetProp, FilterGroup objectProp, int depth)
        { return FiltresToExpression(targetProp, objectProp.Childrens, depth + 1); }

        // Oui c'est de l'anglais, parce que IA
        private static Expression ListeToExpression(Expression targetProp, FilterGroup groupList, int depth)
        {
            // Create a new expression on pPropCible with a call to the method Where of the class Enumerable and then call the method Any
            var whereMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == nameof(Enumerable.Where) && m.GetParameters().Length == 2);
            var anyMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 1);

            // Get pPropCible property name
            string paramName;
            if (targetProp is ParameterExpression paramExpression)
                paramName = paramExpression.Name;
            else if (targetProp is MemberExpression memberExpression)
                paramName = memberExpression.Member.Name;
            else
                throw new NotImplementedException($"The type [{targetProp.GetType()}] is not handled.");

            // Create the lambda expression for the Where method
            var param = Expression.Parameter(targetProp.Type.GetGenericArguments().First(), $"{paramName}_{depth}");
            var lambda = Expression.Lambda(FiltresToExpression(param, groupList.Childrens, depth + 1), param);

            // Allow null value for pPropCible (final result will be a bool?) 
            if (targetProp.Type.GetGenericArguments().First().IsValueType)
                targetProp = Expression.Coalesce(
                    targetProp,
                    Expression.Constant(Activator.CreateInstance(targetProp.Type.GetGenericArguments().First())));

            // Call the Where method
            var whereCall = Expression.Call(
                whereMethod.MakeGenericMethod(targetProp.Type.GetGenericArguments().First()),
                targetProp,
                lambda);

            // Call the Any method
            var anyCall = Expression.Call(
                anyMethod.MakeGenericMethod(targetProp.Type.GetGenericArguments().First()),
                whereCall);

            return anyCall;
        }

        private static Expression ValueToExpression(Expression targetProp, FilterValue valueFilter)
        {
            var constant = Expression.Constant(Convert(valueFilter.Value ?? "", targetProp.Type));

            Expression expression = null;
            switch (valueFilter.Operator)
            {
                case EnumOperatorFilter.EqualsTo:
                case EnumOperatorFilter.NotEqualsTo:
                    expression = Expression.Equal(targetProp, constant);
                    break;
                case EnumOperatorFilter.GreaterThan:
                    expression = Expression.GreaterThan(targetProp, constant);
                    break;
                case EnumOperatorFilter.GreaterThanOrEqual:
                    expression = Expression.GreaterThanOrEqual(targetProp, constant);
                    break;
                case EnumOperatorFilter.LesserThan:
                    expression = Expression.LessThan(targetProp, constant);
                    break;
                case EnumOperatorFilter.LesserThanOrEqual:
                    expression = Expression.LessThanOrEqual(targetProp, constant);
                    break;
                // String specific
                case EnumOperatorFilter.Contains:
                case EnumOperatorFilter.NotContains:
                    {
                        MethodInfo method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
                        expression = Expression.Call(targetProp, method, constant);
                        break;
                    }
                case EnumOperatorFilter.StartsWith:
                case EnumOperatorFilter.NotStartsWith:
                    {
                        MethodInfo method = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
                        expression = Expression.Call(targetProp, method, constant);
                        break;
                    }
                case EnumOperatorFilter.EndsWith:
                case EnumOperatorFilter.NotEndsWith:
                    {
                        MethodInfo method = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
                        expression = Expression.Call(targetProp, method, constant);
                        break;
                    }
                case EnumOperatorFilter.Between:
                case EnumOperatorFilter.NotBetween:
                default:
                    throw new NotImplementedException(
                        $"Operator [{valueFilter.Operator}] is not handled.");
            }

            // HACK : handle Not operator
            return (valueFilter.Operator.ToString().StartsWith("Not")) ? Expression.Not(expression) : expression;
        }

        private static object Convert(object value, Type returnType)
        {
            try
            {
                if (value == null)
                    return null;

                if (value.GetType().IsEnum && !returnType.IsEnum)
                    value = (int)value;

                if (value.GetType() == returnType)
                    return value;

                var converter = System.ComponentModel.TypeDescriptor.GetConverter(returnType);
                return converter.ConvertFrom(null, System.Globalization.CultureInfo.CurrentCulture, value.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"The value [{value}] cannot be converted to [{returnType}]", ex);
            }
        }
    }
}

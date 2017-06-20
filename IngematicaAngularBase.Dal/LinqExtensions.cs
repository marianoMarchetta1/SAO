using IngematicaAngularBase.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IngematicaAngularBase.Dal
{
    public static class LinqExtensions
    {

        private static int MaxQueryResults = 1000;// int.Parse(ConfigurationManager.AppSettings["MaxQueryResults"]);
        //private static int DefaultQueryResults = 20;//int.Parse(ConfigurationManager.AppSettings["DefaultQueryResults"]);

        private static MethodInfo methodOrderBy = typeof(Queryable).GetMethods().First(m => m.Name == "OrderBy");
        private static MethodInfo methodOrderByDescending = typeof(Queryable).GetMethods().First(m => m.Name == "OrderByDescending");
        private static MethodInfo methodThenBy = typeof(Queryable).GetMethods().First(m => m.Name == "ThenBy");
        private static MethodInfo methodThenByDescending = typeof(Queryable).GetMethods().First(m => m.Name == "ThenByDescending");


        private static void ValidateQueryObject(QueryObject queryObject)
        {
            if (queryObject.Take < 1 || queryObject.Take > MaxQueryResults)
                throw new ArgumentException("The Take property cant be > 0 and < to MaxQueryResults");
            if (queryObject.Skip < 0)
                throw new ArgumentException("The Skip property cant be >= 0");
        }

        public static QueryResult<T> ToQueryResult<T>(this IQueryable<T> source, QueryObject queryObject) where T : class
        {

            ValidateQueryObject(queryObject);

            QueryResult<T> result = new QueryResult<T>();
            IQueryable<T> query = source;
           
                try
                {
                    if(queryObject.Order.Any())
                        query = OrderBy<T>(source, queryObject);
                    
                }
                catch (Exception ex)
                {
                    throw new Exception("Please verify if the field names of QueryObject exists. See inner exception for more details", ex);
                }

            result.TotalCount = query.Count();
            result.Data = query.Skip(queryObject.Skip).Take(queryObject.Take).ToList();
            return result;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, QueryObject query)
        {
            IOrderedQueryable<T> result = null;

            for (int i = 0; i < query.Order.Count(); i++)
			{
                if (query.Order[i].Descending)
                    if(i > 0)
                        result = DynamicOrderBy<T>(result, query.Order[i].Property, methodThenByDescending);
                    else
                        result = DynamicOrderBy<T>(source, query.Order[i].Property, methodOrderByDescending);
                else
                    if (i > 0)
                        result = DynamicOrderBy<T>(result, query.Order[i].Property, methodThenBy);
                    else
                        result = DynamicOrderBy<T>(source, query.Order[i].Property, methodOrderBy);
			}

            return result;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return DynamicOrderBy<T>(source, property, methodOrderBy);
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return DynamicOrderBy<T>(source, property, methodOrderByDescending);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IQueryable<T> source, string property)
        {
            return DynamicOrderBy<T>(source, property, methodThenBy);
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IQueryable<T> source, string property)
        {
            return DynamicOrderBy<T>(source, property, methodThenByDescending);
        }

        public static IOrderedQueryable<T> DynamicOrderBy<T>(IQueryable<T> source, string property, MethodInfo methodInfo)
        {
            var expr = source.Expression;
            var p = Expression.Parameter(typeof(T), "x");
            var propInfo = typeof(T).GetProperty(property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var sortExpr = Expression.Lambda(Expression.Property(p, propInfo), p);
            var method = methodInfo.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            var call = Expression.Call(method, expr, sortExpr);
            var newQuery = source.Provider.CreateQuery<T>(call);
            return newQuery as IOrderedQueryable<T>;
        }

        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }

    }

    public class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;

        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;
            if (map.TryGetValue(p, out replacement))
            {
                p = replacement;
            }
            return base.VisitParameter(p);
        }
    }
}

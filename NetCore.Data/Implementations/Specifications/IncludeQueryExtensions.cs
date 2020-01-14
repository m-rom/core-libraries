using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NetCore.Data.Implementations.Specifications
{
    public static class IncludeQueryExtensions
    {
        public static IIncludeQuery<T, TNewProperty> Include<T, TPreviousProperty, TNewProperty>(
            this IIncludeQuery<T, TPreviousProperty> query,
            Expression<Func<T, TNewProperty>> selector)
        {
            query.Visitor.Visit(selector);

            var includeQuery = new IncludeQuery<T, TNewProperty>(query.PathMap);
            query.PathMap[includeQuery] = query.Visitor.Path;

            return includeQuery;
        }

        public static IIncludeQuery<T, TNewProperty> ThenInclude<T, TPreviousProperty, TNewProperty>(
            this IIncludeQuery<T, TPreviousProperty> query,
            Expression<Func<TPreviousProperty, TNewProperty>> selector)
        {
            query.Visitor.Visit(selector);

            // If the visitor did not generated a path, return a new IncludeQuery with an unmodified PathMap.
            if (string.IsNullOrEmpty(query.Visitor.Path))
            {
                return new IncludeQuery<T, TNewProperty>(query.PathMap);
            }

            var pathMap = query.PathMap;
            var existingPath = pathMap[query];
            pathMap.Remove(query);

            var includeQuery = new IncludeQuery<T, TNewProperty>(query.PathMap);
            pathMap[includeQuery] = $"{existingPath}.{query.Visitor.Path}";

            return includeQuery;
        }

        public static IIncludeQuery<T, TNewProperty> ThenInclude<T, TPreviousProperty, TNewProperty>(
            this IIncludeQuery<T, IEnumerable<TPreviousProperty>> query,
            Expression<Func<TPreviousProperty, TNewProperty>> selector)
        {
            query.Visitor.Visit(selector);

            // If the visitor did not generated a path, return a new IncludeQuery with an unmodified PathMap.
            if (string.IsNullOrEmpty(query.Visitor.Path))
            {
                return new IncludeQuery<T, TNewProperty>(query.PathMap);
            }

            var pathMap = query.PathMap;
            var existingPath = pathMap[query];
            pathMap.Remove(query);

            var includeQuery = new IncludeQuery<T, TNewProperty>(query.PathMap);
            pathMap[includeQuery] = $"{existingPath}.{query.Visitor.Path}";

            return includeQuery;
        }
    }
}

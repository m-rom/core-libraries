using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NetCore.Data.Implementations.Specifications
{
    public class IncludeAggregator<T>
    {
        public IncludeQuery<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            var visitor = new IncludeVisitor();
            visitor.Visit(selector);

            var pathMap = new Dictionary<IIncludeQuery, string>();
            var query = new IncludeQuery<T, TProperty>(pathMap);

            if (!string.IsNullOrEmpty(visitor.Path))
            {
                pathMap[query] = visitor.Path;
            }

            return query;
        }
    }
}

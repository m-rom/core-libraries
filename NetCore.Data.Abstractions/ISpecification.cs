using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NetCore.Data.Abstractions
{
    public interface ISpecification<T>
        where T : IEntity
    {
        Expression<Func<T, bool>> Criteria { get; }
        Expression<Func<T, object>> OrderBy { get; }
        Expression<Func<T, object>> OrderByDescending { get; }
        Expression<Func<T, object>> GroupBy { get; }

        public string PartitionKeyValue { get; set; }

        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }
    }
}

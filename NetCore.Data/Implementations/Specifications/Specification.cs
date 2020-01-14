using NetCore.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NetCore.Data.Implementations.Specifications
{
    internal class Specification<T> : BaseSpecification<T>
        where T : class, IEntity
    {
        public Specification()
            : base(b => b.Id != null)
        {
        }

        public Specification(Expression<Func<T, bool>> criteria)
            : base(criteria)
        {
        }
    }
}

using NetCore.Data.Abstractions;
using NetCore.Data.Implementations.Specifications;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NetCore.Data
{
    public class SpecificationFactory
    {
        public static ISpecification<T> Create<T>(Expression<Func<T, bool>> criteria = null) 
            where T : class, IEntity
        {
            if (criteria == null)
            {
                return new Specification<T>();
            }
            return new Specification<T>(criteria);
        }
    }
}

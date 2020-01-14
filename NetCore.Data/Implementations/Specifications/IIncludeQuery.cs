using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Data.Implementations.Specifications
{
    public interface IIncludeQuery
    {
        Dictionary<IIncludeQuery, string> PathMap { get; }
        IncludeVisitor Visitor { get; }
        HashSet<string> Paths { get; }
    }

    public interface IIncludeQuery<T, out TPreviousProperty> : IIncludeQuery
    {
    }
}

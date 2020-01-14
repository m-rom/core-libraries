using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCore.Data.Implementations.Specifications
{
    public class IncludeQuery<T, TPreviousProperty> : IIncludeQuery<T, TPreviousProperty>
    {
        public Dictionary<IIncludeQuery, string> PathMap { get; } = new Dictionary<IIncludeQuery, string>();
        public IncludeVisitor Visitor { get; } = new IncludeVisitor();

        public IncludeQuery(Dictionary<IIncludeQuery, string> pathMap)
        {
            PathMap = pathMap;
        }

        public HashSet<string> Paths => PathMap.Select(x => x.Value).ToHashSet();
    }
}

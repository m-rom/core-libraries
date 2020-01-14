using NetCore.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Data.Azure.Cosmos
{
    public interface ICosmosContainer<T>
        : IRepository<T>
        where T : IEntity
    {
    }
}

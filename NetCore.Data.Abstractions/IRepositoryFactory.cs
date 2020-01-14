using NetCore.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Data.Abstractions
{
    public interface IRepositoryFactory<T>
        where T : IEntity
    {
        IRepository<T> Create();
    }
}

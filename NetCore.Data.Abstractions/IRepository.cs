using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCore.Data.Abstractions
{
    public interface IRepository<T>
        where T : IEntity
    {
        Task<T> CreateAsync(T entity);

        Task DeleteAsync(T entity);

        Task<T> GetAsync(IEntityKey key);

        Task<T> UpdateAsync(T entity);

        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification);

        Task<int> CountAsync(ISpecification<T> specification);
    }
}

using Microsoft.Extensions.Logging;
using NetCore.Data.Abstractions;
using NetCore.Data.Azure.Cosmos.Implementations;
using System;

namespace NetCore.Data.Azure.Cosmos
{
    public class CosmosContainerRepositoryFactory<T>
        : IRepositoryFactory<T>
        where T : class, IEntity
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly CosmosContainerOptions<T> _options;

        public CosmosContainerRepositoryFactory(CosmosContainerOptions<T> options, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IRepository<T> Create()
        {
            return new CosmosContainer<T>(_options, _loggerFactory);
        }
    }
}

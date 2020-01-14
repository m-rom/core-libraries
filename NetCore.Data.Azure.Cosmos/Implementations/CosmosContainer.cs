using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using NetCore.Data.Abstractions;
using NetCore.Data.Implementations.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Data.Azure.Cosmos.Implementations
{
    class CosmosContainer<T>
        : ICosmosContainer<T>
        where T : class, IEntity
    {
        private readonly CosmosContainerOptions<T> _options;
        private readonly CosmosContainerProvider _containerProvider;
        private readonly ILogger _logger;

        protected Container Container
        {
            get
            {
                return _containerProvider.Container;
            }
        }

        public CosmosContainer(CosmosContainerOptions<T> options, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = loggerFactory.CreateLogger<CosmosContainer<T>>()
                ?? throw new ArgumentNullException(nameof(loggerFactory));

            ValidateOptions(options);
            _containerProvider = CosmosContainerProvider.CreateFromOptions(options);
        }

        public async Task<T> CreateAsync(T entity)
        {
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = $"{Guid.NewGuid()}";
            }

            var response = await Container.CreateItemAsync(entity);
            var rus = response.RequestCharge;
            var statusCode = response.StatusCode;
            TrackRequestCharge($"CreateAsync: {statusCode}", rus, "");
            return response.Resource;
        }

        public async Task DeleteAsync(T entity)
        {
            try
            {
                var key = GetKeyFromEntity(entity);
                var response = await Container.DeleteItemAsync<T>(entity.Id, new PartitionKey(key.PartitionKey));
                var rus = response.RequestCharge;
                var statusCode = response.StatusCode;
                TrackRequestCharge($"DeleteAsync: {statusCode}", rus, key.PartitionKey);
            }
            catch (CosmosException cex)
            {
                if (cex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return;
                }
                throw;
            }
        }

        public async Task<T> GetAsync(IEntityKey key)
        {
            try
            {
                var response = await Container.ReadItemAsync<T>(key.Id, new PartitionKey(key.PartitionKey));
                var rus = response.RequestCharge;
                var statusCode = response.StatusCode;
                TrackRequestCharge($"GetAsync: {statusCode}", rus, key.PartitionKey);
                return response.Resource;
            }
            catch (CosmosException cex)
            {
                if (cex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task<T> UpdateAsync(T entity)
        {
            var key = GetKeyFromEntity(entity);
            var response = await Container.UpsertItemAsync(entity, new PartitionKey(key.PartitionKey));
            var rus = response.RequestCharge;
            var statusCode = response.StatusCode;
            TrackRequestCharge($"UpdateAsync: {statusCode}", rus, key.PartitionKey);
            return response.Resource;
        }

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification)
        {
            var queryable = ApplySpecification(specification);
            var iterator = queryable.ToFeedIterator();

            var items = new List<T>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                var rus = response.RequestCharge;
                var statusCode = response.StatusCode;
                TrackRequestCharge($"QueryAsync: {statusCode}", rus, specification.PartitionKeyValue);
                items.AddRange(response);
            }
            return items;
        }

        public async Task<int> CountAsync(ISpecification<T> specification)
        {
            var queryable = ApplySpecification(specification);
            var response = await queryable.CountAsync();
            var rus = response.RequestCharge;
            var statusCode = response.StatusCode;
            TrackRequestCharge($"QueryAsync: {statusCode}", rus, specification.PartitionKeyValue);
            return response;
        }

        protected IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(CreateQuery(spec.PartitionKeyValue), spec);
        }

        private IQueryable<T> CreateQuery(string partitionKeyValue = "")
        {
            var options = new QueryRequestOptions();
            if (!string.IsNullOrEmpty(partitionKeyValue))
            {
                options = new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKeyValue)
                };
            }
            return Container.GetItemLinqQueryable<T>(true, null, options);
        }

        private static void ValidateOptions(ICosmosContainerOptions options)
        {
            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                throw new ArgumentNullException(nameof(options.ConnectionString));
            }
            if (string.IsNullOrEmpty(options.ContainerId))
            {
                throw new ArgumentNullException(nameof(options.ContainerId));
            }
            if (string.IsNullOrEmpty(options.DatabaseId))
            {
                throw new ArgumentNullException(nameof(options.DatabaseId));
            }
            if (options.DefaultTTLInSeconds.HasValue && options.DefaultTTLInSeconds < 1)
            {
                throw new NotSupportedException($"'DefaultTTLInSeconds' cannot be set lower than 1 second.");
            }
            if (options.RequestUnits < 400)
            {
                options.RequestUnits = 400;
            }
        }

        protected virtual IEntityKey GetKeyFromEntity(T value)
        {
            var id = value.Id;
            var partitionKeyValue = GetPartitionKeyValue(value);
            var ky = new EntityKey
            {
                PartitionKey = partitionKeyValue,
                Id = id
            };
            return ky;
        }

        private void TrackRequestCharge(string name, double requestCharge, string partitionKeyValue)
        {
            _logger.LogInformation("Executed: '{0}' [PartitionKey={1}, RequestCharge ={2}]", name, partitionKeyValue, requestCharge);
        }

        private string GetPartitionKeyValue(T entity)
        {
            if (string.IsNullOrEmpty(_options.PartitionKey))
            {
                return "";
            }
            try
            {
                var value = entity.GetType().GetProperty(_options.PartitionKey).GetValue(entity, null);
                if (value is string)
                {
                    return $"{value}";
                }
            }
            catch (Exception)
            {

            }
            return "";
        }

    }
}

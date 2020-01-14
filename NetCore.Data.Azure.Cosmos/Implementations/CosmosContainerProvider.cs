using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Data.Azure.Cosmos.Implementations
{
    class CosmosContainerProvider : IDisposable
    {
        private readonly static IDictionary<string, CosmosClient> _clientCache = new ConcurrentDictionary<string, CosmosClient>();

        private readonly ICosmosContainerOptions _options;
        private Lazy<CosmosClient> _client;

        private Container _container;
        public Container Container
        {
            get
            {
                if (_container == null)
                {
                    _container = _client.Value.GetContainer(_options.DatabaseId, _options.ContainerId);
                }
                return _container;
            }
        }

        public CosmosContainerProvider(ICosmosContainerOptions options)
        {
            _options = options
                ?? throw new ArgumentNullException(nameof(options));

            _client = SetupClient(_options);
        }

        private static Lazy<CosmosClient> SetupClient(ICosmosContainerOptions options)
        {
            var csb = ValidatInput(options.ConnectionString, options.DatabaseId, options.ContainerId);
            var cacheKey = $"{options.DatabaseId}_{options.UseGatewayMode}_{ToSHA256(csb.Key)}";
            return new Lazy<CosmosClient>(() =>
            {
                CosmosClient client = null;
                bool fromCache = false;
                if (_clientCache.ContainsKey(cacheKey))
                {
                    client = _clientCache[cacheKey];
                    if (client != null)
                    {
                        fromCache = true;
                    }
                }

                if (!fromCache)
                {
                    var builder = new CosmosClientBuilder(options.ConnectionString);
                    builder
                        .WithThrottlingRetryOptions(TimeSpan.FromSeconds(30), 10);

                    if (options.UseGatewayMode)
                    {
                        builder.WithConnectionModeGateway(options.ConcurrentConnections);
                    }

                    foreach (var region in options.AzureRegions ?? new List<string>())
                    {
                        builder.WithApplicationRegion(region);
                    }
                    client = builder.Build();

                    _clientCache[cacheKey] = client;
                }

                CreateDatabaseIfNotExistsAsync(client, options).GetAwaiter().GetResult();
                return client;
            });
        }

        public static async Task CreateDatabaseIfNotExistsAsync(CosmosClient client, ICosmosContainerOptions options)
        {
            DatabaseResponse databaseCreationResponse;
            if (options.ProvisionOnDatabaseLevel)
            {
                databaseCreationResponse = await client.CreateDatabaseIfNotExistsAsync(options.DatabaseId, options.RequestUnits);
            }
            else
            {
                databaseCreationResponse = await client.CreateDatabaseIfNotExistsAsync(options.DatabaseId);
            }

            if (databaseCreationResponse.StatusCode == System.Net.HttpStatusCode.Accepted
                || databaseCreationResponse.StatusCode == System.Net.HttpStatusCode.OK
                || databaseCreationResponse.StatusCode == System.Net.HttpStatusCode.Created)
            {
                await CreateContainerIfNotExistsAsync(client, options);
            }
            else
            {
                throw new NotSupportedException($"Database '{options.DatabaseId}' cannot be created. Wrong Url or Key? ({databaseCreationResponse.StatusCode})");
            }
        }

        public static async Task CreateContainerIfNotExistsAsync(CosmosClient client, ICosmosContainerOptions options)
        {
            var partitionKey = options.PartitionKey;
            if (!string.IsNullOrEmpty(partitionKey))
            {
                if (!partitionKey.StartsWith("/"))
                {
                    partitionKey = "/" + partitionKey;
                }
            }

            var database = client.GetDatabase(options.DatabaseId);

            var definition = database.DefineContainer(options.ContainerId, partitionKey);
            if (options.DefaultTTLInSeconds.HasValue)
            {
                definition.WithDefaultTimeToLive(options.DefaultTTLInSeconds.Value);
            }
            definition.Build();
            await definition.CreateIfNotExistsAsync(options.ProvisionOnDatabaseLevel ? (int?)null : options.RequestUnits);
            await Task.Delay(500);
        }

        private static CosmosResourceBuilder ValidatInput(string connectionString, string database, string collection)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            var csb = new CosmosResourceBuilder(connectionString);
            if (csb == null)
            {
                throw new ArgumentNullException(nameof(csb));
            }
            if (csb.Endpoint == null)
            {
                throw new ArgumentNullException(nameof(csb.Endpoint));
            }
            if (string.IsNullOrEmpty(csb.Key))
            {
                throw new ArgumentNullException(nameof(csb.Key));
            }
            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentNullException(nameof(database));
            }
            if (string.IsNullOrEmpty(collection))
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return csb;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_client != null)
                {
                    _client.Value.Dispose();
                    _client = null;
                }
            }
        }

        public static CosmosContainerProvider CreateFromOptions<TEntity>(ICosmosContainerOptions<TEntity> options)
        {
            return new CosmosContainerProvider(options);
        }

        public static string ToSHA256(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}

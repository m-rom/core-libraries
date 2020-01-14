using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCore.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Data.Azure.Cosmos.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosRepository<T>(this IServiceCollection collection, Action<CosmosContainerOptions<T>, IConfiguration> configurationFactory)
            where T : class, IEntity
        {
            collection.AddSingleton(sp =>
            {
                var options = new CosmosContainerOptions<T>();
                var configuration = sp.GetRequiredService<IConfiguration>();
                configurationFactory(options, configuration);
                return options;
            });
            collection.AddSingleton<IRepositoryFactory<T>, CosmosContainerRepositoryFactory<T>>();
            collection.AddSingleton<IRepository<T>>(sp => sp.GetRequiredService<IRepositoryFactory<T>>().Create());
            return collection;
        }
    }
}

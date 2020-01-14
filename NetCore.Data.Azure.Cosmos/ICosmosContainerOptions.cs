using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Data.Azure.Cosmos
{
    public interface ICosmosContainerOptions
    {
        string ConnectionString { get; set; }
        string DatabaseId { get; set; }
        string ContainerId { get; set; }
        string PartitionKey { get; set; }
        IList<string> AzureRegions { get; set; }
        int RequestUnits { get; set; }
        bool ProvisionOnDatabaseLevel { get; set; }
        int? DefaultTTLInSeconds { get; set; }
        int ConcurrentConnections { get; set; }
        bool UseGatewayMode { get; set; }
    }

    public interface ICosmosContainerOptions<T>
        : ICosmosContainerOptions
    {
    }
}

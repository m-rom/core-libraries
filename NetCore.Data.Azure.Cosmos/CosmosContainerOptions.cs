using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Data.Azure.Cosmos
{
    public class CosmosContainerOptions<T> : ICosmosContainerOptions<T>
    {
        public string ConnectionString { get; set; }
        public string DatabaseId { get; set; }
        public string ContainerId { get; set; }
        public string PartitionKey { get; set; }
        public IList<string> AzureRegions { get; set; } = new List<string>();
        public int RequestUnits { get; set; } = 400;
        public bool ProvisionOnDatabaseLevel { get; set; } = false;
        public int? DefaultTTLInSeconds { get; set; }

        public int ConcurrentConnections { get; set; } = 300;
        public bool UseGatewayMode { get; set; } = false;
    }
}

using System;

namespace NetCore.Data.Abstractions
{
    public interface IEntityKey
    {
        string Id { get; set; }
        string PartitionKey { get; set; }
    }

    public class EntityKey : IEntityKey
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
    }
}

using NetCore.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Web.Models
{
    public class Item : Entity
    {
        public string PartitionKey { get; set; }
    }
}

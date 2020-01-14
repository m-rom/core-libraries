using Newtonsoft.Json;
using System;

namespace NetCore.Data.Abstractions
{
    public class Entity : IEntity
    {
#pragma warning disable IDE1006 // Naming Styles
        public string id;
#pragma warning restore IDE1006 // Naming Styles

        public string Id
        {
            get { return id; }
            set { id = value; }
        }
    }
}

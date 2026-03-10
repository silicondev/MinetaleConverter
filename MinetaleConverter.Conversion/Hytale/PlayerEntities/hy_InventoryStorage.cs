using MinetaleConverter.Base.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_InventoryStorage
    {
        public string Id { get; set; } = "Simple";
        public int Capacity { get; set; }
        [JsonConverter(typeof(BsonDocumentToDictionary<int, hy_Item>))]
        public Dictionary<int, hy_Item> Items { get; set; } = new Dictionary<int, hy_Item>();
    }
}

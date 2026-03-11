using MinetaleConverter.Base.Logic.Serialization.BSON.Attributes;
using Newtonsoft.Json;

namespace MinetaleConverter.Hytale.Models
{
    public class hy_BlockComponentChunk
    {
        [JsonConverter(typeof(BsonDocumentToDictionary<int, hy_BlockComponentItem>))]
        public Dictionary<int, hy_BlockComponentItem> BlockComponents { get; set; }
    }
}

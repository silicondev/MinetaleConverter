using MinetaleConverter.Base.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_Binary
    {
        //[JsonConverter(typeof(BsonOptionalProperty<int>))]
        public int? Version { get; set; }
        //[JsonProperty(ItemConverterType = typeof(StringToByteArray))]
        //public byte[] Data { get; set; } = new byte[0];
        public byte[]? Data { get; set; }
    }
}

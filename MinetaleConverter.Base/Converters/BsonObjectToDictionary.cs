using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Converters
{
    public class BsonObjectToDictionary<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>
    {
        public override Dictionary<TKey, TValue>? ReadJson(JsonReader reader, Type objectType, Dictionary<TKey, TValue>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            object? value = reader.Value;
            return new Dictionary<TKey, TValue>();
        }

        public override void WriteJson(JsonWriter writer, Dictionary<TKey, TValue>? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

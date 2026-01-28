using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Converters
{
    public class BsonOptionalClass<T> : JsonConverter<T>
    {
        public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
                return (T)Convert.ChangeType(reader.Value, typeof(T));
            return default;
        }

        public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
            else
                serializer.Serialize(writer, value, value.GetType());
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Converters
{
    public class StringToByteArray : JsonConverter<byte[]>
    {
        public override byte[]? ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string? str = reader.Value?.ToString();
            if (str == null)
                return null;
            return Encoding.UTF8.GetBytes(str);
        }

        public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var b in value)
            {
                writer.WriteValue(b);
            }
            writer.WriteEndArray();
        }
    }
}

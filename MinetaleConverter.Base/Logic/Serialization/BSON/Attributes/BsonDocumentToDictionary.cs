using Newtonsoft.Json;

namespace MinetaleConverter.Base.Logic.Serialization.BSON.Attributes
{
    public class BsonDocumentToDictionary<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>> where TKey : notnull
    {
        public override Dictionary<TKey, TValue>? ReadJson(JsonReader reader, Type objectType, Dictionary<TKey, TValue>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            bool firstPass = true;
            var dict = new Dictionary<TKey, TValue>();

            while (true)
            {
                reader.Read();

                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.Value == null)
                {
                    if (firstPass)
                        return null;
                    else
                        continue;
                }
                firstPass = false;

                TKey key = (TKey)Convert.ChangeType(reader.Value, typeof(TKey));

                reader.Read();

                TValue? val = serializer.Deserialize<TValue>(reader);

                if (val == null)
                    continue;

                dict.Add(key, val);
            }

            return dict;
        }

        public override void WriteJson(JsonWriter writer, Dictionary<TKey, TValue>? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value != null)
            {
                foreach (var kvp in value)
                {
                    writer.WritePropertyName(kvp.Key.ToString()!);
                    serializer.Serialize(writer, kvp.Value);
                }
            }
            writer.WriteEndObject();
        }
    }
}

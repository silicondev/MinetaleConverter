using MinetaleConverter.Conversion.Hytale.WorldEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.Converters
{
    public class SectionListConverter : JsonConverter<IList<hy_Section>>
    {
        public override IList<hy_Section>? ReadJson(JsonReader reader, Type objectType, IList<hy_Section>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            object? data = reader.Value;
            var list = (IList<hy_Section>?)reader.Value;
            if (list == null)
                return null;
            for (int i = 0; i < list.Count(); i++)
                list[i].Id = i;
            return list;
        }

        public override void WriteJson(JsonWriter writer, IList<hy_Section>? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

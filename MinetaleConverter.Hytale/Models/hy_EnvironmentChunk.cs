using MinetaleConverter.Hytale.Logic;
using Newtonsoft.Json;

namespace MinetaleConverter.Hytale.Models
{
    public class hy_EnvironmentChunk
    {
        public byte[] Data { get; set; }

        [JsonIgnore]
        public hy_BiomePalette BiomePalette { get; set; } = new hy_BiomePalette();
    }
}

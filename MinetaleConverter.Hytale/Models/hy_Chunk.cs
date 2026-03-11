using MinetaleConverter.Base.Models.Interfaces;
using Newtonsoft.Json;

namespace MinetaleConverter.Hytale.Models
{
    public class hy_Chunk : IChunk
    {
        public hy_ChunkComponents Components = new hy_ChunkComponents();

        [JsonIgnore]
        public int xPos { get; set; }

        [JsonIgnore]
        public int zPos { get; set; }

        public string GetBiome(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public string GetBlock(int x, int y, int z)
        {
            int sectionId = (int)Math.Floor(y / 32d);
            var section = Components.ChunkColumn.Sections[sectionId];
            return section?.GetBlock(x, y - sectionId * 32, z) ?? "Empty";
        }

        public void SetBlock(string blockId, int x, int y, int z)
        {
            int sectionId = (int)Math.Floor(y / 32d);
            var section = Components.ChunkColumn.Sections[sectionId];
            if (section == null)
                return;
            section.SetBlock(blockId, x, y - sectionId * 32, z);
        }
    }
}

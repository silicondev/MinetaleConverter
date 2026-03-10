using MinetaleConverter.Conversion.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_Chunk : IChunk
    {

        public hy_ChunkComponents Components { get; set; } = new hy_ChunkComponents();
        [JsonIgnore]
        public int xPos { get; set; }
        [JsonIgnore]
        public int zPos { get; set; }

        public hy_Chunk()
        {
            //for (int i = 0; i < 10; i++)
            //{
            //    Components.ChunkColumn.Sections.Add(new hy_Section());
            //}
        }

        public hy_Chunk(int x, int z) : this()
        {
            xPos = x;
            zPos = z;
        }

        public void Populate(int x, int z)
        {
            xPos = x;
            zPos = z;

            for (int i = 0; i < Components.ChunkColumn.Sections.Count(); i++)
            {
                //Components.ChunkColumn.Sections[i].Id = i;
            }
        }

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

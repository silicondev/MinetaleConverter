using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Interfaces;
using MinetaleConverter.Conversion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft.WorldEntities
{
    public class mc_Chunk : IChunk
    {
        public int DataVersion { get; internal set; }
        public int xPos { get; internal set; }
        public int zPos { get; internal set; }
        public int yPos { get; internal set; }
        public string Status { get; internal set; }
        public long LastUpdate { get; internal set; }
        [NbtProperty("sections")]
        public List<mc_Section> Sections { get; internal set; } = new List<mc_Section>();
        public mc_Heightmaps Heightmaps { get; internal set; } = new mc_Heightmaps();
        internal byte[] data { get; set; }
        [NbtProperty("fluid_ticks")]
        public List<mc_TileTick> FluidTicks { get; internal set; } = new List<mc_TileTick>();
        [NbtProperty("block_ticks")]
        public List<mc_TileTick> BlockTicks { get; internal set; } = new List<mc_TileTick>();
        public long InhabitedTime { get; internal set; }
        public byte[] NbtData { get; internal set; }

        public string GetBlock(int x, int y, int z) => GetBlockResource(x, y, z)?.Name ?? "minecraft:air";
        public string GetBiome(int x, int y, int z) => GetBiomeResource(x, y, z)?.Name ?? "minecraft:air";

        public mc_Resource? GetBlockResource(int x, int y, int z)
        {
            int sectionId = (int)Math.Floor(y / 16d);

            var section = Sections.FirstOrDefault(x => x.Y == sectionId);
            if (section == null)
                return null;

            return section.GetBlock(x, y - sectionId * 16, z);
        }

        public mc_Resource? GetBiomeResource(int x, int y, int z)
        {
            int sectionId = (int)(y / 16d);

            var section = Sections.FirstOrDefault(x => x.Y == sectionId);
            if (section == null)
                return null;

            return section.GetBiome(x, y - sectionId * 16, z);
        }

        public void SetBlock(string blockId, int x, int y, int z)
        {
            throw new NotImplementedException();
        }
    }
}

using MinetaleConverter.Base.Logic.Serialization.NBT.Attributes;
using MinetaleConverter.Base.Models.Interfaces;
using MinetaleConverter.Minecraft.Logic;
using System;

namespace MinetaleConverter.Minecraft.Models
{
    public class mc_Chunk : IChunk
    {
        public int Size => 16;
        public int DataVersion { get; set; }
        public int xPos { get; set; }
        public int zPos { get; set; }
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

        public string? GetBiome(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public string? GetBlock(int x, int y, int z)
        {
            int sectionId = (int)Math.Floor(y / 16d);
            var section = Sections.FirstOrDefault(x => x.Y == sectionId);
            if (section == null)
                return null;
            return section?.GetBlock(x, y - sectionId * 16, z)?.Name;
        }

        public void SetBlock(string blockId, int x, int y, int z)
        {
            throw new NotImplementedException();
        }
    }
}

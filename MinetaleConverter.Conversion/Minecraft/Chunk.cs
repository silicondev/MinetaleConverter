using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class Chunk
    {
        public int DataVersion { get; internal set; }
        public int xPos { get; internal set; }
        public int zPos { get; internal set; }
        public int yPos { get; internal set; }
        public string Status { get; internal set; }
        public long LastUpdate { get; internal set; }
        [NbtProperty("sections")]
        public List<Section> Sections { get; internal set; } = new List<Section>();
        public Heightmaps Heightmaps { get; internal set; } = new Heightmaps();
        internal byte[] data { get; set; }
        [NbtProperty("fluid_ticks")]
        public List<TileTick> FluidTicks { get; internal set; } = new List<TileTick>();
        [NbtProperty("block_ticks")]
        public List<TileTick> BlockTicks { get; internal set; } = new List<TileTick>();
        public long InhabitedTime { get; internal set; }
    }
}

using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class Section
    {
        public byte Y { get; internal set; }
        [NbtProperty("block_states")]
        [NbtTagType(TagType.Compound)]
        public State BlockStates { get; internal set; }
        [NbtProperty("biomes")]
        [NbtTagType(TagType.Compound)]
        public State Biomes { get; internal set; }
        public byte[] BlockLight { get; internal set; }
        public byte[] SkyLight { get; internal set; }
    }
}

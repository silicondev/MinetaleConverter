using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Converters;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class Section
    {
        [NbtConverter(typeof(SignedByteToInt))]
        public int Y { get; internal set; }
        [NbtProperty("block_states")]
        [NbtTagType(TagType.Compound)]
        public State BlockStates { get; internal set; }
        [NbtProperty("biomes")]
        [NbtTagType(TagType.Compound)]
        public State Biomes { get; internal set; }
        public byte[] BlockLight { get; internal set; }
        public byte[] SkyLight { get; internal set; }

        public Palette? GetBlock(int x, int y, int z) =>
            BlockStates.GetPalette((y * 16 * 16) + (z * 16) + x, 4);

        public Palette? GetBiome(int x, int y, int z) =>
            Biomes.GetPalette(((y / 4) * 4 * 4) + ((z / 4) * 4) + (x / 4));
    }
}

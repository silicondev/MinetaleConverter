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

        public Palette? GetBlock(int x, int y, int z)
        {
            if (BlockStates.Palettes.Count() == 1)
                return BlockStates.Palettes[0];
            else if (BlockStates.Palettes.Count() == 0)
                return null;

                // Build position index
                int posIndex = (y * 16 * 16) + (z * 16) + x;

            // Get bit sizing
            int paletteCount = BlockStates.Palettes.Count();
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            if (bitCount < 4)
                bitCount = 4;
            var divCount = Math.Floor(64d / bitCount);

            // Get indices of data array both in long array and inside long bits itself
            int dataIndex = (int)Math.Floor(posIndex / divCount);
            int longIndex = posIndex - (dataIndex * (int)divCount);

            // Get the correct long and the bits inside it
            var bits = BitConverter.GetBytes(BlockStates.Data[dataIndex]).Select(x => x.GetBits()).Combine();
            int ind = longIndex * bitCount;
            var intBits = bits[ind..(ind + bitCount)];

            // Convert to int
            int result = 0;
            for (int i = 0; i < bitCount; i++)
                result |= intBits[i] ? (1 << i) : 0;

            // Get palette at index
            return BlockStates.Palettes[result];
        }
    }
}

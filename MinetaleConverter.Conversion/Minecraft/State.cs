using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Interfaces;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class State
    {
        [NbtProperty("data")]
        public long[] Data { get; internal set; }
        [NbtProperty("palette")]
        [NbtConverter(typeof(PrimitivePaletteConverter))]
        public List<Palette> Palettes { get; internal set; } = new List<Palette>();

        public Palette? GetPalette(int index, int minBit = 1)
        {
            if (Palettes.Count() == 0)
                return null;
            else if (Palettes.Count() == 1)
                return Palettes[0];

            int paletteCount = Palettes.Count();
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            if (bitCount < minBit)
                bitCount = minBit;
            var divCount = Math.Floor(64d / bitCount);

            int dataIndex = (int)Math.Floor(index / divCount);
            int longIndex = index - (dataIndex * (int)divCount);

            var bits = BitConverter.GetBytes(Data[dataIndex]).Select(x => x.GetBits()).Combine();
            int ind = longIndex * bitCount;
            var intBits = bits[ind..(ind + bitCount)];

            int result = 0;
            for (int i = 0; i < bitCount; i++)
                result |= intBits[i] ? (1 << i) : 0;

            return Palettes[result];
        }
    }

    internal class PrimitivePaletteConverter : INbtConverter
    {
        public object? Convert(string value)
        {
            var palette = new Palette();
            palette.Name = value.Replace("\"", "");
            return palette;
        }
    }
}

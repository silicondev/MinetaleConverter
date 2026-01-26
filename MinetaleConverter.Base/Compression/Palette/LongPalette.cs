using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Interfaces;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Compression.Palette
{
    public class LongPalette<T> : Palette<T>
    {
        public long[] Data { get; set; } = [];

        public LongPalette(IEnumerable<T> palette, long[] data, int minBits = 1) : base(palette, minBits)
        {
            Data = data;
        }

        public LongPalette(int minBits = 1) : base(minBits)
        {
            
        }

        public override T? GetAtIndex(int index)
        {
            if (Palettes.Count() == 0)
                return default;
            else if (Palettes.Count() == 1)
                return Palettes.First();

            int paletteCount = Palettes.Count();
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            if (bitCount < _minBits)
                bitCount = _minBits;
            var divCount = Math.Floor(64d / bitCount);

            int dataIndex = (int)Math.Floor(index / divCount);
            int longIndex = index - dataIndex * (int)divCount;

            var bits = BitConverter.GetBytes(Data[dataIndex]).Select(x => x.GetBits()).Combine();
            int ind = longIndex * bitCount;
            var intBits = bits[ind..(ind + bitCount)];

            return Palettes[intBits.ToInt()];
        }

        public override List<T> Decompress(int payloadSize = -1)
        {
            throw new NotImplementedException();
        }
    }
}

using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Converters;
using MinetaleConverter.Base.Interfaces;
using Newtonsoft.Json;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Compression.Palette
{
    public class BytePalette<T> : Palette<T>
    {
        public byte[] Data { get; set; } = [];

        public BytePalette(IEnumerable<T> palette, byte[] data, int minBits = 1) : base(palette, minBits)
        {
            Data = data;
        }

        public BytePalette(int minBits = 1) : base(minBits)
        {
            
        }

        public BytePalette() : base(1)
        {
            
        }

        public override T? GetAtIndex(int index)
        {
            if (PalettesDict.Count() == 0)
                return default;
            else if (PalettesDict.Count() == 1)
                return PalettesDict.Values.First();

            int paletteCount = PalettesDict.Count();
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            if (bitCount < _minBits)
                bitCount = _minBits;

            var intBits = new bool[bitCount];
            for (int j = index * bitCount; j < (index + 1) * bitCount; j++)
            {
                int byteIndex = (int)Math.Floor(j / 8d);
                int bitIndex = j - (byteIndex * 8);
                byte b = Data[byteIndex];
                int k = j - (index * bitCount);
                intBits[k] = (b & (1 << bitIndex)) != 0;
            }
            int paletteIndex = intBits.ToInt();
            return PalettesDict[paletteIndex];
        }

        public override List<T> Decompress(int payloadSize = -1)
        {
            var list = new List<T>();
            if (PalettesDict.Count() == 0)
                return list;

            int paletteCount = PalettesDict.Count();
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            if (bitCount < _minBits)
                bitCount = _minBits;

            if (payloadSize == -1)
                payloadSize = Data.Length / bitCount;

            for (int i = 0; i < payloadSize; i++)
            {
                var intBits = new bool[bitCount];
                for (int j = i * bitCount; j < (i + 1) * bitCount; j++)
                {
                    int byteIndex = (int)Math.Floor(j / 8d);
                    int bitIndex = j - (byteIndex * 8);
                    byte b = Data[byteIndex];
                    int k = j - (i * bitCount);
                    intBits[k] = (b & (1 << bitIndex)) != 0;
                }
                int index = intBits.ToInt();
                list.Add(PalettesDict[index]);
            }

            var dist = list.Distinct();
            foreach (var p in PalettesDict.Values)
            {
                if (!dist.Contains(p))
                {
                    //Huh???
                    int k = 0;
                }
            }

            return list;
        }
    }
}
